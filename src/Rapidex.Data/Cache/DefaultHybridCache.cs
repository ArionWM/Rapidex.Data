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
    private int readTimeoutMs;
    private readonly ILogger<DefaultHybridCache> logger;


    public DefaultHybridCache(HybridCache hcache, ILogger<DefaultHybridCache> logger)
    {
        this.cache = hcache;
        this.defaultExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo?.Distributed?.Expiration ?? 6000);
        this.defaultLocalExpiration = TimeSpan.FromSeconds(Database.Configuration.CacheConfigurationInfo?.Distributed?.LocalExpiration ?? 600);
        this.readTimeoutMs = Database.Configuration.CacheConfigurationInfo?.Distributed?.ReadTimeout ?? 60000;
        this.logger = logger;
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

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(this.readTimeoutMs));

            T? result;
            try
            {
                result = await this.cache.GetOrCreateAsync(
                     key,
                     ct => new ValueTask<T>(this.InternalValueFactory(valueFactory)),
                     opt,
                     CacheExtensions.TagContext, 
                     cts.Token
                 );
            }
            catch (OperationCanceledException)
            {
                // Timeout → cache'i atla, direkt factory'den al
                logger.LogWarning("Cache timeout, bypassing: {Key}", key);
                return valueFactory();
            }

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
