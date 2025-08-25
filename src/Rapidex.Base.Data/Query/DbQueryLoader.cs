using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data;

namespace Rapidex.Data.Query
{
    internal class DbQueryLoader : DbQueryOrder, IQueryLoader
    {
        public DbQueryLoader(IDbSchemaScope schema, IDbEntityMetadata em) : base(schema, em)
        {
        }


        public async Task<IEntity> First()
        {
            this.Query.Skip(0).Take(1);
            return await this.Load()
                .ContinueWith<IEntity>(res => res.Result.FirstOrDefault());
        }

        public async Task<ILoadResult<DbEntityId>> GetIds()
        {
            //this.Query.Select(this.GetFieldName(this.EntityMetadata.PrimaryKey.Name), this.GetFieldName(CommonConstants.FIELD_VERSION));
            //string idFieldName = this.GetFieldName(this.EntityMetadata.PrimaryKey.Name);
            //string versionFieldName = this.GetFieldName(CommonConstants.FIELD_VERSION);
            var rawResult = await this.LoadPartial(this.EntityMetadata.PrimaryKey.Name, CommonConstants.FIELD_VERSION);

            List<DbEntityId> result = new List<DbEntityId>();
            foreach (DataRow row in rawResult)
            {
                result.Add(new DbEntityId(row.To<long>(this.EntityMetadata.PrimaryKey.Name), row.To<int>(CommonConstants.FIELD_VERSION)));
            }

            return new LoadResult<DbEntityId>(result);
        }

        public async Task<IEntityLoadResult> Load()
        {
            return await this.Schema.Data.Load(this);
        }

        public async Task<ILoadResult<DataRow>> LoadPartial(params string[] fields)
        {
            //Clone Query?
            this.Query.Select(this.GetFieldName(CommonConstants.FIELD_ID));
            foreach (string field in fields)
            {
                this.Query.Select(this.GetFieldName(field));
            }

            this.Query.Select(this.GetFieldName(CommonConstants.FIELD_VERSION));

            return await this.Schema.Data.LoadRaw(this);
        }

        public virtual object Clone()
        {
            IQueryLoader newLoader = TypeHelper.CreateInstance<IQueryLoader>(this.GetType(), this.Schema, this.EntityMetadata);
            newLoader.Query = this.Query.Clone();
            newLoader.Paging = this.Paging;
            newLoader.Order = this.Order;
            newLoader.Alias = this.Alias;
            return newLoader;

        }
    }
}
