using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace Rapidex.Data.Cache;

internal class DefaultHybridCache : ICache
{
    private readonly HybridCache cache;
    private TimeSpan defaultExpiration;
    private TimeSpan defaultLocalExpiration;


    public DefaultHybridCache(HybridCache hcache)
    {
        this.cache = hcache;
        this.defaultExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo?.Distributed?.Expiration ?? 6000);
        this.defaultLocalExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo?.Distributed?.LocalExpiration ?? 600);
    }

    protected T InternalValueFactory<T>(Func<T> valueFactory)
    {
        if (valueFactory == null)
            return default(T);

        T val = valueFactory.Invoke();
        return val;
    }

    public async Task<T> GetOrSet<T>(string key, Func<T> valueFactory)
    {
        try
        {
            HybridCacheEntryOptions opt = new HybridCacheEntryOptions()
            {
                Expiration = this.defaultExpiration,
                LocalCacheExpiration = this.defaultLocalExpiration
            };

            var result = await this.cache.GetOrCreateAsync(
                key,
                ct => new ValueTask<T>(this.InternalValueFactory(valueFactory)),
                opt,
                CacheExtensions.TagContext
            );

            return result;
        }
        catch (Exception ex)
        {
            ex.Log("Key: " + key);
            throw;
        }
    }

    public async Task Set<T>(string key, T value)
    {
        try
        {
            HybridCacheEntryOptions opt = new HybridCacheEntryOptions()
            {
                Expiration = this.defaultExpiration,
                LocalCacheExpiration = this.defaultLocalExpiration
            };

            await this.cache.SetAsync(key, value, opt, CacheExtensions.TagContext);
        }
        catch (Exception ex)
        {
            ex.Log("Key: " + key);
            throw;
        }

    }

    public async Task Remove(string key)
    {
        await this.cache.RemoveAsync(key);
    }

    public async Task RemoveByTag(string tag)
    {
        await this.cache.RemoveByTagAsync(tag);
    }
}
