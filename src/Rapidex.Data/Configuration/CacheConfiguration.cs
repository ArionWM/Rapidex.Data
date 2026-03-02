using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Rapidex.Data.Cache;
using StackExchange.Redis;

namespace Rapidex.Data;


public class InMemoryCacheConfigurationInfo
{
    /// <summary>
    /// Max item count for in-memory cache. If null, no limit.
    /// </summary>
    public int? ItemLimit { get; set; } = null;

    /// <summary>
    /// Expiration time in seconds for in-memory cache items.
    /// </summary>
    public int? AbsoluteExpiration { get; set; } = null;

    /// <summary>
    /// Expiration time in seconds for in-memory cache items.
    /// </summary>
    public int? SlidingExpiration { get; set; } = null;
}

public class DistributedCacheConfigurationInfo
{
    /// <summary>
    /// For now only "Redis" is supported. 
    /// If null or empty, no distributed (hybrid) cache is used.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Connection string for the distributed cache.
    /// If null or empty, no distributed (hybrid) cache is used.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Expiration time in seconds for distributed (remote) cache items.
    /// </summary>
    public int? Expiration { get; set; } = null;

    /// <summary>
    /// Expiration time in seconds for (local) in-memory cache items.
    /// If null, uses InMemory / Expiration setting.
    /// </summary>
    public int? LocalExpiration { get; set; } = null;
}

public class CacheConfigurationInfo
{
    public InMemoryCacheConfigurationInfo InMemory { get; set; } = new InMemoryCacheConfigurationInfo();

    public DistributedCacheConfigurationInfo Distributed { get; set; } = new DistributedCacheConfigurationInfo();

}

public class CacheConfigurationManager
{
    protected void AddMemoryCacheInternal(CacheConfigurationInfo conf, IServiceCollection services)
    {
        services.AddMemoryCache(options =>
        {
            if ((conf?.InMemory?.ItemLimit ?? 0) > 0)
            {
                options.SizeLimit = conf.InMemory!.ItemLimit;
            }
        });
    }

    public void AddMemoryCache(CacheConfigurationInfo conf, IServiceCollection services)
    {
        this.AddMemoryCacheInternal(conf, services);
        services.AddSingleton<DefaultInMemoryCache, DefaultInMemoryCache>();
        services.AddSingleton<ICache, DefaultInMemoryCache>(sp => sp.GetRequiredService<DefaultInMemoryCache>());


        services.AddSingleton<DefaultEntityInMemoryCache, DefaultEntityInMemoryCache>();
        services.AddSingleton<IEntityCache, DefaultEntityInMemoryCache>(sp => sp.GetRequiredService<DefaultEntityInMemoryCache>());
        
    }


    public void AddHybridCache(CacheConfigurationInfo conf, IServiceCollection services)
    {
        this.AddMemoryCacheInternal(conf, services);

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions()
            {
                Expiration = conf?.Distributed?.Expiration == null ? null : TimeSpan.FromSeconds(conf!.Distributed!.Expiration.Value),
                LocalCacheExpiration = conf?.Distributed?.LocalExpiration == null ? null : TimeSpan.FromSeconds(conf!.Distributed!.LocalExpiration.Value)
            };
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = ConfigurationOptions.Parse(conf!.Distributed!.ConnectionString);
            options.ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
        });

        services.AddSingleton<DefaultHybridCache, DefaultHybridCache>();
        services.AddSingleton<ICache, DefaultHybridCache>(sp => sp.GetRequiredService<DefaultHybridCache>());

        services.AddSingleton<DefaultEntityHybridCache, DefaultEntityHybridCache>();
        services.AddSingleton<IEntityCache, DefaultEntityHybridCache>(sp => sp.GetRequiredService<DefaultEntityHybridCache>());
    }


    public void ApplyCacheConfiguration(CacheConfigurationInfo conf, IServiceCollection services)
    {
        services.AddSingleton<CacheSignalImplementer>((sp) =>
        {
            CacheSignalImplementer csi = new();
            csi.Start();
            return csi;
        });

        if (conf?.Distributed?.ConnectionString.IsNOTNullOrEmpty() ?? false)
        {
            this.AddHybridCache(conf, services);
        }
        else
        {
            this.AddMemoryCache(conf, services);
        }
    }
}
