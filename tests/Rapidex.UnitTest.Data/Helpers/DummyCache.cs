using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.Helpers;

public class DummyCache : ICache
{
    public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        return default;
    }

    public Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        return default;
    }

    public void Remove(string key)
    {
        
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null)
    {
        return Task.CompletedTask;
    }
}
