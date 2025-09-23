using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data
{


    public class EntityLoadResult<T> : LoadResult<T>, IEntityLoadResult<T> where T : IEntity
    {
        private List<T> _items = new List<T>();

        public EntityLoadResult()
        {

        }

        public EntityLoadResult(IEnumerable<T> items) : base(items)
        {
        }

        public EntityLoadResult(IEnumerable<T> items, long pageSize, long pageIndex, long pageCount, long totalCount) : base(items, pageSize, pageIndex, pageCount, totalCount)
        {
        }

        //public static implicit operator EntityLoadResult<T>(LoadResult<T> source)
        //{

        //    return new EntityLoadResult<T>(source, source.PageSize, source.PageIndex, source.PageCount, source.TotalCount);

        //}


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
