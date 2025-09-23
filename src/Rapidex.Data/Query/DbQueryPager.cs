using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Query
{
    internal abstract class DbQueryPager : DbQueryCriteria, IQueryPager
    {

        public IPaging Paging { get; set; }

        public DbQueryPager(IDbSchemaScope schema, IDbEntityMetadata em) : base(schema, em)
        {
            this.Paging = new PagingInfo();
        }

        public IQueryPager Page(long pageSize, long startIndex, bool includeTotalCount = true)
        {
            this.Paging.PageSize = pageSize;
            this.Paging.StartIndex = startIndex;
            this.Paging.IncludeTotalItemCount = includeTotalCount;
            this.Query.Skip(Convert.ToInt32(startIndex)).Take(Convert.ToInt32(pageSize)); //:(
            return this;

        }

        public IQueryPager ClearPaging()
        {
            this.Query.ClearComponent("offset");
            this.Query.ClearComponent("limit");
            return this;
        }
    }
}
