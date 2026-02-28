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
    TimeSpan? DefaultAbsoluteExpiration { get; }
    TimeSpan? DefaultSlidingExpiration { get; }

    public DefaultInMemoryCache()
    {
        if ((Database.Configuration.CacheConfigurationInfo?.InMemory?.AbsoluteExpiration ?? 0) > 0)
        {
            this.DefaultAbsoluteExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo!.InMemory!.AbsoluteExpiration!.Value);
        }

        if ((Database.Configuration.CacheConfigurationInfo?.InMemory?.SlidingExpiration ?? 0) > 0)
        {
            this.DefaultSlidingExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo!.InMemory!.SlidingExpiration!.Value);
        }

        var mcOptions = new MemoryCacheOptions();
        if (Database.Configuration.CacheConfigurationInfo?.InMemory?.ItemLimit.HasValue ?? false)
            mcOptions.SizeLimit = Database.Configuration.CacheConfigurationInfo.InMemory.ItemLimit.Value;

        this.Cache = new MemoryCache(mcOptions);
    }

    protected MemoryCacheEntryOptions GetOptions()
    {
        MemoryCacheEntryOptions opt = new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = this.DefaultAbsoluteExpiration,
            SlidingExpiration = this.DefaultSlidingExpiration,
            Size = 1
        };

        return opt;
    }

    public virtual T GetOrSet<T>(string key, Func<T> valueFactory)
    {
        if (!this.Cache.TryGetValue(key, out T value))
        {
            value = valueFactory();
            this.Cache.Set(key, value, this.GetOptions());
        }
        return value;
    }

    public virtual async Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory)
    {
        if (!this.Cache.TryGetValue(key, out T value))
        {
            value = await Task.Run(valueFactory);
            this.Cache.Set(key, value, this.GetOptions());
        }
        return value;
    }

    public virtual Task Remove(string key)
    {
        this.Cache.Remove(key);
        return Task.CompletedTask;
    }

    public virtual void Set<T>(string key, T value)
    {
        this.Cache.Set(key, value, this.GetOptions());
    }

    public virtual Task SetAsync<T>(string key, T value)
    {


        this.Cache.Set(key, value, this.GetOptions());
        return Task.CompletedTask;
    }

    public Task RemoveByTag(string tag)
    {
        return Task.CompletedTask;

    }
}
