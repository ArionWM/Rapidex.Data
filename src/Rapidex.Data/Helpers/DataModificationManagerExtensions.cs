
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

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

        IDbEntityMetadata em = dmm.ParentSchema.ParentDbScope.Metadata.Get(entityName);
        em.NotNull($"Entity metadata not found for '{entityName}'");

        IEntity entity = dmm.New(em);
        return entity;

    }

    public static TEntity New<TEntity>(this IDbDataModificationScope dmm) where TEntity : IConcreteEntity
    {
        dmm.NotNull();

        IDbEntityMetadata em = dmm.ParentSchema.ParentDbScope.Metadata.Get<TEntity>();
        em.NotNull($"Entity metadata not found for {typeof(TEntity).Name}");

        IEntity entity = dmm.New(em.Name);
        TEntity concEntity = dmm.ParentSchema.Mapper.Map<TEntity>(entity);

        return concEntity;
    }

    public static IEntityLoadResult Load(this IDbDataReadScope dmm, IDbEntityMetadata em, Action<IQueryCriteria> act = null)
    {
        dmm.NotNull();

        var query = dmm.ParentSchema.GetQuery(em);
        act?.Invoke(query);

        var loadResult = dmm.Load(query);
        return loadResult;
    }


    public static IEntityLoadResult Load(this IDbDataReadScope dmm, string entityName, Action<IQueryCriteria> act = null)
    {
        dmm.NotNull();

        var em = dmm.ParentSchema.ParentDbScope.Metadata.Get(entityName);
        return dmm.Load(em, act);
    }



    public static IEntityLoadResult<TEntity> Load<TEntity>(this IDbDataReadScope dmm, Action<IQueryCriteria> act = null) where TEntity : IConcreteEntity
    {
        dmm.NotNull();

        var em = dmm.ParentSchema.ParentDbScope.Metadata.Get<TEntity>();
        em.NotNull($"Entity metadata not found for {typeof(TEntity).Name}");

        var loadResult = dmm.Load(em, act);
        return loadResult.CastTo<TEntity>();
    }

    public static IEntity Find(this IDbDataReadScope dmm, string entityName, long id)
    {
        dmm.NotNull();

        var em = dmm.ParentSchema.ParentDbScope.Metadata.Get(entityName);
        em.NotNull($"Entity metadata not found for '{entityName}'");

        return dmm.Find(em, id);
    }


    public static TEntity Find<TEntity>(this IDbDataReadScope dmm, long id) where TEntity : IConcreteEntity
    {
        dmm.NotNull();

        var em = dmm.ParentSchema.ParentDbScope.Metadata.Get<TEntity>();
        em.NotNull($"Entity metadata not found for {typeof(TEntity).Name}");

        var result = dmm.Find(em, id);
        return (TEntity)result;
    }

    public static TEntity[] Find<TEntity>(this IDbDataReadScope dmm, params long[] ids) where TEntity : IConcreteEntity
    {
        dmm.NotNull();

        var em = dmm.ParentSchema.ParentDbScope.Metadata.Get<TEntity>();
        em.NotNull($"Entity metadata not found for {typeof(TEntity).Name}");

        var result = dmm.Find(em, ids);
        return result.Cast<TEntity>().ToArray();
    }


    public static void Delete(this IDbDataModificationScope dmm, IEnumerable<IEntity> entities)
    {
        dmm.NotNull();
        foreach (var entity in entities)
        {
            dmm.Delete(entity);
        }
    }

    public static void CheckActive(this IDbDataModificationScope dmm)
    {
        if (dmm == null)
            throw new InvalidOperationException("Data modification scope is null.");

        if (dmm.IsFinalized)
            throw new WorkScopeNotAvailableException(null, "This scope is finalized");
    }

    /// <summary>
    /// Load original entity and map partial entity values to it.
    /// </summary>
    /// <param name="pentity"></param>
    /// <returns></returns>
    public static IEntity ConvertToActualEntity(this IPartialEntity pentity)
    {
        pentity.NotNull();


        pentity._Schema.NotNull("Schema is not set in partial entity. Set or use abc");

        var em = pentity._Schema.ParentDbScope.Metadata.Get(pentity._TypeName);
        em.NotNull($"Entity metadata not found for '{pentity._TypeName}'");

        IEntity actualEnt;
        if (pentity._IsNew)
        {
            actualEnt = Database.EntityFactory.Create(em, pentity._Schema, true);
        }
        else
        {
            actualEnt = pentity._Schema.Find(em, pentity.GetId().As<long>());
        }

        pentity._Schema.Mapper.Map(em, pentity, actualEnt);

        return actualEnt;
    }

    /// <summary>
    /// if entity is partial entity, load actual entity and map values to it.
    /// </summary>
    /// <param name="pentity"></param>
    /// <returns></returns>
    public static IEntity EnsureForActualEntity(this IEntity ppEntity)
    {
        if (ppEntity is IPartialEntity pentity)
        {
            return pentity.ConvertToActualEntity();
        }

        return ppEntity;
    }

    public static bool IsAnyValueContained(this IPartialEntity pentity)
    {
        pentity.NotNull();

        var values = pentity.GetAllValues();
        if (values.Count > 1)
            return true;

        if (values.Count == 0)
            return false;

        var em = pentity.GetMetadata();
        var fieldName = values.First().Key;
        return string.Compare(em.PrimaryKey.Name, fieldName, true) != 0;
    }

}
