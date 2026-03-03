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
        public virtual bool ForceUseQueryCache { get; protected set; }
        public virtual bool ForceSkipQueryCache { get; protected set; }

        public DbQueryLoader(IDbSchemaScope schema, IDbEntityMetadata em, int aliasNo) : base(schema, em, aliasNo)
        {
        }


        public async Task<IEntity> First()
        {
            this.Query.Skip(0).Take(1);
            var res = await this.Load();
            return res.FirstOrDefault();
        }

        public async Task<ILoadResult<DbEntityId>> GetIds()
        {
            //this.Query.Select(this.GetFieldName(this.EntityMetadata.PrimaryKey.Name), this.GetFieldName(DatabaseConstants.FIELD_VERSION));
            //string idFieldName = this.GetFieldName(this.EntityMetadata.PrimaryKey.Name);
            //string versionFieldName = this.GetFieldName(DatabaseConstants.FIELD_VERSION);
            var rawResult = await this.LoadPartial(this.EntityMetadata.PrimaryKey.Name, DatabaseConstants.FIELD_VERSION);

            List<DbEntityId> result = new List<DbEntityId>();
            foreach (DataRow row in rawResult)
            {
                result.Add(new DbEntityId(row.To<long>(this.EntityMetadata.PrimaryKey.Name), row.To<int>(DatabaseConstants.FIELD_VERSION)));
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
            this.Query.Select(this.GetFieldName(DatabaseConstants.FIELD_ID));
            foreach (string field in fields)
            {
                this.Query.Select(this.GetFieldName(field));
            }

            this.Query.Select(this.GetFieldName(DatabaseConstants.FIELD_VERSION));

            return await this.Schema.Data.LoadRaw(this);
        }

        public virtual object Clone()
        {
            IQueryLoader newLoader = TypeHelper.CreateInstance<IQueryLoader>(this.GetType(), this.Schema, this.EntityMetadata, this.queryAliasesNo);
            newLoader.Query = this.Query.Clone();
            newLoader.Paging = this.Paging;
            newLoader.Order = this.Order;
            newLoader.Alias = this.Alias;
            return newLoader;
        }

        public virtual IQueryLoader UseQueryCache()
        {
            this.ForceSkipQueryCache = false;
            this.ForceUseQueryCache = true;

            return this;
        }

        public virtual IQueryLoader SkipQueryCache()
        {
            this.ForceSkipQueryCache = true;
            this.ForceUseQueryCache = false;

            return this;
        }


    }

}
