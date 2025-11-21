using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Rapidex.Data.Cache;

internal class InMemoryCache : ICache   
{
    MemoryCache Cache { get; }
    TimeSpan DefaultExpiration { get; }

    public InMemoryCache()
    {
        this.DefaultExpiration = TimeSpan.FromMinutes(10);
        this.Cache = new MemoryCache(new MemoryCacheOptions());
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (!this.Cache.TryGetValue(key, out T value))
        {
            value = valueFactory();
            this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
        }
        return value;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (!this.Cache.TryGetValue(key, out T value))
        {
            value = await Task.Run(valueFactory);
            this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
        }
        return value;
    }

    public void Remove(string key)
    {
        this.Cache.Remove(key);
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        this.Cache.Set(key, value, expiration ?? this.DefaultExpiration);
        return Task.CompletedTask;
    }
}
