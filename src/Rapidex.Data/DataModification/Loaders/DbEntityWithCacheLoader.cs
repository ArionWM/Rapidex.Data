using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlKata;

namespace Rapidex.Data.DataModification.Loaders;


internal class DbEntityWithCacheLoader : DbEntityLoaderBase, IDbEntityLoader
{

    protected async Task<IEntityLoadResult> LoadWithCacheInternal(IDbEntityMetadata em, SqlResult result)
    {
        EntityDataJsonConverter.SetContext(this.ParentScope);
        try
        {
            var loadResultFromCache = await Database.Cache.GetQuery(em, this.ParentScope, result);
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

    protected override async Task<IEntity> FindWithCacheInternal(IDbEntityMetadata em, DbEntityId id)
    {
        EntityDataJsonConverter.SetContext(this.ParentScope);
        try
        {
            var entity = await Database.Cache.GetEntity(this.ParentScope, em.Name, id.Id);

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

    protected override async Task<IEntity[]> LoadWithCacheInternal(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        IEntity[] result = await base.LoadWithCacheInternal(em, ids);
        if (em.CacheOptions.IsIdCacheEnabled)
        {
            var uncachedEntities = result.Where(e => e._loadSource == LoadSource.Database);
            if (uncachedEntities.Any())
               await Database.Cache.SetEntities(uncachedEntities);
        }

        return result;
    }


    public override async Task<IEntityLoadResult> Load(IDbEntityMetadata em, IQueryLoader loader, SqlResult compiledSql)
    {

        bool useQueryCache = (em.CacheOptions.IsQueryCacheEnabled || loader.ForceUseQueryCache) && !loader.ForceSkipQueryCache;
        if (useQueryCache)
        {
            var loadResultFromCache = await this.LoadWithCacheInternal(em, compiledSql);
            if (loadResultFromCache != null)
            {
                return loadResultFromCache;
            }
        }

        IEntityLoadResult lastLoadResult = null;
        lastLoadResult = await this.BaseDmProvider.Load(em, loader, compiledSql);
        if (lastLoadResult.IsNOTNullOrEmpty())
        {
           await Database.Cache.SetEntities(lastLoadResult);

            if (useQueryCache)
            {
                //this.StoreQuery(Database.Cache, em, this.ParentScope, compiledSql, lastLoadResult);
                await Database.Cache.StoreQuery(em, this.ParentScope, compiledSql, lastLoadResult);
            }

            return lastLoadResult;
        }

        return lastLoadResult;
    }

    public override async Task<ILoadResult<DataRow>> LoadRawInternal(IQueryLoader loader)
    {
        ILoadResult<DataRow> lastLoadResult = null;
        lastLoadResult = await this.BaseDmProvider.LoadRaw(loader);
        if (lastLoadResult.IsNOTNullOrEmpty())
        {
            return lastLoadResult;
        }

        return lastLoadResult;
    }


}
