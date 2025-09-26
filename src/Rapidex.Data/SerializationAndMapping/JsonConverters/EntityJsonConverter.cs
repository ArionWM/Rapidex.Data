using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rapidex.Data.Exceptions;
using Rapidex.Data.Metadata.Implementers;

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

        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;

            string entityTypeName = this.GetRequiredEntityType(root);
            (bool? isNew, bool? isDeleted) = this.GetFlags(root);

            bool idRequired = !(isNew ?? false);
            long? entityId = this.GetEntityId(root, idRequired);

            IPartialEntity entity = Database.EntityFactory.CreatePartialDetached(entityTypeName, entityId, isNew ?? false, isDeleted ?? false);

            if (root.TryGetProperty("Values", out JsonElement valuesElement))
            {
                this.SetEntityFields(entity, valuesElement, options);
            }
            else
            {
                //We support direct field definitions on root level as well
                this.SetEntityFields(entity, root, options, skipReservedFields: true);
            }

            if (!typeToConvert.IsAssignableFrom(typeof(IPartialEntity)))
            {
                throw new DataSerializationException("DeserializationSupport", $"Cannot deserialize json detached entity of type {typeToConvert}. For direct deserialize for defined entity type (concrete) use 'abc'. See: SerializationDeserializationJson.md ");
            }

            entity.EnsureDataTypeInitialization();

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

    private void SetEntityFields(IEntity entity, JsonElement sourceElement, JsonSerializerOptions options, bool skipReservedFields = false)
    {
        var reservedFields = new HashSet<string> {
            "Entity", CommonConstants.DATA_FIELD_TYPENAME,
            "Id", CommonConstants.FIELD_ID,
            CommonConstants.FIELD_VERSION,
            "IsNew", "IsDeleted"
        };

        // Direkt JSON property'lerini oku ve entity'ye ata
        foreach (JsonProperty property in sourceElement.EnumerateObject())
        {
            string fieldName = property.Name;
            JsonElement fieldElement = property.Value;

            // Reserved field'ları atla
            if (skipReservedFields && reservedFields.Contains(fieldName))
                continue;

            try
            {
                object value = this.DeserializeFieldValue(fieldElement, options);
                entity.SetValue(fieldName, value);
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error deserializing field '{fieldName}': {ex.Message}", ex);
            }
        }

        // Set version if present
        this.SetEntityVersion(entity, sourceElement);
    }

    private void SetEntityVersion(IEntity entity, JsonElement sourceElement)
    {
        if (sourceElement.TryGetProperty(CommonConstants.FIELD_VERSION, out JsonElement versionElement))
        {
            entity.DbVersion = versionElement.GetInt32();
        }
    }

    private object DeserializeFieldValue(JsonElement fieldElement, JsonSerializerOptions options)
    {
        if (fieldElement.ValueKind == JsonValueKind.Null)
            return null;

        return fieldElement.GetValueAsOriginalType();
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
