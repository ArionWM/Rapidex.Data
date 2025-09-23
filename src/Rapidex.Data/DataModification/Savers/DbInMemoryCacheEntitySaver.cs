using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.DataModification.Savers
{
    internal class DbInMemoryCacheEntitySaver : IDbEntityUpdater
    {
        public IDbEntityMetadata EntityMetadata { get; protected set; }

        public IEntityUpdateResult BulkUpdate(IDbEntityMetadata em, IQueryUpdater query)
        {
            throw new NotImplementedException();
        }

        public IEntityUpdateResult Delete(IDbEntityMetadata em, IEnumerable<long> ids)
        {
            throw new NotImplementedException();
        }

        public IEntityUpdateResult InsertOrUpdate(IDbEntityMetadata em, IEnumerable<IEntity> entities)
        {
            this.EntityMetadata = em;

            throw new NotImplementedException();
        }


    }
}
