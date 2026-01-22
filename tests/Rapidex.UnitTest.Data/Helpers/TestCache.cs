using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Rapidex.Data.Cache;

namespace Rapidex.UnitTest.Data.Helpers;

internal class TestCache : ICache
{
    protected DefaultInMemoryCache MemoryCache { get; }
    protected DefaultHybridCache HybridCache { get; }

    public static bool MemoryCacheEnabled { get; set; } = false;
    public static bool HybridCacheEnabled { get; set; } = false;

    public TestCache(IServiceProvider serviceProvider)
    {
        this.MemoryCache = new DefaultInMemoryCache();

        var hybridCache = serviceProvider.GetService<HybridCache>();
        this.HybridCache = new DefaultHybridCache(hybridCache);
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (MemoryCacheEnabled)
        {
            return this.MemoryCache.GetOrSet<T>(key, valueFactory, expiration, localCacheExpiration);
        }

        if (HybridCacheEnabled)
        {
            return this.HybridCache.GetOrSet<T>(key, valueFactory, expiration, localCacheExpiration);
        }

        return default;
    }

    public Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (MemoryCacheEnabled)
        {
            return this.MemoryCache.GetOrSetAsync<T>(key, valueFactory, expiration, localCacheExpiration);
        }

        if (HybridCacheEnabled)
        {
            return this.HybridCache.GetOrSetAsync<T>(key, valueFactory, expiration, localCacheExpiration);
        }


#pragma warning disable CS8619 
        return Task.FromResult(default(T));
#pragma warning restore CS8619 

    }

    public void Remove(string key)
    {
        if (MemoryCacheEnabled)
        {
            this.MemoryCache.Remove(key);
        }

        if (HybridCacheEnabled)
        {
            this.HybridCache.Remove(key);
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (MemoryCacheEnabled)
        {
            this.MemoryCache.Set<T>(key, value, expiration, localCacheExpiration);
        }

        if (HybridCacheEnabled)
        {
            this.HybridCache.Set<T>(key, value, expiration, localCacheExpiration);
        }


    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (MemoryCacheEnabled)
        {
            return this.MemoryCache.SetAsync<T>(key, value, expiration, localCacheExpiration);
        }

        if (HybridCacheEnabled)
        {
            return this.HybridCache.SetAsync<T>(key, value, expiration, localCacheExpiration);
        }
        return Task.CompletedTask;
    }

    public void RemoveByTag(string tag)
    {
        if (MemoryCacheEnabled)
        {
        }

        if (HybridCacheEnabled)
        {
            this.HybridCache.RemoveByTag(tag);
        }
    }
}
