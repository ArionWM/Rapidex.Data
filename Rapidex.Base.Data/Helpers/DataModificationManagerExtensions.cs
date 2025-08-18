
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

        public static IEntity New(this IDbDataModificationManager dmm, string entityName)
        {
            dmm.NotNull();

            IDbEntityMetadata em = dmm.ParentScope.ParentDbScope.Metadata.Get(entityName);
            em.Should().NotBeNull($"Entity metadata not found for '{entityName}'");

            IEntity entity = dmm.New(em);
            return entity;

        }

        public static TEntity New<TEntity>(this IDbDataModificationManager dmm) where TEntity : IConcreteEntity
        {
            dmm.NotNull();

            IDbEntityMetadata em = dmm.ParentScope.ParentDbScope.Metadata.Get<TEntity>();
            em.Should().NotBeNull($"Entity metadata not found for {typeof(TEntity).Name}");

            IEntity entity = dmm.New(em.Name);
            TEntity concEntity = dmm.ParentScope.Mapper.Map<TEntity>(entity);

            return concEntity;
        }

        public static Task<IEntityLoadResult> Load(this IDbDataModificationManager dmm, IDbEntityMetadata em, Action<IQueryCriteria> act = null)
        {
            dmm.NotNull();

            var query = dmm.ParentScope.GetQuery(em);
            act?.Invoke(query);

            var loadResult = dmm.Load(query);
            return loadResult;
        }

        //public static Task<IEntityLoadResult> LoadAsync(this IDbDataModificationManager dmm, IDbEntityMetadata em, Action<IQueryCriteria> act = null)
        //{
        //    dmm.NotNull();
        //    return Task<IEntityLoadResult>.Run(() =>
        //    {
        //        return dmm.Load(em, act);
        //    });
        //}



        public static Task<IEntityLoadResult> Load(this IDbDataModificationManager dmm, string entityName, Action<IQueryCriteria> act = null)
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get(entityName);
            return dmm.Load(em, act);
        }

        //public static Task<IEntityLoadResult> LoadAsync(this IDbDataModificationManager dmm, string entityName, Action<IQueryCriteria> act = null)
        //{
        //    dmm.NotNull();
        //    return Task<IEntityLoadResult>.Run(() =>
        //    {
        //        return dmm.Load(entityName, act);
        //    });

        //}


        public static async Task<IEntityLoadResult<TEntity>> Load<TEntity>(this IDbDataModificationManager dmm, Action<IQueryCriteria> act = null) where TEntity : IConcreteEntity
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get<TEntity>();
            em.Should().NotBeNull($"Entity metadata not found for {typeof(TEntity).Name}");

            var loadResult = await dmm.Load(em, act);
            return loadResult.CastTo<TEntity>();
        }

        //public static Task<IEntityLoadResult<TEntity>> LoadAsync<TEntity>(this IDbDataModificationManager dmm, Action<IQueryCriteria> act = null) where TEntity : IConcreteEntity
        //{
        //    dmm.NotNull();
        //    return Task<IEntityLoadResult<TEntity>>.Run(() =>
        //    {
        //        return dmm.Load<TEntity>(act);
        //    });
        //}

        public static Task<IEntity> Find(this IDbDataModificationManager dmm, string entityName, long id)
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get(entityName);
            em.Should().NotBeNull($"Entity metadata not found for '{entityName}'");

            return dmm.Find(em, id);
        }

        //public static Task<IEntity> FindAsync(this IDbDataModificationManager dmm, string entityName, long id)
        //{
        //    dmm.NotNull();
        //    return Task<IEntity>.Run(() =>
        //    {
        //        return dmm.Find(entityName, id);
        //    });

        //}

        public static Task<TEntity> Find<TEntity>(this IDbDataModificationManager dmm, long id) where TEntity : IConcreteEntity
        {
            dmm.NotNull();

            var em = dmm.ParentScope.ParentDbScope.Metadata.Get<TEntity>();
            em.Should().NotBeNull($"Entity metadata not found for {typeof(TEntity).Name}");

            var result = dmm.Find(em, id);
            return result.ContinueWith<TEntity>(res => (TEntity)res.Result);
        }

        //public static Task<TEntity> FindAsync<TEntity>(this IDbDataModificationManager dmm, long id) where TEntity : IConcreteEntity
        //{
        //    dmm.NotNull();
        //    return Task<TEntity>.Run(() =>
        //    {
        //        return dmm.Find<TEntity>(id);
        //    });
        //}

        public static void Delete(this IDbDataModificationManager dmm, IEnumerable<IEntity> entities)
        {
            dmm.NotNull();
            foreach (var entity in entities)
            {
                dmm.Delete(entity);
            }
        }

        //public static Task DeleteAsync(this IDbDataModificationManager dmm, IEnumerable<IEntity> entities)
        //{
        //    return Task.Run(() =>
        //     {
        //         dmm.Delete(entities);
        //     });
        //}

        //public static Task<IEntity> FindAsync(this IDbDataModificationManager dmm, IDbEntityMetadata em, long id)
        //{
        //    return Task<IEntity>.Run(() =>
        //     dmm.Find(em, id)
        //     );
        //}

        //public static Task<IEntityLoadResult> LoadAsync(this IDbDataModificationManager dmm, IQueryLoader loader)
        //{
        //    return Task<IEntityLoadResult>.Run(() =>
        //    {
        //        return dmm.Load(loader);
        //    });
        //}

        //public static Task<ILoadResult<DataRow>> LoadRawAsync(this IDbDataModificationManager dmm, IQueryLoader loader)
        //{
        //    return Task<ILoadResult<DataRow>>.Run(() =>
        //    {
        //        return dmm.LoadRaw(loader);
        //    });
        //}

        //public static Task SaveAsync(this IDbDataModificationManager dmm, IEntity entity)
        //{
        //    return Task.Run(() =>
        //    {
        //        dmm.Save(entity);
        //    });
        //}

        //public static Task SaveAsync(this IDbDataModificationManager dmm, IEnumerable<IEntity> entities)
        //{
        //    return Task.Run(() =>
        //    {
        //        dmm.Save(entities);
        //    });
        //}

        //public static Task AddAsync(this IDbDataModificationManager dmm, IQueryUpdater updater)
        //{
        //    return Task.Run(() =>
        //    {
        //        dmm.Add(updater);
        //    });
        //}

        //public static Task DeleteAsync(this IDbDataModificationManager dmm, IEntity entity)
        //{
        //    return Task.Run(() =>
        //    {
        //        dmm.Delete(entity);
        //    });
        //}

        /// <summary>
        /// Write changes to database
        /// If transaction is available, changes will be written before commit transaction
        /// </summary>
        //public static Task<IEntityUpdateResult> CommitOrApplyChanges(IDbDataModificationManager dmm)
        //{
        //    dmm.NotNull();
        //    return Task<IEntityUpdateResult>.Run(() =>
        //    {
        //        return dmm.CommitOrApplyChanges();
        //    });


        //}


    }
}
