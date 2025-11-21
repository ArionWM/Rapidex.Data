using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rapidex.Data.DataModification.Loaders;


internal class DbEntityWithCacheLoader : DbEntityLoaderBase, IDbEntityLoader
{
    public override IEntityLoadResult GetInternal(IQueryLoader loader)
    {
        EntityLoadResult lres = new EntityLoadResult();
        return lres;
    }

    public override ILoadResult<DataRow> GetInternalRaw(IQueryLoader loader)
    {
        LoadResult<DataRow> lres = new LoadResult<DataRow>();
        return lres;
    }

    protected override IEntity GetInternal(DbEntityId id)
    {
        return null;
        //throw new NotImplementedException();
    }
}
