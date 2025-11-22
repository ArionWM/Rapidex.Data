using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Query
{
    internal abstract class DbQueryOrder : DbQueryPager, IQueryOrder
    {
        public DbQueryOrder(IDbSchemaScope schema, IDbEntityMetadata em, int aliasNo) : base(schema, em, aliasNo)
        {
            this.Order = new OrderCollection();
        }

        public OrderCollection Order { get; set; }

        public IQueryOrder OrderBy(OrderDirection direction, params string[] fields)
        {
            foreach (var field in fields)
            {
                this.Order.Add(new OrderInfo(field, direction));

                switch (direction)
                {
                    case OrderDirection.Asc:
                        this.Query.OrderBy(this.GetFieldName(field));
                        break;
                    case OrderDirection.Desc:
                        this.Query.OrderByDesc(this.GetFieldName(field));
                        break;
                }
            }
            return this;
        }
    }
}
