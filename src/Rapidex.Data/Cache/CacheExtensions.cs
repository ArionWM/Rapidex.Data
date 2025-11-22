using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

public static class CacheExtensions
{
    public static string GetEntityCacheKey(IEntity entity)
    {
        IDbSchemaScope dbSchema = entity._Schema;
        string dbName = dbSchema.ParentDbScope.Name;
        string schemaName = dbSchema.SchemaName;
        var typeName = entity._TypeName;
        var id = entity.GetId();

        return $"{dbName}:{schemaName}:{typeName}:{id}";
    }

    public static void SetEntity(this ICache cache, IEntity entity)
    {
        if (entity.HasPrematureId())
            return;

        string key = GetEntityCacheKey(entity);
        cache.Set(key, entity);
    }

    public static void SetEntities(this ICache cache, IEnumerable<IEntity> entities)
    {
        foreach (var entity in entities)
        {
            cache.SetEntity(entity);
        }
    }

    public static IEntity GetEntity(this ICache cache, IDbSchemaScope dbSchema, string typeName, long id)
    {
        if (id.IsPrematureId())
            return null;

        string dbName = dbSchema.ParentDbScope.Name;
        string schemaName = dbSchema.SchemaName;
        string key = $"{dbName}:{schemaName}:{typeName}:{id}";
        var entity = cache.GetOrSet<IEntity>(key, () => null);
        entity._loadSource = LoadSource.Cache;
        return entity;
    }

    public static void RemoveEntity(this ICache cache, IEntity entity)
    {
        if (entity.HasPrematureId())
            return;

        string key = GetEntityCacheKey(entity);
        cache.Remove(key);
    }
}
