using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace Rapidex.Data.Cache;

internal class CacheFactory
{
    private ICache availableCache;
    private HybridCache keyedHybridCache;
    private HybridCache genericHybridCache;
    private CacheSignalImplementer signalImplementer;

    public CacheFactory(IServiceProvider serviceProvider)
    {
        // CacheSignalImplementer signalImplementer, ICache availableCache, [FromKeyedServices("rdata")] HybridCache hcache, HybridCache genericHCcache

        this.signalImplementer = serviceProvider.GetRequiredService<CacheSignalImplementer>();
        this.availableCache = serviceProvider.GetService<ICache>();
        this.keyedHybridCache = serviceProvider.GetKeyedService<HybridCache>("rdata");
        this.genericHybridCache = serviceProvider.GetService<HybridCache>(); 
    }

    public ICache Create()
    {
        if (this.availableCache != null)
            return this.availableCache;

        //Check hybrid cache first
        if (this.keyedHybridCache != null)
            return new DefaultHybridCache(this.keyedHybridCache);

        //Fallback to generic hybrid cache
        if (this.genericHybridCache != null)
            return new DefaultHybridCache(this.genericHybridCache);

        Rapidex.Common.DefaultLogger.LogWarning("using DefaultInMemoryCache (No another cache is configured) See: abc");
        return new DefaultInMemoryCache();
    }
}
