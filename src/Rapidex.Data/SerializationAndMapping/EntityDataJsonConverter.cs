using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Rapidex.Data;
public static class EntityDataJsonConverter
{
    [ThreadStatic]
    static IDbSchemaScope deserializationContext = null;

    public static void AddDefaultJsonOptions(this IServiceCollection services)
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.SetDefaultOptions();
        });
    }

    internal static IDbSchemaScope DeserializationContext
    {
        get => deserializationContext;
        private set => deserializationContext = value;
    }


    public static string Serialize(IEntity entity)
    {
        if (entity == null)
            return "null";

        string json = entity.ToJson();
        return json;
    }

    public static string Serialize(IEnumerable<IEntity> entities)
    {
        if (entities == null)
            return "null";

        string json = entities.ToJson();
        return json;
    }


    private static IEnumerable<IEntity> DeserializeInternal(string json, IDbSchemaScope scope)
    {
        try
        {
            DeserializationContext = scope;

            string pJson = json?.Trim();
            if (pJson.IsNullOrEmpty())
                return Array.Empty<IPartialEntity>();

            JsonNode node = JsonNode.Parse(json);

            if (node.GetValueKind() == JsonValueKind.Array)
            {
                IEntity[] ents = JsonSerializer.Deserialize<IEntity[]>(json, JsonHelper.JsonSerializerOptions);
                return ents;
            }
            else
            {
                IEntity ent = JsonSerializer.Deserialize<IEntity>(node, JsonHelper.JsonSerializerOptions);
                return new IEntity[] { ent };
            }
        }
        finally
        {
            DeserializationContext = null;
        }
    }

    /// <summary>
    /// Create attached full entities from json
    /// </summary>
    /// <param name="json"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    /// <remarks>not support json commands abc</remarks>
    public static IEnumerable<IEntity> Deserialize(string json, IDbSchemaScope scope)
    {
        IEnumerable<IEntity> entities = DeserializeInternal(json, scope);
        return entities;
    }

    public static IEnumerable<T> Deserialize<T>(string json, IDbSchemaScope scope) where T : IConcreteEntity
    {
        IEnumerable<IEntity> entities = DeserializeInternal(json, scope);
        return entities.Cast<T>();
    }


}
