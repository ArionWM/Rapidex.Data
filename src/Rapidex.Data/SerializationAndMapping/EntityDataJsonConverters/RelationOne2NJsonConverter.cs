using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Rapidex.Data.RelationN2N;
using static Rapidex.Data.RelationOne2N;

namespace Rapidex.Data.SerializationAndMapping.JsonConverters;
internal class RelationOne2NJsonConverter : JsonConverter<RelationOne2N>
{
    public override RelationOne2N? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;


        IEntity parent = EntityDataJsonConverter.DeserializationContext.CurrentEntity;
        VirtualRelationOne2NDbFieldMetadata fm = EntityDataJsonConverter.DeserializationContext.CurrentFieldMetadata as VirtualRelationOne2NDbFieldMetadata;
        fm.NotNull($"Field '{fm.Name}' must be VirtualRelationOne2NDbFieldMetadata");

        RelationOne2N relation = Activator.CreateInstance(fm.Type) as RelationOne2N;
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
                    //IPartialEntity entity = Database.EntityFactory.CreatePartial(em, EntityDataJsonConverter.DeserializationContext, false, true);
                    entity.SetId(id);
                    relation.Add(entity);
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    IDictionary<string, object> values = JsonHelper.FromJsonToDictionary(ref reader);
                    if (values != null)
                    {
                        long id = values.Get("id").As<long>();
                        IPartialEntity entity = new PartialEntity();
                        entity.SetId(id);
                        relation.Add(entity, false);
                    }
                }
            }

            return relation;
        }
        else
        {
            throw new JsonException($"Unexpected token {reader.TokenType} when parsing RelationOne2N. Array required ([ .. ]).");
        }
    }

    public override void Write(Utf8JsonWriter writer, RelationOne2N value, JsonSerializerOptions options)
    {
        JsonConvertersHelper.WriteRelation(writer, value, options);
    }

    public static void Register()
    {
        RelationOne2NJsonConverter conv = new RelationOne2NJsonConverter();
        JsonHelper.Register(conv);
    }
}
