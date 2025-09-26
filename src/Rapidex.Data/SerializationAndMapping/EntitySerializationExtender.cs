using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rapidex.Data
{
    public static class EntitySerializationExtender
    {
        public static T FromJson<T>(this string json, IDbSchemaScope scope) where T : IEntity
        {
            aaaa

            json = json?.Trim();
            if (json.IsNullOrEmpty())
                return default(T);

            T ent = JsonSerializer.Deserialize<T>(json, JsonHelper.JsonSerializerOptions);
            ent._Schema = scope;

            ent.EnsureDataTypeInitialization();

            return ent;
        }
    }
}
