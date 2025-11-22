using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex.Data.Query
{
    internal abstract class DbQueryBase : IQueryBase
    {
        public enum QMode
        {
            Select,
            Update,
        }

        protected QMode Mode { get; set; } = QMode.Select;

        internal int queryAliasesNo = 0;
        internal int queryAliasesSubIndex = 0;

        public IDbSchemaScope Schema { get; set; }
        public IDbEntityMetadata EntityMetadata { get; set; }
        public string Alias { get; set; }
        public string TableName { get; protected set; }
        public SqlKata.Query Query { get; set; }




        protected DbQueryBase()
        {

        }

        protected DbQueryBase(IDbSchemaScope schema, IDbEntityMetadata em, int aliasNo)
        {
            schema.NotNull("Schema can't be null");
            em.NotNull("Metadata can't be null");

            this.Schema = schema;
            this.EntityMetadata = em;
            this.queryAliasesNo = aliasNo;
            this.Alias = em.Name.AbbrFromFirstLetters() + this.queryAliasesNo.ToString("00") + this.GetAliasSubIndex().ToString("00"); //Tekilliği garanti edilmeli

            this.Query = new SqlKata.Query();

            string schemaName = this.Schema.Structure.CheckObjectName(this.Schema.SchemaName);
            string tableName = this.Schema.Structure.CheckObjectName(em.TableName);

            if (em.OnlyBaseSchema)
                schemaName = this.Schema.Structure.CheckObjectName(DatabaseConstants.DEFAULT_SCHEMA_NAME);

            this.TableName = $"{schemaName}.{tableName}";

            string tableNameWithAlias = $"{this.TableName} as {this.Alias}";

            this.Query.From(tableNameWithAlias).As(this.Alias);
        }

        internal int GetAliasSubIndex()
        {
            Interlocked.Increment(ref this.queryAliasesSubIndex);
            return this.queryAliasesSubIndex;
        }

        protected string GetFieldName(string field)
        {
            field.NotEmpty();

            field = this.Schema.Structure.CheckObjectName(field);

            if (this.Mode == QMode.Update)
                return field;
            return $"{this.Alias}.{field}";
        }

        public void EnterUpdateMode()
        {
            this.Mode = QMode.Update;
        }

    }
}
