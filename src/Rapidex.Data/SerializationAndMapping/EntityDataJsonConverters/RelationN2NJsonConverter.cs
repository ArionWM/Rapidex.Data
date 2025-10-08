using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static Rapidex.Data.RelationN2N;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class RelationN2NJsonConverter : JsonConverter<RelationN2N>
{
    public override RelationN2N? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        IEntity parent = EntityDataJsonConverter.DeserializationContext.CurrentEntity;
        VirtualRelationN2NDbFieldMetadata fm = EntityDataJsonConverter.DeserializationContext.CurrentFieldMetadata as VirtualRelationN2NDbFieldMetadata;
        fm.NotNull($"Field '{fm.Name}' must be VirtualRelationN2NDbFieldMetadata");

        RelationN2N relation = Activator.CreateInstance(fm.Type) as RelationN2N;
        relation.SetupInstance(parent, fm);

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Handle array of entities or entity references
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;


                if (reader.TokenType == JsonTokenType.Number)
                {
                    //Direct id value; [123, 123] etc.
                    // Create a partial entity reference with just the ID

                    long id = reader.GetInt64();

                    IPartialEntity entity = new PartialEntity();
                    entity.SetId(id);
                    IEntity junctionEntity = relation.Add(entity, false);
                    EntityDataJsonConverter.DeserializationContext.CreatedEntities.Add(junctionEntity);
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    IDictionary<string, object> values = JsonHelper.FromJsonToDictionary(ref reader);
                    if (values != null)
                    {
                        long id = values.Get("id").As<long>();
                        IPartialEntity entity = new PartialEntity();
                        entity.SetId(id);
                        IEntity junctionEntity = relation.Add(entity, false);
                        EntityDataJsonConverter.DeserializationContext.CreatedEntities.Add(junctionEntity);
                    }
                }
            }
        }
        else
        {
            throw new JsonException($"Unexpected token {reader.TokenType} when parsing RelationOne2N. Array required ([ .. ]).");
        }

        return relation;
    }

    public override void Write(Utf8JsonWriter writer, RelationN2N value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteRelation(writer, value, options);
    }

    public static void Register()
    {
        RelationN2NJsonConverter conv = new RelationN2NJsonConverter();
        JsonHelper.Register(conv);
    }
}
