using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rapidex.Data.Cache;

internal class DefaultEntityHybridCache : IEntityCache, IDisposable
{
    private readonly DefaultHybridCache cache;
    private readonly ILogger<DefaultEntityHybridCache> logger;
    private readonly EntityMapper unattachedMapper;
    private readonly Channel<CacheWriteItem> writeChannel;
    private readonly CancellationTokenSource cts;

    public DefaultEntityHybridCache(IServiceProvider sp)
    {
        this.cache = sp.GetRequiredService<DefaultHybridCache>();
        this.logger = sp.GetService<ILogger<DefaultEntityHybridCache>>();
        this.unattachedMapper = new EntityMapper();

        this.writeChannel = Channel.CreateBounded<CacheWriteItem>(
            new BoundedChannelOptions(1024)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });

        this.cts = new CancellationTokenSource();
        _ = this.ConsumeWriteChannelAsync();
    }

    protected sealed class CacheWriteItem
    {
        public string Key { get; init; }
        public object Value { get; init; }
        public bool IsMultiple { get; init; }
    }

    private async Task ConsumeWriteChannelAsync()
    {
        try
        {
            await foreach (var item in this.writeChannel.Reader.ReadAllAsync(this.cts.Token))
            {
                try
                {
                    switch (item.IsMultiple)
                    {
                        case false:
                            var storageValue = this.CheckValueForStorage(item.Value);
                            await this.cache.Set(item.Key, storageValue);
                            break;
                        case true:
                            var entities = (List<IEntity>)item.Value;
                            List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();
                            foreach (var e in entities)
                            {
                                var sv = this.CheckValueForStorage(e);
                                values.Add((Dictionary<string, object>)sv);
                            }
                            await this.cache.Set(item.Key, values);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this.logger?.LogError(ex, "Cache write failed for key: {Key}", item.Key);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
    }

    protected object CheckValueForStorage(object value)
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

    public Task Set<T>(T entity) where T : IEntity
    {
        if (entity is IPartialEntity)
            return Task.CompletedTask;

        string key = CacheExtensions.GetEntityCacheKey(entity);

        this.writeChannel.Writer.TryWrite(new CacheWriteItem
        {
            Key = key,
            Value = entity,
            IsMultiple = false
        });

        return Task.CompletedTask;
    }


    public Task Set<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, string hash, IEnumerable<T> entities) where T : IEntity
    {
        string key = CacheExtensions.GetQueryCacheKey(em, dbSchema, hash);

        var entityList = entities.Cast<IEntity>().ToList();

        this.writeChannel.Writer.TryWrite(new CacheWriteItem
        {
            Key = key,
            Value = entityList,
            IsMultiple = true
        });

        return Task.CompletedTask;
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

    public void Dispose()
    {
        this.cts.Cancel();
        this.writeChannel.Writer.TryComplete();
        this.cts.Dispose();
    }
}
