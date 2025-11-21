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

    public CacheFactory(ICache availableCache, [FromKeyedServices("rdata")] HybridCache hcache, HybridCache genericHCcache)
    {
        this.availableCache = availableCache;
        this.keyedHybridCache = hcache;
        this.genericHybridCache = genericHCcache;
    }

    public ICache Create()
    {
        if (this.availableCache != null)
            return this.availableCache;

        //Check hybrid cache first
        if (this.keyedHybridCache != null)
            return new DefaultCache(this.keyedHybridCache);

        //Fallback to generic hybrid cache
        if (this.genericHybridCache != null)
            return new DefaultCache(this.genericHybridCache);

        Rapidex.Common.DefaultLogger.LogWarning("No cache is configured. See: abc");
        return new InMemoryCache();
    }
}
