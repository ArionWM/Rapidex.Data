using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Rapidex.Data
{
    public class Json : BasicBaseDataType<string, Json>
    {
        public override string TypeName => "json";

        public override IDbFieldMetadata SetupMetadata(IDbEntityMetadataManager containerManager, IDbFieldMetadata self, ObjDictionary values)
        {
            self.DbProperties.Length = -1; //Max
            return base.SetupMetadata(containerManager, self, values);
        }

        public override IValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            Json clone = new Json();
            clone.Value = this.Value;
            return clone;
        }


        public static implicit operator Json(string value)
        {
            return new Json() { Value = value };
        }

        public static implicit operator string(Json value)
        {
            return value?.Value;
        }

        public static implicit operator Json(JsonNode value)
        {
            return new Json() { Value = value.ToJsonString() };
        }

        public static implicit operator JsonNode(Json value)
        {
            JsonNode node = value.Value.IsNullOrEmpty() ? new JsonObject() : JsonNode.Parse(value.Value);

            return node;
        }
    }
}
