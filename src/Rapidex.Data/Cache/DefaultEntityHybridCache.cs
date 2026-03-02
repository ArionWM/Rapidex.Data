using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Cache;

internal class DefaultEntityHybridCache : IEntityCache
{
    private readonly DefaultHybridCache cache;
    private readonly ILogger<DefaultEntityHybridCache> logger;
    private readonly EntityMapper unattachedMapper;

    public DefaultEntityHybridCache(IServiceProvider sp)
    {
        this.cache = sp.GetRequiredService<DefaultHybridCache>();
        this.logger = sp.GetService<ILogger<DefaultEntityHybridCache>>();
        this.unattachedMapper = new EntityMapper();
    }

    protected object CheckFalueForStorage(object value)
    {
        switch (value)
        {
            case IEntity entity:
                Dictionary<string, object> rvalue = (Dictionary<string, object>)EntityMapper.MapToDict(entity, true);
                return rvalue;
            default:
                return value;
        }
    }

    protected object CheckEntityValueForRetrieve(IDbSchemaScope dbSchema, string typeName, Dictionary<string, object> value)
    {
        var em = dbSchema.ParentDbScope.Metadata.Get(typeName);
        IEntity entity = this.unattachedMapper.Map(dbSchema, em, value);
        if (entity != null)
        {
            entity._loadSource = LoadSource.Cache;
            entity._Schema = dbSchema;
            entity._SchemaName = dbSchema.SchemaName;
        }
        return entity;
    }


    public async Task<T> Get<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, object id) where T : IEntity
    {
        string key = CacheExtensions.GetEntityCacheKey(dbSchema, em, id);

        var retValue = await this.cache.GetOrSet<Dictionary<string, object>>(key, () => null);
        if (retValue == null)
            return default(T);
        
        var cValue = this.CheckEntityValueForRetrieve(dbSchema, em.Name, retValue);
        T eValue = (T)cValue;
        return (T)cValue;
    }




    public Task Set<T>(T entity) where T : IEntity
    {
        if (entity is IPartialEntity)
            return Task.CompletedTask;

        string key = CacheExtensions.GetEntityCacheKey(entity);

        var storageValue = this.CheckFalueForStorage(entity);

        return this.cache.Set(key, storageValue);
    }

    public async Task<T[]> GetMultiple<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, string hash) where T : IEntity
    {
        
        string key = CacheExtensions.GetQueryCacheKey(em, dbSchema, hash);

        var retValue = await this.cache.GetOrSet<Dictionary<string, object>[]>(key, () => default);
        if (retValue != null)
        {
            List<T> entities = new List<T>();
            foreach (var dict in retValue)
            {
                var cValue = this.CheckEntityValueForRetrieve(dbSchema, em.Name, dict);
                T eValue = (T)cValue;
                entities.Add(eValue);
            }

            return entities.ToArray();
        }

        return null;
    }

    public async Task Set<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, string hash, IEnumerable<T> entities) where T : IEntity
    {
        string key = CacheExtensions.GetQueryCacheKey(em, dbSchema, hash);

        List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();
        foreach (var e in entities)
        {
            var storageValue = this.CheckFalueForStorage(e);
            values.Add((Dictionary<string, object>)storageValue);
        }

        await this.cache.Set(key, values);
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


}
