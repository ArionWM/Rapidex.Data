using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rapidex.Data.Metadata.Implementers;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class EntityJsonConverter : JsonConverter<IEntity>
{
    [ThreadStatic]
    internal static bool? useNestedEntities = null;

    public override IEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, IEntity value, JsonSerializerOptions options)
    {
        try
        {
            if (useNestedEntities == null)
            {
                useNestedEntities = true;
            }

            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (value is not IPartialEntity)
                value.EnsureDataTypeInitialization();

            IDbEntityMetadata em = value.GetMetadata();

            long id = value.GetId().As<long>();

            writer.WriteStartObject();
            writer.WriteString(CommonConstants.DATA_FIELD_TYPENAME, em.Name);
            writer.WriteString(CommonConstants.DATA_FIELD_CAPTION, value.Caption());
            writer.WriteNumber(CommonConstants.DATA_FIELD_ID, id);

            writer.WriteNumber(CommonConstants.FIELD_ID, id);
            writer.WriteNumber(CommonConstants.FIELD_VERSION, value.DbVersion);

            writer.WritePropertyName("Values");
            writer.WriteStartObject();

            foreach (IDbFieldMetadata fm in em.Fields.Values)
            {
                writer.WritePropertyName(fm.Name);

                object upperValue = fm.ValueGetterUpper(value, fm.Name);
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
