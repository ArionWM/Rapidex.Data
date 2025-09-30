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
            (bool? isNew, bool? isDeleted) = this.GetFlags(root);

            bool idRequired = !(isNew ?? false);
            long? entityId = this.GetEntityId(root, idRequired);

            var em = EntityDataJsonConverter.DeserializationContext.ParentDbScope.Metadata.Get(entityTypeName)
                 .NotNull($"Entity type '{entityTypeName}' not found in metadata.");

            IEntity entity = Database.EntityFactory.Create(em, EntityDataJsonConverter.DeserializationContext, isNew ?? false, isDeleted ?? false);

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
            root.TryGetProperty(CommonConstants.DATA_FIELD_TYPENAME, out entityElement))
        {
            string entityTypeName = entityElement.GetString();
            if (!string.IsNullOrEmpty(entityTypeName))
                return entityTypeName;
        }
        throw new JsonException("Required field '_entity' or 'Entity' not found in JSON");
    }

    private (bool? isNew, bool? isDeleted) GetFlags(JsonElement root)
    {
        bool? isNew = null;
        bool? isDeleted = null;
        if (root.TryGetProperty("IsNew", out JsonElement isNewElement))
        {
            isNew = isNewElement.GetBoolean();
        }

        if (root.TryGetProperty("IsDeleted", out JsonElement isDeletedElement))
        {
            isDeleted = isDeletedElement.GetBoolean();
        }

        return (isNew, isDeleted);
    }

    private long? GetEntityId(JsonElement root, bool required)
    {
        if (root.TryGetProperty("Values", out JsonElement valuesElement))
        {
            if (valuesElement.TryGetProperty("Id", out JsonElement idElement) ||
                valuesElement.TryGetProperty(CommonConstants.FIELD_ID, out idElement))
            {
                return idElement.GetInt64();
            }
        }

        if (root.TryGetProperty("Id", out JsonElement directIdElement) ||
            root.TryGetProperty(CommonConstants.FIELD_ID, out directIdElement))
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
            "Entity", CommonConstants.DATA_FIELD_TYPENAME,
            "Id", CommonConstants.FIELD_ID,
            CommonConstants.FIELD_VERSION,
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
        if (sourceElement.TryGetProperty(CommonConstants.FIELD_VERSION, out JsonElement versionElement))
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
            writer.WriteString(CommonConstants.DATA_FIELD_TYPENAME, em.Name);
            writer.WriteString(CommonConstants.DATA_FIELD_CAPTION, entity.Caption());
            writer.WriteNumber(CommonConstants.DATA_FIELD_ID, id);

            writer.WriteNumber(CommonConstants.FIELD_ID, id);
            writer.WriteNumber(CommonConstants.FIELD_VERSION, entity.DbVersion);

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
