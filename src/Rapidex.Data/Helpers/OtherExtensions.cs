using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Rapidex.Data.Entities;
using Rapidex.Data.Helpers;

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

    public static bool IsPrematureOrEmptyId(this long id)
    {
        return id <= DatabaseConstants.DEFAULT_EMPTY_ID;
    }

    public static bool IsPersistedRecordId(this long id)
    {
        return id > 0;
    }

    public static bool HasPrematureId(this IEntity entity)
    {
        return ((long)entity.GetId()).IsPrematureId();
    }

    public static bool HasPrematureOrEmptyId(this IEntity entity)
    {
        return ((long)entity.GetId()).IsPrematureOrEmptyId();
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

        if (value is Enum enumVal)
        {
            value = Convert.ToInt64(enumVal);
        }

        return value;
    }

    public static void Add(this PredefinedDataCollection coll, params IEntity[] entities)
    {
        var em = entities.First().GetMetadata();
        var valueDicts = entities.Select(ent => EntityMapper.MapToDict(em, ent));
        coll.Add(em, valueDicts.ToArray());
    }


    public static void CanCreateDatabase(this IDbProvider provider)
    {
        using var ac = provider.GetAuthorizationChecker();
        if (!ac.CanCreateDatabase())
        {
            throw new DbInsufficientPermissionsException("CreateDatabase", ac.GetCurrentUserId());
        }
    }

    public static void CanCreateSchema(this IDbProvider provider)
    {
        using var ac = provider.GetAuthorizationChecker();
        if (!ac.CanCreateSchema())
        {
            throw new DbInsufficientPermissionsException("CreateSchema", ac.GetCurrentUserId());
        }
    }

    public static void CanCreateTable(this IDbProvider provider, string schemaName)
    {
        using var ac = provider.GetAuthorizationChecker();
        if (!ac.CanCreateTable(schemaName))
        {
            throw new DbInsufficientPermissionsException($"CreateTable in {schemaName}", ac.GetCurrentUserId());
        }
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public static IResult<int> Subscribe<T>(this IConcreteEntityImplementer<T> implementer, string @event, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
#pragma warning restore IDE0060 // Remove unused parameter
    {
        return Signal.Hub.SubscribeEntityReleatedForAllLevels(@event, nameof(T), handler);
    }

    public static IResult<int> SubscribeBeforeSave<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_BEFORESAVE, handler);
    }

    public static IResult<int> SubscribeAfterSave<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_AFTERSAVE, handler);
    }

    public static IResult<int> SubscribeAfterCommit<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_AFTERCOMMIT, handler);
    }

    public static IResult<int> SubscribeBeforeDelete<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_BEFOREDELETE, handler);
    }

    public static IResult<int> SubscribeAfterDelete<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_AFTERDELETE, handler);
    }

    public static IResult<int> SubscribeValidate<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_VALIDATE, handler);
    }

    public static IResult<int> SubscribeExecLogic<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_EXEC_LOGIC, handler);
    }

    public static IResult<int> SubscribeOnNew<T>(this IConcreteEntityImplementer<T> implementer, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler) where T : IConcreteEntity
    {
        return implementer.Subscribe<T>(DataReleatedSignalConstants.SIGNAL_NEW, handler);
    }

    public static void PrepareCommit(this IEntity entity, IDbDataModificationScope parentDms, DataUpdateType updateType)
    {
        entity.NotNull();
        if (!entity.IsAttached())
            throw new InvalidOperationException("Entity must be attached before preparing for commit.");

        var em = entity.GetMetadata();

        foreach (var fl in em.Fields)
        {
            //IDataType            fl.
            IDbFieldMetadata? fm = fl.Value;
            if (fm.Type.IsSupportTo<IDataType>())
            {
                var fieldValue = entity.GetValue(fm.Name) as IDataType;
                fieldValue?.PrepareCommit(entity, parentDms, updateType);
            }
        }
    }

    public static ByteArrayContent GetByteArrayContent(this BlobRecord blobRecord)
    {
        var content = new ByteArrayContent(blobRecord.Name, blobRecord.ContentType, blobRecord.Data);
        return content;
    }

    public static StreamContent GetStreamContent(this BlobRecord blobRecord)
    {
        var content = new StreamContent( blobRecord.Name, blobRecord.ContentType, new MemoryStream(blobRecord.Data));
        return content;
    }

   
}
