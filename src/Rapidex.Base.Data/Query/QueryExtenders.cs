using FluentAssertions.Equivalency;
using Rapidex.Data.Metadata.Relations;
using Rapidex.Data.Query;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Rapidex.Data.RelationN2N;
using static Rapidex.Data.RelationOne2N;

namespace Rapidex.Data
{
    public static class QueryExtenders
    {
        public static T Map<T>(this T qc, string entityName) where T : IQueryCriteria
        {
            var em = qc.Schema.ParentDbScope.Metadata.Get(entityName);
            qc.Map(em);
            return qc;
        }

        public static T Map<T, Entity>(this T qc)
            where Entity : IConcreteEntity
            where T : IQueryCriteria
        {
            var em = qc.Schema.ParentDbScope.Metadata.Get<Entity>();
            qc.Map(em);
            return qc;
        }

        public static T NotEq<T>(this T qc, string field, object value) where T : IQueryCriteria
        {
            return (T)qc.Not(q => q.Eq(field, value));


        }

        public static T NotLike<T>(this T qc, string field, object value) where T : IQueryCriteria
        {
            return (T)qc.Not(q => q.Like(field, value));
        }

        public static T NotIn<T>(this T qc, string field, params object[] values) where T : IQueryCriteria
        {
            return (T)qc.Not(q => q.In(field, values));
        }

        public static T NotBetween<T>(this T qc, string field, object value1, object value2) where T : IQueryCriteria
        {
            return (T)qc.Not(q => q.Between(field, value1, value2));
        }

        public static T Native<T>(this T qc, string native) where T : IQueryCriteria
        {
            throw new NotImplementedException();
        }

        public static T Kata<T>(this T qc, Action<SqlKata.Query> kataQuery) where T : IQueryCriteria
        {
            throw new NotImplementedException();
        }

        //public static Entity FindA<T>(this T qc, long id) 
        //    where T : IQuery<Entity> 
        //    where Entity :IConcreteEntity
        //{
        //    return (Entity)qc.Eq(CommonConstants.FIELD_ID, id).First();
        //}

        public static IEntity Find<T>(this T qc, long id)
                where T : IQuery
        {
            return qc.Eq(CommonConstants.FIELD_ID, id).First();

        }


        public static IQuery IdEq(this IQuery qo, long id)
        {
            return qo.Eq(CommonConstants.FIELD_ID, id);
        }

        public static IQuery<T> IdEq<T>(this IQuery<T> qo, long id) where T : IConcreteEntity
        {
            return qo.Eq(CommonConstants.FIELD_ID, id);
        }


        public static IQueryOrder Asc(this IQueryOrder qo, params string[] fields)
        {
            return qo.OrderBy(OrderDirection.Asc, fields);
        }

        public static IQueryOrder Desc(this IQueryOrder qo, params string[] fields)
        {
            return qo.OrderBy(OrderDirection.Desc, fields);
        }


        public static IQuery Asc(this IQuery qo, params string[] fields)
        {
            return qo.OrderBy(OrderDirection.Asc, fields);
        }

        public static IQuery Desc(this IQuery qo, params string[] fields)
        {
            return qo.OrderBy(OrderDirection.Desc, fields);
        }

        public static IQuery<T> Asc<T>(this IQuery<T> qo, params string[] fields) where T : IConcreteEntity
        {
            return qo.OrderBy(OrderDirection.Asc, fields);
        }

        public static IQuery<T> Desc<T>(this IQuery<T> qo, params string[] fields) where T : IConcreteEntity
        {
            return qo.OrderBy(OrderDirection.Desc, fields);
        }


        //IsActive


        public static IQuery GetQuery(this IDbDataReadScope dmm, string entityName)
        {
            var em = dmm.ParentSchema.ParentDbScope.Metadata.Get(entityName)
                .NotNull($"Metadata for '{entityName}' not found");
            return dmm.GetQuery(em);
        }


        public static IDbCriteriaParser FindParser(this IEnumerable<IDbCriteriaParser> parsers, string filter)
        {
            foreach (var parser in parsers)
            {
                if (parser.IsYours(filter))
                    return parser;
            }

            throw new InvalidOperationException($"No parser found for filter '{filter}'");

        }

        public static IQueryCriteria ReleatedN2N(this IQueryCriteria crit, IEntity parentEntity, string releatedField)
        {
            var parentEm = parentEntity.GetMetadata();
            var parentFm = parentEm.Fields.Get(releatedField);
            var relFm = parentFm as VirtualRelationN2NDbFieldMetadata;
            relFm.NotNull($"{parentEm.Name} / {releatedField} is not available or not N2N relation");

            var targetEm = crit.Schema.ParentDbScope.Metadata.Get(relFm.TargetEntityName)
                .NotNull($"Metadata for '{relFm.TargetEntityName}' not found");

            //if (crit.EntityMetadata != relFm.TargetEntityMetadata)
            //    throw new InvalidOperationException($"Field '{releatedField}' parent metadata different ('{crit.EntityMetadata.Name}')");

            JunctionHelper.SetEntitiesCriteria(crit.Schema, relFm, parentEntity, crit);

            return crit;
        }

        public static IQuery<T> ReleatedN2N<T>(this IQuery<T> crit, IEntity parentEntity, string releatedField) where T : IConcreteEntity
        {
            return ((IQueryCriteria)crit).ReleatedN2N(parentEntity, releatedField) as IQuery<T>;
        }

        public static IQueryCriteria ReleatedOne2N(this IQueryCriteria crit, IEntity parentEntity, string releatedField)
        {

            var parentEm = parentEntity.GetMetadata();
            var parentFm = parentEm.Fields.Get(releatedField);
            var relFm = parentFm as VirtualRelationOne2NDbFieldMetadata;
            relFm.NotNull($"{parentEm.Name} / {releatedField} is not available or not One2N relation");

            var detailFm = crit.EntityMetadata.Fields.Get(relFm.DetailParentFieldName);

            if (crit.EntityMetadata != relFm.ReferencedEntityMetadata)
                throw new InvalidOperationException($"Field '{releatedField}' is not a reference field in entity '{crit.EntityMetadata.Name}'");

            crit.Eq(detailFm.Name, parentEntity.GetId());

            return crit;
        }

        public static IQuery<T> ReleatedOne2N<T>(this IQuery<T> crit, IEntity parentEntity, string releatedField) where T : IConcreteEntity
        {
            return ((IQueryCriteria)crit).ReleatedOne2N(parentEntity, releatedField) as IQuery<T>;
        }

        public static IQueryCriteria Related(this IQueryCriteria crit, IEntity parentEntity, string releatedField)
        {
            var parentEm = parentEntity.GetMetadata();
            var parentFm = parentEm.Fields.Get(releatedField);

            switch (parentFm)
            {
                case VirtualRelationOne2NDbFieldMetadata relFm:
                    crit.ReleatedOne2N(parentEntity, releatedField);
                    break;
                case VirtualRelationN2NDbFieldMetadata relFm:
                    crit.ReleatedN2N(parentEntity, releatedField);
                    break;
                default:
                    throw new InvalidOperationException($"Field '{releatedField}' is not a reference field in entity '{crit.EntityMetadata.Name}'");
            }

            return crit;
        }

        public static IQuery<T> Related<T>(this IQuery<T> crit, IEntity parentEntity, string releatedField) where T : IConcreteEntity
        {
            return ((IQueryCriteria)crit).Related(parentEntity, releatedField) as IQuery<T>;
        }

        public static T Sum<T>(this IQueryAggregate query, string field)
        {
            return query.Sum(field).As<T>();
        }

        public static T Min<T>(this IQueryAggregate query, string field)
        {
            return query.Min(field).As<T>();
        }

        public static T Max<T>(this IQueryAggregate query, string field)
        {
            return query.Max(field).As<T>();
        }

        public static T Avg<T>(this IQueryAggregate query, string field)
        {
            return query.Avg(field).As<T>();
        }



    }
}
