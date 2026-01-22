using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace Rapidex.Data.Cache;

internal class DefaultHybridCache : ICache
{
    private const string TAG = "entity";
    private readonly HybridCache cache;
    private TimeSpan defaultExpiration;
    private TimeSpan defaultLocalExpiration;


    public DefaultHybridCache([FromKeyedServices("rdata")] HybridCache hcache)
    {
        this.cache = hcache;
        this.defaultExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo?.Distributed?.Expiration ?? 6000);
        this.defaultLocalExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo?.Distributed?.LocalExpiration ?? 600);
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        HybridCacheEntryOptions opt = new HybridCacheEntryOptions()
        {
            Expiration = expiration ?? this.defaultExpiration,
            LocalCacheExpiration = localCacheExpiration ?? this.defaultLocalExpiration
        };

        var task = this.cache.GetOrCreateAsync(
            key,
            ct => new ValueTask<T>(valueFactory()),
            opt,
            CacheExtensions.TagContext
        );
        return task.AsTask().GetAwaiter().GetResult();
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        HybridCacheEntryOptions opt = new HybridCacheEntryOptions()
        {
            Expiration = expiration ?? this.defaultExpiration,
            LocalCacheExpiration = localCacheExpiration ?? this.defaultLocalExpiration
        };

        return await this.cache.GetOrCreateAsync(
            key,
            ct => new ValueTask<T>(valueFactory()),
            opt,
            CacheExtensions.TagContext
        );
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        HybridCacheEntryOptions opt = new HybridCacheEntryOptions()
        {
            Expiration = expiration ?? this.defaultExpiration,
            LocalCacheExpiration = localCacheExpiration ?? this.defaultLocalExpiration
        };

        this.cache.SetAsync(key, value, opt, CacheExtensions.TagContext).AsTask().GetAwaiter().GetResult();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        HybridCacheEntryOptions opt = new HybridCacheEntryOptions()
        {
            Expiration = expiration ?? this.defaultExpiration,
            LocalCacheExpiration = localCacheExpiration ?? this.defaultLocalExpiration
        };

        await this.cache.SetAsync(key, value, opt, CacheExtensions.TagContext);
    }

    public void Remove(string key)
    {
        this.cache.RemoveAsync(key).AsTask().GetAwaiter().GetResult();
    }

    public void RemoveByTag(string tag)
    {
        this.cache.RemoveByTagAsync(tag).AsTask().GetAwaiter().GetResult();
    }
}
