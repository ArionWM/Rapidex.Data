using Rapidex.Data.Metadata.Relations;
using Rapidex.Data.Query;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static Rapidex.Data.Reference;
using static Rapidex.Data.RelationN2N;
using static Rapidex.Data.RelationOne2N;

namespace Rapidex.Data.Query
{
    internal class DbQueryCriteria : DbQueryBase, IQueryCriteria
    {


        public DbQueryCriteria(IDbSchemaScope schema, IDbEntityMetadata em) : base(schema, em)
        {
        }


        public IQueryCriteria Between(string field, object value1, object value2)
        {
            this.Query.WhereBetween(this.GetFieldName(field), value1.EnsureLowerValue(), value2.EnsureLowerValue());
            return this;
        }

        public IQueryCriteria Eq(string field, object value)
        {
            if (value == null)
                this.Query.WhereNull(this.GetFieldName(field));
            else
                this.Query.Where(this.GetFieldName(field), value.EnsureLowerValue());
            return this;

        }

        /// <summary>
        /// field > value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IQueryCriteria Gt(string field, object value)
        {
            field.NotEmpty();
            value.NotNull();

            this.Query.Where(this.GetFieldName(field), ">", value.EnsureLowerValue());
            return this;
        }

        /// <summary>
        /// field >= value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IQueryCriteria GtEq(string field, object value)
        {
            field.NotEmpty();
            value.NotNull();

            this.Query.Where(this.GetFieldName(field), ">=", value.EnsureLowerValue());
            return this;

        }

        public IQueryCriteria In(string field, params object[] values)
        {
            object[] _values = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
                _values[i] = values[i].EnsureLowerValue();

            this.Query.WhereIn(this.GetFieldName(field), _values);
            return this;

        }

        public IQueryCriteria Like(string field, object value)
        {
            field.NotEmpty();
            value.NotNull();

            string val = value.EnsureLowerValue().ToString();
            val = val.Replace('*', '%');
            this.Query.WhereLike(this.GetFieldName(field), val);
            return this;
        }

        /// <summary>
        /// field < value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IQueryCriteria Lt(string field, object value)
        {
            field.NotEmpty();
            value.NotNull();

            this.Query.Where(this.GetFieldName(field), "<", value.EnsureLowerValue());
            return this;

        }

        /// <summary>
        /// field <= value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IQueryCriteria LtEq(string field, object value)
        {
            field.NotEmpty();
            value.NotNull();

            this.Query.Where(this.GetFieldName(field), "<=", value.EnsureLowerValue());
            return this;
        }

        public IQueryCriteria Nested(string entityAFieldName, string entityBFieldName, IDbEntityMetadata refEm, Action<IQueryCriteria> nestedCriteria)
        {
            DbQueryCriteria refQuery = new DbQueryCriteria(this.Schema, refEm);

            nestedCriteria.NotNull();

            entityAFieldName = this.Schema.Structure.CheckObjectName(entityAFieldName);
            entityBFieldName = this.Schema.Structure.CheckObjectName(entityBFieldName);

            nestedCriteria?.Invoke(refQuery);
            var innerComponents = refQuery.Query.GetComponents("where");

            foreach (var component in innerComponents)
            {
                this.Query.AddComponent("where", component);
            }

            //https://sqlkata.com/docs/join
            this.Query.Join(refQuery.TableName + " as " + refQuery.Alias, join =>
            {
                string masterRefName = $"{this.Alias}.{entityAFieldName}";
                string detailRefName = $"{refQuery.Alias}.{entityBFieldName}";

                join.On(masterRefName, detailRefName);
                return join;
            });


            return this;
        }

        public IQueryCriteria Nested(string referenceField, Action<IQueryCriteria> nestedCriteria)
        {
            IDbFieldMetadata fm = this.EntityMetadata.Fields[referenceField];
            if (fm == null)
                throw new InvalidOperationException($"Field '{referenceField}' not found in entity '{this.EntityMetadata.Name}'");

            if (!fm.Type.IsSupportTo<IReference>())
                throw new InvalidOperationException($"Field '{referenceField}' is not a reference field in entity '{this.EntityMetadata.Name}'");

            ReferenceDbFieldMetadata refFm = fm as ReferenceDbFieldMetadata;
            if (refFm == null)
                throw new InvalidOperationException($"Field '{referenceField}' is not a reference field in entity '{this.EntityMetadata.Name}'");

            IDbEntityMetadata refEm = refFm.ReferencedEntityMetadata.EnsureIsNotPremature(this.Schema.ParentDbScope);
            if(refEm.IsPremature)
                throw new InvalidOperationException($"Field '{referenceField}' is referred metada is premature '{refEm.Name}'");

            return this.Nested(fm.Name, refEm.PrimaryKey.Name, refEm, nestedCriteria);
        }




        public IQueryCriteria Not(Action<IQueryCriteria> act)
        {
            DbQueryCriteria newCrit = new DbQueryCriteria(this.Schema, this.EntityMetadata);
            newCrit.Alias = this.Alias;
            act(newCrit);
            this.Query.WhereNot(c => newCrit.Query);
            return this;
        }

        public IQueryCriteria And(params Action<IQueryCriteria>[] acts)
        {
            DbQueryCriteria newCrit = new DbQueryCriteria(this.Schema, this.EntityMetadata);
            newCrit.Alias = this.Alias;
            foreach (var act in acts)
            {
                act(newCrit);
            }
            this.Query.Where(c => newCrit.Query);
            return this;

        }

        public IQueryCriteria Or(params Action<IQueryCriteria>[] acts)
        {
            this.Query.Where(q =>
            {


                foreach (var act in acts)
                {
                    DbQueryCriteria newCrit = new DbQueryCriteria(this.Schema, this.EntityMetadata);
                    newCrit.Alias = this.Alias;

                    act(newCrit);
                    q.OrWhere(c => newCrit.Query);
                }

                return q;
            });


            return this;
        }

    }
}
