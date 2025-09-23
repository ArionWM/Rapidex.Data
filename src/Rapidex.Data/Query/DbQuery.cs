using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Query
{
    internal class DbQuery : DbQueryAggregate, IQuery
    {
        public ObjDictionary UpdateData { get; internal set; }

        public DbQuery(IDbSchemaScope schema, IDbEntityMetadata em) : base(schema, em)
        {
        }

        IQuery IQuery.And(params Action<IQueryCriteria>[] acts)
        {
            return (IQuery)base.And(acts);

        }

        IQuery IQuery.Between(string field, object value1, object value2)
        {
            return (IQuery)base.Between(field, value1, value2);

        }

        IQuery IQuery.Eq(string field, object value)
        {
            return (IQuery)base.Eq(field, value);
        }

        IQuery IQuery.Gt(string field, object value)
        {
            return (IQuery)base.Gt(field, value);
        }

        IQuery IQuery.GtEq(string field, object value)
        {
            return (IQuery)base.GtEq(field, value);
        }

        IQuery IQuery.In(string field, params object[] values)
        {
            return (IQuery)base.In(field, values);
        }

        IQuery IQuery.Like(string field, object value)
        {
            return (IQuery)base.Like(field, value);
        }

        IQuery IQuery.Lt(string field, object value)
        {
            return (IQuery)base.Lt(field, value);

        }

        IQuery IQuery.LtEq(string field, object value)
        {
            return (IQuery)base.LtEq(field, value);

        }

        IQuery IQuery.Nested(string referenceField, Action<IQueryCriteria> nestedCriteria)
        {
            return (IQuery)base.Nested(referenceField, nestedCriteria);
        }

        IQuery IQuery.Nested(string entityAFieldName, string entityBFieldName, IDbEntityMetadata referenceEntityMetadata, Action<IQueryCriteria> nestedCriteria)
        {
            return (IQuery)base.Nested(entityAFieldName, entityBFieldName, referenceEntityMetadata, nestedCriteria);
        }

        //IQuery IQuery.Releated(string releatedField, IEntity parentEntity, Action<IQueryCriteria> additionalCriteria = null)
        //{
        //    return (IQuery)base.Releated(releatedField, parentEntity, additionalCriteria);
        //}

        IQuery IQuery.Not(Action<IQueryCriteria> act)
        {
            return (IQuery)base.Not(act);
        }

        IQuery IQuery.Or(params Action<IQueryCriteria>[] acts)
        {
            return (IQuery)base.Or(acts);
        }

        public override object Clone()
        {
            IQuery newQuery = (IQuery)base.Clone();
            return newQuery;
        }

        IQuery IQuery.OrderBy(OrderDirection direction, params string[] fields)
        {
            return (IQuery)base.OrderBy(direction, fields);

        }

        IQuery IQuery.Page(long pageSize, long skip, bool includeTotalCount = true)
        {
            return (IQuery)base.Page(pageSize, skip, includeTotalCount);
        }

        IQuery IQuery.ClearPaging()
        {
            return (IQuery)base.ClearPaging();
        }

        public void Update(IDbDataModificationScope workScope, ObjDictionary data)
        {
            if (this.Mode != QMode.Update)
                throw new InvalidOperationException("Query is not in update mode");

            workScope.CheckActive();

            this.UpdateData = new ObjDictionary();

            this.Alias = null;
            this.Query.QueryAlias = null;
            this.Query.ClearComponent("from");
            this.Query.From(this.TableName);

            foreach (string fieldName in data.Keys)
            {
                if (!this.EntityMetadata.Fields.ContainsKey(fieldName))
                    throw new InvalidOperationException($"Field '{fieldName}' not found in entity '{this.EntityMetadata.Name}'");

                IDbFieldMetadata fm = this.EntityMetadata.Fields[fieldName];

                if (!fm.IsPersisted)
                    throw new InvalidOperationException($"Field '{fieldName}' is not persisted in entity '{this.EntityMetadata.Name}'");

                object rawValue = data[fieldName];
                object lowerValue = rawValue.EnsureLowerValue();

                string _fieldName = this.Schema.Structure.CheckObjectName(fieldName);

                this.UpdateData.Add(_fieldName, lowerValue);
            }

            workScope.Add(this);
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public new IQuery EnterUpdateMode()
        {
            base.EnterUpdateMode();
            return this;
        }
    }

    internal class Query<T> : DbQuery, IQuery<T> where T : IConcreteEntity
    {
        public Query(IDbSchemaScope schema) : base(schema, schema.ParentDbScope.Metadata.Get<T>().NotNull($"'{typeof(T).Name}' metadata not found"))
        {
        }

        public Query(IDbSchemaScope schema, IDbEntityMetadata em) : base(schema, em)
        {
        }

        public T Find(long id)
        {
            return this.Schema.Find<T>(id);
        }



        IQuery<T> IQuery<T>.And(params Action<IQueryCriteria>[] acts)
        {
            return (IQuery<T>)base.And(acts);
        }

        IQuery<T> IQuery<T>.Between(string field, object value1, object value2)
        {
            return (IQuery<T>)base.Between(field, value1, value2);
        }

        IQuery<T> IQuery<T>.Eq(string field, object value)
        {
            return (IQuery<T>)base.Eq(field, value);
        }

        T IQueryLoader<T>.First()
        {
            return (T)base.First();
        }

        IQuery<T> IQuery<T>.Gt(string field, object value)
        {
            return (IQuery<T>)base.Gt(field, value);
        }

        IQuery<T> IQuery<T>.GtEq(string field, object value)
        {
            return (IQuery<T>)base.GtEq(field, value);
        }

        IQuery<T> IQuery<T>.In(string field, params object[] values)
        {
            return (IQuery<T>)base.In(field, values);
        }

        IQuery<T> IQuery<T>.Like(string field, object value)
        {
            return (IQuery<T>)base.Like(field, value);
        }

        IEntityLoadResult<T> IQueryLoader<T>.Load()
        {
            IEntityLoadResult res = base.Load();
            var lres = res.CastTo<T>();
            return lres;
        }

        IQuery<T> IQuery<T>.Lt(string field, object value)
        {
            return (IQuery<T>)base.Lt(field, value);
        }

        IQuery<T> IQuery<T>.LtEq(string field, object value)
        {
            return (IQuery<T>)base.LtEq(field, value);
        }

        IQuery<T> IQuery<T>.Nested(string referenceField, Action<IQueryCriteria> nestedCriteria)
        {
            return (IQuery<T>)base.Nested(referenceField, nestedCriteria);
        }

        IQuery<T> IQuery<T>.Nested(string entityAFieldName, string entityBFieldName, IDbEntityMetadata refEm, Action<IQueryCriteria> nestedCriteria)
        {
            return (IQuery<T>)base.Nested(entityAFieldName, entityBFieldName, refEm, nestedCriteria);
        }

        //IQuery<T> IQuery<T>.Releated(string releatedField, IEntity parentEntity, Action<IQueryCriteria> additionalCriteria = null)
        //{
        //    return (IQuery<T>)base.Releated(releatedField, parentEntity, additionalCriteria);
        //}

        IQuery<T> IQuery<T>.Not(Action<IQueryCriteria> act)
        {
            return (IQuery<T>)base.Not(act);
        }

        IQuery<T> IQuery<T>.Or(params Action<IQueryCriteria>[] acts)
        {
            return (IQuery<T>)base.Or(acts);
        }

        IQuery<T> IQuery<T>.OrderBy(OrderDirection direction, params string[] fields)
        {
            return (IQuery<T>)base.OrderBy(direction, fields);
        }

        IQuery<T> IQuery<T>.Page(long pageSize, long skip, bool includeTotalCount = true)
        {
            return (IQuery<T>)base.Page(pageSize, skip);
        }

        IQuery<T> IQuery<T>.ClearPaging()
        {
            return (IQuery<T>)base.ClearPaging();
        }

        public new IQuery<T> EnterUpdateMode()
        {
            base.EnterUpdateMode();
            return this;
        }


    }
}

