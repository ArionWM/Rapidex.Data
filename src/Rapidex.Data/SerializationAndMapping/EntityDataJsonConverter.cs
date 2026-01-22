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
    public class EntityDeserializationContext
    {
        public IDbSchemaScope Scope { get; protected set; }
        public IDbEntityMetadata CurrentEntityMetadata { get; internal set; }
        public IDbFieldMetadata CurrentFieldMetadata { get; internal set; }
        public IEntity CurrentEntity { get; internal set; }
        //public string CurrentField { get; protected set; }

        public IList<IEntity> CreatedEntities { get; } = new List<IEntity>();

        public EntityDeserializationContext()
        {

        }

        public EntityDeserializationContext(IDbSchemaScope scope)
        {
            this.Scope = scope.NotNull();
        }
    }

    [ThreadStatic]
#pragma warning disable IDE1006 // Naming Styles
    static EntityDeserializationContext deserializationContext = null;
#pragma warning restore IDE1006 // Naming Styles

    public static void AddDefaultJsonOptions(this IServiceCollection services)
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.SetDefaultOptions();
        });
    }

    internal static EntityDeserializationContext DeserializationContext
    {
        get => deserializationContext;
        private set => deserializationContext = value;
    }


    public static void SetContext(IDbSchemaScope scope)
    {
        DeserializationContext = new EntityDeserializationContext(scope);
    }

    public static void ClearContext()
    {
        DeserializationContext = null;
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
            DeserializationContext = new EntityDeserializationContext(scope);

            string pJson = json?.Trim();
            if (pJson.IsNullOrEmpty())
                return Array.Empty<IPartialEntity>();

            JsonNode node = JsonNode.Parse(pJson, JsonHelper.DefaultJsonNodeOptions, JsonHelper.DefaultJsonDocumentOptions);
            List<IEntity> entities;
            if (node.GetValueKind() == JsonValueKind.Array)
            {
                entities = JsonSerializer.Deserialize<IEntity[]>(json, JsonHelper.DefaultJsonSerializerOptions).ToList();
            }
            else
            {
                IEntity ent = JsonSerializer.Deserialize<IEntity>(node, JsonHelper.DefaultJsonSerializerOptions);
                entities = new IEntity[] { ent }.ToList();
            }

            entities.AddRange(DeserializationContext.CreatedEntities);
            return entities;
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
