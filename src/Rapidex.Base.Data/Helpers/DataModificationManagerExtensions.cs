
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data
{
    public static class DataModificationManagerExtensions
    {
        public static IEntityLoadResult<TEntity> CastTo<TEntity>(this IEntityLoadResult entities) where TEntity : IConcreteEntity
        {
            EntityLoadResult<TEntity> result = new EntityLoadResult<TEntity>(entities.ToArray().Cast<TEntity>());
            result.TotalItemCount = entities.TotalItemCount;
            result.PageCount = entities.PageCount;
            result.PageIndex = entities.PageIndex;
            result.PageSize = entities.PageSize;

            return result;
        }

        public static IEntityLoadResult<TEntity> CastTo<TEntity>(this IEntityLoadResult<IEntity> entities) where TEntity : IConcreteEntity
        {
            EntityLoadResult<TEntity> result = new EntityLoadResult<TEntity>(entities.ToArray().Cast<TEntity>());
            result.TotalItemCount = entities.TotalItemCount;
            result.PageCount = entities.PageCount;
            result.PageIndex = entities.PageIndex;
            result.PageSize = entities.PageSize;

            return result;
        }

        public static IEntityLoadResult<TEntity> CastTo<SEntity, TEntity>(this IEntityLoadResult<SEntity> entities) where TEntity : IEntity where SEntity : IConcreteEntity
        {
            EntityLoadResult<TEntity> result = new EntityLoadResult<TEntity>(entities.ToArray().Cast<TEntity>());
            result.TotalItemCount = entities.TotalItemCount;
            result.PageCount = entities.PageCount;
            result.PageIndex = entities.PageIndex;
            result.PageSize = entities.PageSize;

            return result;
        }

        public static IEntity New(this IDbDataModificationScope dmm, string entityName)
        {
            dmm.NotNull();

            IDbEntityMetadata em = dmm.ParentScope.ParentDbScope.Metadata.Get(entityName);
            em.Should().NotBeNull($"Entity metadata not found for '{entityName}'");

            IEntity entity = dmm.New(em);
            return entity;

        }

        public static TEntity New<TEntity>(this IDbDataModificationScope dmm) where TEntity : IConcreteEntity
        {
            dmm.NotNull();

            IDbEntityMetadata em = dmm.ParentScope.ParentDbScope.Metadata.Get<TEntity>();
            em.Should().NotBeNull($"Entity metadata not found for {typeof(TEntity).Name}");

            IEntity entity = dmm.New(em.Name);
            TEntity concEntity = dmm.ParentScope.Mapper.Map<TEntity>(entity);

            return concEntity;
        }

        public static IEntityLoadResult Load(this IDbDataReadScope dmm, IDbEntityMetadata em, Action<IQueryCriteria> act = null)
        {
            dmm.NotNull();

            var query = dmm.ParentScope.GetQuery(em);
            act?.Invoke(query);

            var loadResult = dmm.Load(query);
            return loadResult;
        }


        public static IEntityLoadResult Load(this IDbDataReadScope dmm, string entityName, Action<IQueryCriteria> act = null)
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get(entityName);
            return dmm.Load(em, act);
        }



        public static IEntityLoadResult<TEntity> Load<TEntity>(this IDbDataReadScope dmm, Action<IQueryCriteria> act = null) where TEntity : IConcreteEntity
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get<TEntity>();
            em.Should().NotBeNull($"Entity metadata not found for {typeof(TEntity).Name}");

            var loadResult = dmm.Load(em, act);
            return loadResult.CastTo<TEntity>();
        }

        public static IEntity Find(this IDbDataReadScope dmm, string entityName, long id)
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get(entityName);
            em.Should().NotBeNull($"Entity metadata not found for '{entityName}'");

            return dmm.Find(em, id);
        }


        public static TEntity Find<TEntity>(this IDbDataReadScope dmm, long id) where TEntity : IConcreteEntity
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get<TEntity>();
            em.Should().NotBeNull($"Entity metadata not found for {typeof(TEntity).Name}");

            var result = dmm.Find(em, id);
            return (TEntity)result;
        }


        public static void Delete(this IDbDataModificationScope dmm, IEnumerable<IEntity> entities)
        {
            dmm.NotNull();
            foreach (var entity in entities)
            {
                dmm.Delete(entity);
            }
        }



    }
}
