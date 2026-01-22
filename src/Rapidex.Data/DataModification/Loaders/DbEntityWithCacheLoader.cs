using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using SqlKata;

namespace Rapidex.Data.DataModification.Loaders;


internal class DbEntityWithCacheLoader : DbEntityLoaderBase, IDbEntityLoader
{
    protected IEntityLoadResult LoadWithCacheInternal(IDbEntityMetadata em, SqlResult result)
    {
        EntityDataJsonConverter.SetContext(this.ParentScope);
        try
        {
            var loadResultFromCache = Database.Cache.GetQuery(em, this.ParentScope, result);
            if (loadResultFromCache != null)
            {
                return loadResultFromCache;
            }
            return null;
        }
        finally
        {
            EntityDataJsonConverter.ClearContext();
        }
    }

    protected override IEntity FindWithCacheInternal(IDbEntityMetadata em, DbEntityId id)
    {
        EntityDataJsonConverter.SetContext(this.ParentScope);
        try
        {
            var entity = Database.Cache.GetEntity(this.ParentScope, em.Name, id.Id);

            if (id.Version > -1 && (entity == null || entity.DbVersion != id.Version))
            {
                return null;
            }

            return entity;
        }
        finally
        {
            EntityDataJsonConverter.ClearContext();
        }
    }

    protected override IEntity[] LoadWithCacheInternal(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        IEntity[] result = base.LoadWithCacheInternal(em, ids);
        if (em.CacheOptions.IsIdCacheEnabled)
        {
            var uncachedEntities = result.Where(e => e._loadSource == LoadSource.Database);
            if (uncachedEntities.Any())
                Database.Cache.SetEntities(uncachedEntities, em.CacheOptions.Expiration);
        }

        return result;
    }

    public override IEntityLoadResult Load(IDbEntityMetadata em, IQueryLoader loader, SqlResult compiledSql)
    {

        bool useQueryCache = (em.CacheOptions.IsQueryCacheEnabled || loader.ForceUseQueryCache) && !loader.ForceSkipQueryCache;
        if (useQueryCache)
        {
            var loadResultFromCache = this.LoadWithCacheInternal(em, compiledSql);
            if (loadResultFromCache != null)
            {
                return loadResultFromCache;
            }
        }

        IEntityLoadResult lastLoadResult = null;
        lastLoadResult = this.BaseDmProvider.Load(em, loader, compiledSql);
        if (lastLoadResult.IsNOTNullOrEmpty())
        {
            Database.Cache.SetEntities(lastLoadResult, em.CacheOptions.Expiration);

            if (useQueryCache)
            {
                Database.Cache.StoreQuery(em, this.ParentScope, compiledSql, lastLoadResult, em.CacheOptions.Expiration);
            }

            return lastLoadResult;
        }

        return lastLoadResult;
    }

    public override ILoadResult<DataRow> LoadRawInternal(IQueryLoader loader)
    {
        ILoadResult<DataRow> lastLoadResult = null;
        lastLoadResult = this.BaseDmProvider.LoadRaw(loader);
        if (lastLoadResult.IsNOTNullOrEmpty())
        {
            return lastLoadResult;
        }

        return lastLoadResult;
    }


}
