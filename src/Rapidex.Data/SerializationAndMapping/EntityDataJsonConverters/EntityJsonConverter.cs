using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Rapidex.Data.Exceptions;
using Rapidex.Data.Metadata;
using Rapidex.Data.SerializationAndMapping.MetadataImplementers;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;

internal class EntityJsonConverter : JsonConverter<IEntity>
{
    const int OPERATION_FLAG_NONE = 0;
    const int OPERATION_FLAG_ISNEW = 1;
    const int OPERATION_FLAG_ISDELETED = 2;
    const int OPERATION_FLAG_UPDATE = 4;

    const int OPERATION_FLAG_ADD = 10;
    const int OPERATION_FLAG_REMOVE = 11;


    private static readonly AsyncLocal<bool?> UseNestedEntitiesStore = new();

    internal static bool? UseNestedEntities
    {
        get => UseNestedEntitiesStore.Value;
        set => UseNestedEntitiesStore.Value = value;
    }

    public override IEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        EntityDataJsonConverter.DeserializationContext.NotNull("Invalid deserialization context");

        JsonNode node = JsonNode.Parse(ref reader, JsonHelper.DefaultJsonNodeOptions);
        JsonObject root = node.AsObject();

        string entityTypeName = this.GetRequiredEntityType(root);
        int operation = this.GetRequiredOperation(root);

        bool idRequired = operation != OPERATION_FLAG_ISNEW;
        long? entityId = this.GetEntityId(root, idRequired);

        var em = EntityDataJsonConverter.DeserializationContext.Scope.ParentDbScope.Metadata.Get(entityTypeName)
             .NotNull($"Entity type '{entityTypeName}' not found in metadata.");

        EntityDataJsonConverter.DeserializationContext.CurrentEntityMetadata = em;

        try
        {

            IEntity entity;

            switch (operation)
            {
                case OPERATION_FLAG_NONE:
                    entity = Database.EntityFactory.Create(em, EntityDataJsonConverter.DeserializationContext.Scope, false);
                    break;
                case OPERATION_FLAG_ISNEW:
                    entity = Database.EntityFactory.Create(em, EntityDataJsonConverter.DeserializationContext.Scope, true);
                    if (entityId.HasValue && !entityId.Value.IsEmptyId())
                        entity.SetId(entityId.Value);
                    break;
                case OPERATION_FLAG_ISDELETED:
                    entity = Database.EntityFactory.CreatePartial(em, EntityDataJsonConverter.DeserializationContext.Scope, false, true);
                    if (entityId.HasValue)
                        entity.SetId(entityId.Value);
                    break;
                case OPERATION_FLAG_UPDATE:
                    entity = Database.EntityFactory.CreatePartial(em, EntityDataJsonConverter.DeserializationContext.Scope, false, false);
                    if (entityId.HasValue)
                        entity.SetId(entityId.Value);
                    break;
                default:
                    throw new JsonException($"Unsupported operation flag '{operation}'");
            }

            EntityDataJsonConverter.DeserializationContext.CurrentEntity = entity;

            if (root.TryGetPropertyValue("Values", out JsonNode? valuesElement))
            {
                this.SetEntityFields(em, entity, valuesElement.AsObject(), options);
            }
            else
            {
                //We support direct field definitions on root level as well
                this.SetEntityFields(em, entity, root, options, skipReservedFields: true);
            }

            entity.EnsureDataTypeInitialization();

            return entity;
        }
        finally
        {
            EntityDataJsonConverter.DeserializationContext.CurrentEntityMetadata = null;
            EntityDataJsonConverter.DeserializationContext.CurrentEntity = null;
        }

    }

    private string GetRequiredEntityType(JsonObject root)
    {
        if (root.TryGetPropertyValue("Entity", out JsonNode? entityNode) ||
           root.TryGetPropertyValue(DatabaseConstants.DATA_FIELD_TYPENAME, out entityNode))
        {
            string? entityTypeName = entityNode?.GetValue<string>();
            if (!string.IsNullOrEmpty(entityTypeName))
                return entityTypeName;
        }

        throw new JsonException("Required field '_entity' or 'Entity' not found in JSON");
    }

    private int GetRequiredOperation(JsonObject root)
    {
        if (root.TryGetPropertyValue("type", out JsonNode? operationNode) ||
            root.TryGetPropertyValue(DatabaseConstants.DATA_FIELD_OPERATION, out operationNode))
        {
            string operationTypeName = operationNode?.GetValue<string>();
            if (!string.IsNullOrEmpty(operationTypeName))
            {
                switch (operationTypeName.ToLowerInvariant())
                {
                    case "new":
                    case "insert":
                        return OPERATION_FLAG_ISNEW;
                    case "delete":
                        return OPERATION_FLAG_ISDELETED;
                    case "update":
                        return OPERATION_FLAG_UPDATE;
                    case "none":
                        return OPERATION_FLAG_NONE;
                    case "add":
                        return OPERATION_FLAG_ADD;
                    case "remove":
                    case "rem":
                        return OPERATION_FLAG_REMOVE;

                    default:
                        throw new JsonException($"Unknown operation type '{operationTypeName}'");
                }
            }
        }
        return OPERATION_FLAG_NONE;
    }

    private long? GetEntityId(JsonObject root, bool required)
    {
        if (root.TryGetPropertyValue("Values", out JsonNode? valuesElement))
        {
            if (valuesElement.AsObject().TryGetPropertyValue("Id", out JsonNode? idElement) ||
                valuesElement.AsObject().TryGetPropertyValue(DatabaseConstants.FIELD_ID, out idElement))
            {
                return idElement.GetValue<long>();
            }
        }

        if (root.TryGetPropertyValue("Id", out JsonNode? directIdElement) ||
            root.TryGetPropertyValue(DatabaseConstants.FIELD_ID, out directIdElement))
        {
            return directIdElement.GetValue<long>();
        }

        if (required)
            throw new JsonException("Required field 'Id' or '_id' not found in JSON");

        return null;
    }

    private void SetEntityFields(IDbEntityMetadata em, IEntity entity, JsonObject sourceElement, JsonSerializerOptions options, bool skipReservedFields = false)
    {
        var reservedFields = new HashSet<string> {
            "Entity", DatabaseConstants.DATA_FIELD_TYPENAME,
            "Id", DatabaseConstants.FIELD_ID,
            DatabaseConstants.FIELD_VERSION,
            "IsNew", "IsDeleted"
        };

        // Iterate through metadata fields to ensure we only process defined fields
        foreach (var fieldMetadata in em.Fields.Values)
        {
            if (skipReservedFields && reservedFields.Contains(fieldMetadata.Name))
                continue;

            if (!sourceElement.TryGetPropertyValue(fieldMetadata.Name, out JsonNode? fieldElement))
                continue;

            try
            {
                if (fieldElement == null)
                {
                    entity.SetValue(fieldMetadata.Name, null);
                    continue;
                }

                object value = this.DeserializeFieldValue(fieldElement, fieldMetadata, options);
                entity.SetValue(fieldMetadata.Name, value);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error deserializing field '{fieldMetadata.Name}': {ex.Message}", ex);
            }
        }

        // Set version if present
        this.SetEntityVersion(entity, sourceElement);
    }

    private object DeserializeFieldValue(JsonNode fieldElement, IDbFieldMetadata fieldMetadata, JsonSerializerOptions options)
    {
        if (fieldElement.GetValueKind() == JsonValueKind.Null)
            return null;

        EntityDataJsonConverter.DeserializationContext.CurrentFieldMetadata = fieldMetadata;

        try
        {
            return JsonSerializer.Deserialize(fieldElement, fieldMetadata.Type, options);
        }
        finally
        {
            EntityDataJsonConverter.DeserializationContext.CurrentFieldMetadata = null;
        }
    }

    private void SetEntityVersion(IEntity entity, JsonObject sourceElement)
    {
        if (sourceElement.TryGetPropertyValue(DatabaseConstants.FIELD_VERSION, out JsonNode? versionElement))
        {
            entity.DbVersion = versionElement.GetValue<int>();
        }
    }

    public override void Write(Utf8JsonWriter writer, IEntity entity, JsonSerializerOptions options)
    {
        try
        {
            if (UseNestedEntities == null)
            {
                UseNestedEntities = true;
            }

            if (entity == null)
            {
                writer.WriteNullValue();
                return;
            }

            entity.EnsureDataTypeInitialization();

            IDbEntityMetadata em = entity.GetMetadata();

            long id = entity.GetId().As<long>();

            writer.WriteStartObject();
            writer.WriteString(DatabaseConstants.DATA_FIELD_TYPENAME, em.Name);
            writer.WriteString(DatabaseConstants.DATA_FIELD_CAPTION, entity.Caption());
            writer.WriteNumber(DatabaseConstants.DATA_FIELD_ID, id);

            writer.WriteNumber(DatabaseConstants.FIELD_ID, id);
            writer.WriteNumber(DatabaseConstants.FIELD_VERSION, entity.DbVersion);

            writer.WritePropertyName("Values");
            writer.WriteStartObject();

            foreach (IDbFieldMetadata fm in em.Fields.Values)
            {
                writer.WritePropertyName(fm.Name);

                object upperValue = fm.ValueGetterUpper(entity, fm.Name);
                if (upperValue.IsNullOrEmpty())
                {
                    writer.WriteNullValue();
                }
                else
                {
                    JsonSerializer.Serialize(writer, upperValue, upperValue.GetType(), options);
                }
            }

            writer.WriteEndObject(); // values
            writer.WriteEndObject(); // entity
        }
        finally
        {
            UseNestedEntities = null;
        }
    }

    public static void Register()
    {
        EntityJsonConverter conv = new EntityJsonConverter();
        JsonHelper.Register(conv);
    }
}
