using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    [ThreadStatic]
    internal static bool? useNestedEntities = null;

    public override IEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        EntityDataJsonConverter.DeserializationContext.NotNull("Invalid deserialization context");

        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;

            string entityTypeName = this.GetRequiredEntityType(root);
            int operation = this.GetRequiredOperation(root);

            bool idRequired = operation != OPERATION_FLAG_ISNEW;
            long? entityId = this.GetEntityId(root, idRequired);

            var em = EntityDataJsonConverter.DeserializationContext.ParentDbScope.Metadata.Get(entityTypeName)
                 .NotNull($"Entity type '{entityTypeName}' not found in metadata.");

            IEntity entity;

            switch (operation)
            {
                case OPERATION_FLAG_NONE:
                    entity = Database.EntityFactory.Create(em, EntityDataJsonConverter.DeserializationContext, false);
                    break;
                case OPERATION_FLAG_ISNEW:
                    entity = Database.EntityFactory.Create(em, EntityDataJsonConverter.DeserializationContext, true);
                    break;
                case OPERATION_FLAG_ISDELETED:
                    entity = Database.EntityFactory.CreatePartial(em, EntityDataJsonConverter.DeserializationContext,  false, true);
                    if(entityId.HasValue)
                        entity.SetId(entityId.Value);
                    break;
                case OPERATION_FLAG_UPDATE:
                    entity = Database.EntityFactory.CreatePartial(em, EntityDataJsonConverter.DeserializationContext, false, false);
                    if (entityId.HasValue)
                        entity.SetId(entityId.Value);
                    break;
                default:
                    throw new JsonException($"Unsupported operation flag '{operation}'");
            }


            if (root.TryGetProperty("Values", out JsonElement valuesElement))
            {
                this.SetEntityFields(em, entity, valuesElement, options);
            }
            else
            {
                //We support direct field definitions on root level as well
                this.SetEntityFields(em, entity, root, options, skipReservedFields: true);
            }

            return entity;
        }
    }

    private string GetRequiredEntityType(JsonElement root)
    {
        if (root.TryGetProperty("Entity", out JsonElement entityElement) ||
            root.TryGetProperty(DatabaseConstants.DATA_FIELD_TYPENAME, out entityElement))
        {
            string entityTypeName = entityElement.GetString();
            if (!string.IsNullOrEmpty(entityTypeName))
                return entityTypeName;
        }
        throw new JsonException("Required field '_entity' or 'Entity' not found in JSON");
    }

    private int GetRequiredOperation(JsonElement root)
    {
        if (root.TryGetProperty("type", out JsonElement operationElement) ||
           root.TryGetProperty(DatabaseConstants.DATA_FIELD_OPERATION, out operationElement))
        {
            string operationTypeName = operationElement.GetString();
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
                    default:
                        throw new JsonException($"Unknown operation type '{operationTypeName}'");
                }
            }
        }
        return OPERATION_FLAG_NONE;
    }

    //private (bool? isNew, bool? isDeleted) GetFlags(JsonElement root)
    //{
    //    bool? isNew = null;
    //    bool? isDeleted = null;
    //    if (root.TryGetProperty("IsNew", out JsonElement isNewElement))
    //    {
    //        isNew = isNewElement.GetBoolean();
    //    }

    //    if (root.TryGetProperty("IsDeleted", out JsonElement isDeletedElement))
    //    {
    //        isDeleted = isDeletedElement.GetBoolean();
    //    }

    //    return (isNew, isDeleted);
    //}

    private long? GetEntityId(JsonElement root, bool required)
    {
        if (root.TryGetProperty("Values", out JsonElement valuesElement))
        {
            if (valuesElement.TryGetProperty("Id", out JsonElement idElement) ||
                valuesElement.TryGetProperty(DatabaseConstants.FIELD_ID, out idElement))
            {
                return idElement.GetInt64();
            }
        }

        if (root.TryGetProperty("Id", out JsonElement directIdElement) ||
            root.TryGetProperty(DatabaseConstants.FIELD_ID, out directIdElement))
        {
            return directIdElement.GetInt64();
        }

        if (required)
            throw new JsonException("Required field 'Id' or '_id' not found in JSON");

        return null;
    }

    private void SetEntityFields(IDbEntityMetadata em, IEntity entity, JsonElement sourceElement, JsonSerializerOptions options, bool skipReservedFields = false)
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

            if (!sourceElement.TryGetProperty(fieldMetadata.Name, out JsonElement fieldElement))
                continue;

            try
            {
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

    private object DeserializeFieldValue(JsonElement fieldElement, IDbFieldMetadata fieldMetadata, JsonSerializerOptions options)
    {
        if (fieldElement.ValueKind == JsonValueKind.Null)
            return null;

        return JsonSerializer.Deserialize(fieldElement.GetRawText(), fieldMetadata.Type, options);
    }

    private void SetEntityVersion(IEntity entity, JsonElement sourceElement)
    {
        if (sourceElement.TryGetProperty(DatabaseConstants.FIELD_VERSION, out JsonElement versionElement))
        {
            entity.DbVersion = versionElement.GetInt32();
        }
    }

    public override void Write(Utf8JsonWriter writer, IEntity entity, JsonSerializerOptions options)
    {
        try
        {
            if (useNestedEntities == null)
            {
                useNestedEntities = true;
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
            useNestedEntities = null;
        }
    }

    public static void Register()
    {
        EntityJsonConverter conv = new EntityJsonConverter();
        JsonHelper.Register(conv);
    }
}
