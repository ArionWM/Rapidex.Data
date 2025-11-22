using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using SqlKata;

namespace Rapidex.Data.DataModification.Loaders;


internal class DbEntityWithCacheLoader : DbEntityLoaderBase, IDbEntityLoader
{
    protected IEntityLoadResult LoadInternal(IDbEntityMetadata em, SqlResult result)
    {
        var loadResultFromCache = Database.Cache.GetQuery(em, this.ParentScope, result);
        if (loadResultFromCache != null)
        {
            return loadResultFromCache;
        }
        return null;
    }

    public override IEntityLoadResult Load(IDbEntityMetadata em, SqlResult result)
    {

        if (em.CacheOptions.IsQueryCacheEnabled)
        {
            var loadResultFromCache = this.LoadInternal(em, result);
            if (loadResultFromCache != null)
            {
                return loadResultFromCache;
            }
        }

        IEntityLoadResult lastLoadResult = null;
        lastLoadResult = this.BaseDmProvider.Load(em, result);
        if (lastLoadResult.IsNOTNullOrEmpty())
        {
            Database.Cache.SetEntities(lastLoadResult);

            if (em.CacheOptions.IsQueryCacheEnabled)
            {
                Database.Cache.StoreQuery(em, this.ParentScope, result, lastLoadResult);
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

    protected override IEntity GetInternal(IDbEntityMetadata em, DbEntityId id)
    {
        var entity = Database.Cache.GetEntity(this.ParentScope, em.Name, id.Id);

        if (id.Version > -1 && (entity == null || entity.DbVersion != id.Version))
        {
            return null;
        }

        return entity;
    }
}
