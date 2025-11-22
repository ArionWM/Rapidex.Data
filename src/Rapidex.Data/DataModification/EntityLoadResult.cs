using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{


    public class EntityLoadResult<T> : LoadResult<T>, IEntityLoadResult<T> where T : IEntity
    {
        public EntityLoadResult()
        {

        }

        public EntityLoadResult(IEnumerable<T> items) : base(items)
        {
        }

        public EntityLoadResult(IEnumerable<T> items, long pageSize, long pageIndex, long pageCount, long totalCount) : base(items, pageSize, pageIndex, pageCount, totalCount)
        {
        }
    }

    public class EntityLoadResult : EntityLoadResult<IEntity>, IEntityLoadResult, IEntityLoadResult<IEntity>
    {
        public EntityLoadResult()
        {
        }

        public EntityLoadResult(IEnumerable<IEntity> items) : base(items)
        {
        }

        public EntityLoadResult(IEnumerable<IEntity> items, long pageSize, long pageIndex, long pageCount, long totalCount) : base(items, pageSize, pageIndex, pageCount, totalCount)
        {
        }

        //public static implicit operator EntityLoadResult(EntityLoadResult<T> source) where T: 
        //{

        //}
    }
}
