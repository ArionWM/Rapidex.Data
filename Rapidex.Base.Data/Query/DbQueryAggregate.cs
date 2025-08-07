using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Query
{
    internal abstract class DbQueryAggregate : DbQueryLoader, IQueryAggregate
    {
        public DbQueryAggregate(IDbSchemaScope schema, IDbEntityMetadata em) : base(schema, em)
        {
        }

        public async Task<object> Avg(string field)
        {
            field = this.Schema.Structure.CheckObjectName(field);

            this.Query.ClearComponent("select");
            this.Query.AsAvg(field);

            var result = await this.Schema.Data.LoadRaw(this);
            DataRow row = result.FirstOrDefault();
            if (row == null)
                throw new InvalidOperationException($"Count invalid");

            object res = row[0];
            return res;
        }

        public async Task<long> Count()
        {
            this.Query.ClearComponent("select");
            this.Query.AsCount(new string[] { this.GetFieldName(this.EntityMetadata.PrimaryKey.Name) });
            //this.Query.AsCount(new string[] { this.EntityMetadata.PrimaryKey.Name });

            var result = await this.Schema.Data.LoadRaw(this);
            DataRow row = result.FirstOrDefault();
            if (row == null)
                throw new InvalidOperationException($"Count invalid");

            long count = row.To<long>(0);
            return count;
        }

        public async Task<bool> Exist()
        {
            SqlKata.Query _query = this.Query.Clone();
            _query.ClearComponent("select");
            _query.Select(this.GetFieldName(this.EntityMetadata.PrimaryKey.Name));
            _query.Limit(1);


            var result = await this.Schema.Data.LoadRaw(this);
            DataRow row = result.FirstOrDefault();
            if (row == null)
                return false;

            return true;
        }

        public async Task<object>Max(string field)
        {
            field = this.Schema.Structure.CheckObjectName(field);

            this.Query.ClearComponent("select");
            this.Query.AsMax(this.GetFieldName(field));

            var result = await this.Schema.Data.LoadRaw(this);
            DataRow row = result.FirstOrDefault();
            if (row == null)
                throw new InvalidOperationException($"Count invalid");

            object res = row[0];
            return res;
        }

        public async Task<object>Min(string field)
        {
            field = this.Schema.Structure.CheckObjectName(field);

            this.Query.ClearComponent("select");
            this.Query.AsMin(field);

            var result = await this.Schema.Data.LoadRaw(this);
            DataRow row = result.FirstOrDefault();
            if (row == null)
                throw new InvalidOperationException($"Count invalid");

            object res = row[0];
            return res;
        }

        public async Task<object>Sum(string field)
        {
            field = this.Schema.Structure.CheckObjectName(field);

            this.Query.ClearComponent("select");
            this.Query.AsSum(field);

            var result = await this.Schema.Data.LoadRaw(this);
            DataRow row = result.FirstOrDefault();
            if (row == null)
                throw new InvalidOperationException($"Count invalid");

            object res = row[0];
            return res;
        }
    }
}
