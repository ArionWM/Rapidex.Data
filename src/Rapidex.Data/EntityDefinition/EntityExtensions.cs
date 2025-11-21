
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Rapidex.Data.Exceptions;

namespace Rapidex.Data;

public static class EntityExtensions
{
    //public static IEntityCollection Details(this IEntity entity, string relationName, IDbCriteria criteria)
    //{
    //    throw new NotImplementedException();
    //}

    public static string ToJson(this IEntity entity, bool pretty, int deep = 0)
    {
        throw new NotImplementedException();
    }

    public static string ToJson(this IEnumerable<IEntity> entity, bool pretty, int deep = 0)
    {
        throw new NotImplementedException();
    }

    public static IEntity Clone(this IEntity entity, IDbSchemaScope targetScope = null)
    {
        if (entity == null)
            return null;

        EntityMapper mapper = targetScope?.Mapper ?? entity._Schema.Mapper;
        return mapper.Clone(entity, targetScope);
    }

    public static IEntity[] Clone(this IEnumerable<IEntity> entities, IDbSchemaScope targetScope = null)
    {
        return entities.Select(e => e.Clone(targetScope)).ToArray();
    }

    public static bool IsUseful(this PropertyInfo inf)
    {
        if (inf.GetIndexParameters().Length > 0)
            return false;

        if (inf.PropertyType.IsInterface)
            return false;

        if (inf.Name.StartsWith('_'))
            return false;

        return true;
    }

    public static void Save(this IEntity entity)
    {
        entity.NotNull();

        if (!entity.IsAttached())
            throw new EntityAttachScopeException("AttachNeeded", "Entity is not attached to any schema");

        entity._Schema.CurrentWork.NotNull("No active work found in current scope");

        entity._Schema.CurrentWork.Save(entity);
    }

    public static void Save(this IEnumerable<IEntity> entities)
    {
        if (!entities.Any())
            return;

        //TODO: Farklı scope'lar için ayrı ayrı kaydetme yapılabilir mi?
        IDbSchemaScope scope = entities.FirstOrDefault()._Schema;

        scope.CurrentWork.NotNull("No active work found in current scope");
        scope.CurrentWork.Save(entities);
    }

    public static void Attach(this IDbDataModificationScope scope, IEnumerable<IEntity> entities)
    {
        foreach (IEntity ent in entities)
            scope.Attach(ent);
    }

    public static void SaveOn(this IEntity entity, IDbDataModificationScope scope)
    {
        scope.Attach(entity);
        entity.Save();
    }

    public static void SaveOn(this IEnumerable<IEntity> entities, IDbDataModificationScope scope)
    {
        scope.Attach(entities);
        entities.Save();
    }


    public static void EnsureDataTypeInitialization(this IEntity entity)
    {
        entity.NotNull();
        entity._Schema.NotNull();

        if (entity is IPartialEntity partialEntity)
            entity._Schema.Mapper.EnsureAdvancedDataTypesForPartial(partialEntity);
        else
            entity._Schema.Mapper.EnsureAdvancedDataTypes(entity);
    }

    public static string Caption(this IEntity entity)
    {
        var em = entity.GetMetadata();
        string caption = null;
        if (em.Caption != null)
        {
            caption = entity[em.Caption.Name]?.ToString();
        }
        else
        {
            caption = entity.ToString();

            if (caption == null)
            {
                caption = $"{entity._TypeName} / {entity.GetId()}";
            }
        }

        return caption;
    }

    public static bool IsEqual(this IEntity entityA, IEntity entityB)
    {
        bool isEq = entityA.GetId().As<long>() == entityB.GetId().As<long>() &&
             entityA._DbName == entityB._DbName &&
             entityA._SchemaName == entityB._SchemaName &&
             entityA._TypeName == entityB._TypeName &&
             string.Equals(entityA._TypeName, entityB._TypeName, StringComparison.InvariantCultureIgnoreCase);

        return isEq;
    }

    public static RelationOne2N RelationOne2N(this IEntity entityA, string relationFieldName)
    {
        RelationOne2N relation = entityA.GetValue<RelationOne2N>(relationFieldName);
        relation.NotNull($"Relation '{relationFieldName}' is not found");
        return relation;
    }

    public static RelationN2N RelationN2N(this IEntity entityA, string relationFieldName)
    {
        RelationN2N relation = entityA.GetValue<RelationN2N>(relationFieldName);
        relation.NotNull($"Relation '{relationFieldName}' is not found");
        return relation;
    }

    public static void Add(this RelationN2N rel, IEnumerable<IEntity> entities)
    {
        foreach (var entity in entities)
        {
            rel.Add(entity);
        }
    }

    public static void Add(this RelationOne2N rel, IEnumerable<IEntity> entities)
    {
        foreach (var entity in entities)
        {
            rel.Add(entity);
        }
    }

    public static IDictionary<long, IEntity> ToIdDict(this IEnumerable<IEntity> entities)
    {
        if (entities == null)
            return new Dictionary<long, IEntity>();
        var dict = new Dictionary<long, IEntity>();

        foreach (var entity in entities)
        {
            if (entity == null)
                continue;
            long id = entity.GetId().As<long>();
            dict[id] = entity;
        }
        return dict;
    }

    public static bool IsAttached(this IEntity entity)
    {
        return entity._Schema.IsNOTNullOrEmpty();
    }
}
