using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data.Cache;

namespace Rapidex.UnitTest.Data.Helpers;

internal class TestCache : DefaultInMemoryCache, ICache
{
    public static bool CacheEnabled { get; set; } = false;

    public override T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (CacheEnabled)
        {
            return base.GetOrSet<T>(key, valueFactory, expiration, localCacheExpiration);
        }

        return default;
    }

    public override Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (CacheEnabled)
        {
            return base.GetOrSetAsync<T>(key, valueFactory, expiration, localCacheExpiration);
        }
#pragma warning disable CS8619 
        return Task.FromResult(default(T));
#pragma warning restore CS8619 

    }

    public override void Remove(string key)
    {
        if (CacheEnabled)
        {
            base.Remove(key);
        }
    }

    public override void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (CacheEnabled)
        {
            base.Set<T>(key, value, expiration, localCacheExpiration);
        }
    }

    public override Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        if (CacheEnabled)
        {
            return base.SetAsync<T>(key, value, expiration, localCacheExpiration);
        }
        return Task.CompletedTask;
    }
}
