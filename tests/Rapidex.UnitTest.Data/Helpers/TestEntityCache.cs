using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Rapidex.Data.Cache;
using YamlDotNet.Core.Tokens;

namespace Rapidex.UnitTest.Data.Helpers;

internal class TestEntityCache : IEntityCache
{
    protected DefaultEntityInMemoryCache EntityMemoryCache { get; }
    protected DefaultEntityHybridCache EntityHybridCache { get; }

    public static bool MemoryCacheEnabled { get; set; } = false;
    public static bool HybridCacheEnabled { get; set; } = false;

    public TestEntityCache(IServiceProvider serviceProvider)
    {
        this.EntityMemoryCache = serviceProvider.GetRequiredService<DefaultEntityInMemoryCache>();
        this.EntityHybridCache = serviceProvider.GetRequiredService<DefaultEntityHybridCache>();
    }

    public async Task<T> Get<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, object id) where T : IEntity
    {
        if (MemoryCacheEnabled)
        {
            return await this.EntityMemoryCache.Get<T>(dbSchema, em, id);
        }

        if (HybridCacheEnabled)
        {
            return await this.EntityHybridCache.Get<T>(dbSchema, em, id);
        }

        return default;
    }

    public async Task<T[]> GetMultiple<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, string hash) where T : IEntity
    {
        if (MemoryCacheEnabled)
        {
            return await this.EntityMemoryCache.GetMultiple<T>(dbSchema, em, hash);
        }

        if (HybridCacheEnabled)
        {
            return await this.EntityHybridCache.GetMultiple<T>(dbSchema, em, hash);
        }
        return default;
    }

    public async Task Set<T>(T entity) where T : IEntity
    {
        if (MemoryCacheEnabled)
        {
            await this.EntityMemoryCache.Set(entity);
        }

        if (HybridCacheEnabled)
        {
            await this.EntityHybridCache.Set(entity);
        }
    }

    public async Task Set<T>(IDbSchemaScope dbSchema, IDbEntityMetadata em, string hash, IEnumerable<T> entities) where T : IEntity
    {
        if (MemoryCacheEnabled)
        {
            await this.EntityMemoryCache.Set(dbSchema, em, hash, entities);
        }

        if (HybridCacheEnabled)
        {
            await this.EntityHybridCache.Set(dbSchema, em, hash, entities);
        }
    }

    public async Task Remove<T>(T entity) where T : IEntity
    {
        if (MemoryCacheEnabled)
        {
            await this.EntityMemoryCache.Remove(entity);
        }

        if (HybridCacheEnabled)
        {
            await this.EntityHybridCache.Remove(entity);
        }
    }

    public async Task Remove(string key)
    {
        if (MemoryCacheEnabled)
        {
            await this.EntityMemoryCache.Remove(key);
        }

        if (HybridCacheEnabled)
        {
            await this.EntityHybridCache.Remove(key);
        }

    }

    public async Task RemoveByTag(string tag)
    {
        if (MemoryCacheEnabled)
        {
            await this.EntityMemoryCache.RemoveByTag(tag);
        }

        if (HybridCacheEnabled)
        {
            await this.EntityHybridCache.RemoveByTag(tag);
        }
    }
}
