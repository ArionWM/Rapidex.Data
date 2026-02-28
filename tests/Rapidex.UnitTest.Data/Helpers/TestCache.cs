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

    public T GetOrSet<T>(string key, Func<T> valueFactory)
    {
        if (MemoryCacheEnabled)
        {
            return this.MemoryCache.GetOrSet<T>(key, valueFactory);
        }

        if (HybridCacheEnabled)
        {
            return this.HybridCache.GetOrSet<T>(key, valueFactory);
        }

        return default;
    }

    public Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory)
    {
        if (MemoryCacheEnabled)
        {
            return this.MemoryCache.GetOrSetAsync<T>(key, valueFactory);
        }

        if (HybridCacheEnabled)
        {
            return this.HybridCache.GetOrSetAsync<T>(key, valueFactory);
        }


#pragma warning disable CS8619 
        return Task.FromResult(default(T));
#pragma warning restore CS8619 

    }

    public async Task Remove(string key)
    {
        if (MemoryCacheEnabled)
        {
           await this.MemoryCache.Remove(key);
        }

        if (HybridCacheEnabled)
        {
            await this.HybridCache.Remove(key);
        }
    }

    public void Set<T>(string key, T value)
    {
        if (MemoryCacheEnabled)
        {
            this.MemoryCache.Set<T>(key, value);
        }

        if (HybridCacheEnabled)
        {
            this.HybridCache.Set<T>(key, value);
        }


    }

    public Task SetAsync<T>(string key, T value)
    {
        if (MemoryCacheEnabled)
        {
            return this.MemoryCache.SetAsync<T>(key, value);
        }

        if (HybridCacheEnabled)
        {
            return this.HybridCache.SetAsync<T>(key, value);
        }
        return Task.CompletedTask;
    }

    public async Task RemoveByTag(string tag)
    {
        if (MemoryCacheEnabled)
        {
        }

        if (HybridCacheEnabled)
        {
            await this.HybridCache.RemoveByTag(tag);
        }
    }
}
