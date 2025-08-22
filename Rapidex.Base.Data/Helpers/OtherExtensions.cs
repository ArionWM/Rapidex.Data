using Rapidex.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data;

public static class OtherExtensions
{
    public static DbEntityId[] ToDbEntityIds(this IEntity[] entities)
    {
        return entities.Select(e => new DbEntityId((long)e.GetId(), e.DbVersion)).ToArray();

    }

    public static DbEntityId[] ToDbEntityIds(this IEntityLoadResult<IEntity> entities)
    {
        return entities.Select(e => new DbEntityId((long)e.GetId(), e.DbVersion)).ToArray();

    }

    public static string ToLogStr(this DbVariable variable)
    {
        return $"{variable.FieldName}: {variable.Value}";
    }

    public static string ToLogStr(this IEnumerable<DbVariable> variables)
    {
        return string.Join(", ", variables.Select(v => v.ToLogStr()));


    }


    public static IEntity[] Map(this EntityMapper mapper, IDbEntityMetadata em, IEnumerable<ObjDictionary> from)
    {
        return from.Select(r => mapper.MapToNew(em, r)).ToArray();
    }



    //public static IEntityLoadResult<TEntity> Cast<TEntity>(this IEntityLoadResult<IEntity> entities) where TEntity : IEntity
    //{
    //    var result = new EntityLoadResult<TEntity>(entities.Select(e => (TEntity)e));
    //    result.TotalCount = entities.TotalCount;
    //    result.PageCount = entities.PageCount;
    //    result.PageIndex = entities.PageIndex;
    //    result.PageSize = entities.PageSize;

    //    return result;
    //}

    public static bool IsEmptyId(this long id)
    {
        return id == DatabaseConstants.DEFAULT_EMPTY_ID || id == 0;
    }

    public static bool IsPrematureId(this long id)
    {
        return id < DatabaseConstants.DEFAULT_EMPTY_ID;
    }

    public static bool IsPersistedRecordId(this long id)
    {
        return id > 0;
    }

    public static bool HasPrematureId(this IEntity entity)
    {
        return ((long)entity.GetId()).IsPrematureId();
    }

    public static object EnsureLowerValue(this object value)
    {
        if (value is IDataType dt)
        {
            value = dt?.GetValueLower();
        }

        if (value is IEntity entity)
        {
            value = entity.GetId();
        }
        return value;
    }

    public static void Add(this PredefinedDataCollection coll, params IEntity[] entities)
    {
        var em = entities.First().GetMetadata();
        var valueDicts = entities.Select(ent => EntityMapper.MapToDict(em, ent));
        coll.Add(em, valueDicts.ToArray());
    }
}
