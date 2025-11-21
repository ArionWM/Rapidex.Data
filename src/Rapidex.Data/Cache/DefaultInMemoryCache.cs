using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Rapidex.Data.Cache;

internal class DefaultInMemoryCache : ICache
{
    MemoryCache Cache { get; }
    TimeSpan DefaultExpiration { get; }

    public DefaultInMemoryCache()
    {
        this.DefaultExpiration = TimeSpan.FromMinutes(10);
        this.Cache = new MemoryCache(new MemoryCacheOptions());
    }

    public virtual T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (!this.Cache.TryGetValue(key, out T value))
        {
            value = valueFactory();
            this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
        }
        return value;
    }

    public virtual async Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (!this.Cache.TryGetValue(key, out T value))
        {
            value = await Task.Run(valueFactory);
            this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
        }
        return value;
    }

    public virtual void Remove(string key)
    {
        this.Cache.Remove(key);
    }

    public virtual void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
    }

    public virtual Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
        return Task.CompletedTask;
    }
}
