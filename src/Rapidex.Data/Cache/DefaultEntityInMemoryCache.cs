using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data.Cache;
using Superpower.Model;

namespace Rapidex.Data;

internal class DefaultEntityInMemoryCache : IEntityCache
{
    private readonly DefaultInMemoryCache cache;
    private readonly ILogger<DefaultEntityInMemoryCache> logger;

    public DefaultEntityInMemoryCache(IServiceProvider sp)
    {
        this.cache = sp.GetRequiredService<DefaultInMemoryCache>();
        this.logger = sp.GetService<ILogger<DefaultEntityInMemoryCache>>();

    }

    public async Task<T> Get<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, object id) where T : IEntity
    {
        string key = CacheExtensions.GetEntityCacheKey(dbSchema, em, id);

        var retValue = await this.cache.GetOrSet<T>(key, () => default(T));
        if (retValue != null)
        {
            retValue._Schema = dbSchema;
            retValue._SchemaName = dbSchema.SchemaName;
            retValue._loadSource = LoadSource.Cache;
        }
        return retValue;
    }


    public Task Set<T>(T entity) where T : IEntity
    {
        if (entity is IPartialEntity)
            return Task.CompletedTask;
        
        string key = CacheExtensions.GetEntityCacheKey(entity);

        return this.cache.Set(key, entity);
    }

    public async Task Remove<T>(T entity) where T : IEntity
    {
        if (entity.HasPrematureOrEmptyId())
            return;

        string key = CacheExtensions.GetEntityCacheKey(entity);
        await this.Remove(key);
    }

    public async Task Remove(string key)
    {
        await this.cache.Remove(key);
    }

    public async Task RemoveByTag(string tag)
    {
        await this.cache.RemoveByTag(tag);
    }

    public async Task<T[]> GetMultiple<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, string hash) where T : IEntity
    {
        string key = CacheExtensions.GetQueryCacheKey(em, dbSchema, hash);

        var retValue = await this.cache.GetOrSet<T[]>(key, () => default(T[]));
        if (retValue != null)
        {
            foreach (var entity in retValue)
            {
                entity._Schema = dbSchema;
                entity._SchemaName = dbSchema.SchemaName;
                entity._loadSource = LoadSource.Cache;
            }
        }

        return retValue;
    }

    public async Task Set<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, string hash, IEnumerable<T> entities) where T : IEntity
    {
        entities.NotEmpty();
        string key =CacheExtensions.GetQueryCacheKey(em, dbSchema, hash);
        await this.cache.Set(key, entities);
    }
}
