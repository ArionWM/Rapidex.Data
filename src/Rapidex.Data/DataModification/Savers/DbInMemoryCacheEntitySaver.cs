using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification.Savers;

internal class DbInMemoryCacheEntitySaver : IDbEntityUpdater
{
    public IDbEntityMetadata EntityMetadata { get; protected set; }

    public Task<IEntityUpdateResult> BulkUpdate(IDbEntityMetadata em, IQueryUpdater query)
    {
        throw new NotImplementedException();
    }

    public Task<IEntityUpdateResult> Delete(IDbEntityMetadata em, IEnumerable<long> ids)
    {
        throw new NotImplementedException();
    }

    public Task<IEntityUpdateResult> InsertOrUpdate(IDbEntityMetadata em, IEnumerable<IEntity> entities)
    {
        this.EntityMetadata = em;

        throw new NotImplementedException();
    }


}
