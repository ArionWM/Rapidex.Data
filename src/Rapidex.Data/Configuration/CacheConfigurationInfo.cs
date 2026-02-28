using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
