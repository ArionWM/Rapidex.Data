using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rapidex.Data.DataModification.Loaders;


internal class DbEntityWithCacheLoader : DbEntityLoaderBase, IDbEntityLoader
{
    public override IEntityLoadResult LoadInternal(IQueryLoader loader)
    {
        IEntityLoadResult lastLoadResult = null;
        foreach (var sloader in this.SecondaryLoaders)
        {
            lastLoadResult = sloader.Load(loader);
            if (lastLoadResult.IsNOTNullOrEmpty())
            {
                Database.Cache.SetEntities(lastLoadResult);
                return lastLoadResult;
            }
        }

        return lastLoadResult;
    }

    public override ILoadResult<DataRow> LoadRawInternal(IQueryLoader loader)
    {
        ILoadResult<DataRow> lastLoadResult = null;
        foreach (var sloader in this.SecondaryLoaders)
        {
            lastLoadResult = sloader.LoadRaw(loader);
            if (lastLoadResult.IsNOTNullOrEmpty())
            {
                return lastLoadResult;
            }
        }

        return lastLoadResult;
    }

    protected override IEntity GetInternal(IDbEntityMetadata em, DbEntityId id)
    {
        var entity = Database.Cache.GetEntity(this.ParentScope, em.Name, id.Id);

        if (entity == null || entity.DbVersion != id.Version)
        {
            return null;
        }

        return entity;
    }
}
