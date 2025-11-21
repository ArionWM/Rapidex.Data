using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace Rapidex.Data.Cache;

internal class DefaultHybridCache : ICache
{
    private readonly HybridCache cache;

    public DefaultHybridCache([FromKeyedServices("rdata")] HybridCache hcache)
    {
        this.cache = hcache;
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {

        HybridCacheEntryOptions opt = null;
        if(expiration is not null || localCacheExpiration is not null)
        {
            opt = new HybridCacheEntryOptions()
            {
                Expiration = expiration,
                LocalCacheExpiration = localCacheExpiration
            };
        }

        var task = this.cache.GetOrCreateAsync(
            key,
            ct => new ValueTask<T>(valueFactory()),
            opt
        );
        return task.AsTask().GetAwaiter().GetResult();
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        HybridCacheEntryOptions opt = null;
        if (expiration is not null || localCacheExpiration is not null)
        {
            opt = new HybridCacheEntryOptions()
            {
                Expiration = expiration,
                LocalCacheExpiration = localCacheExpiration
            };
        }

        return await this.cache.GetOrCreateAsync(
            key,
            ct => new ValueTask<T>(valueFactory()),
            opt
        );
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        HybridCacheEntryOptions opt = null;
        if (expiration is not null || localCacheExpiration is not null)
        {
            opt = new HybridCacheEntryOptions()
            {
                Expiration = expiration,
                LocalCacheExpiration = localCacheExpiration
            };
        }

        this.cache.SetAsync(key, value, opt).AsTask().GetAwaiter().GetResult();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        HybridCacheEntryOptions opt = null;
        if (expiration is not null || localCacheExpiration is not null)
        {
            opt = new HybridCacheEntryOptions()
            {
                Expiration = expiration,
                LocalCacheExpiration = localCacheExpiration
            };
        }

        await this.cache.SetAsync(key, value, opt);
    }

    public void Remove(string key)
    {
        this.cache.RemoveAsync(key).AsTask().GetAwaiter().GetResult();
    }
}
