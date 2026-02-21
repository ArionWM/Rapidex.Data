using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Rapidex.Data.RelationN2N;

namespace Rapidex.Data.Metadata.Relations;

internal class JunctionHelper
{
    public const string DEFAULT_JUNCTION_ENTITY_NAME = "GenericJunction";
    public const string FIELD_SELF_RELATION = "Self";

    public static string GetJunctionSourceFieldName(VirtualRelationN2NDbFieldMetadata fm)
    {
        if (fm.TargetEntityName == fm.ParentMetadata.Name)
        {
            return FIELD_SELF_RELATION;
        }

        return fm.JunctionSourceFieldName;
    }

    public static string GetJunctionTargetFieldName(VirtualRelationN2NDbFieldMetadata fm)
    {
        return fm.JunctionTargetFieldName;
    }



    public static void AddJunctionFields(IDbEntityMetadata sourceEm, IDbEntityMetadata targetEm, string junctionEntityName = DEFAULT_JUNCTION_ENTITY_NAME)
    {
        IDbEntityMetadata junctionEm = sourceEm.Parent.DbScope.Metadata.Get(junctionEntityName).NotNull();
        string sourceFieldName = sourceEm.Name.ToFriendly();
        string targetFieldName = targetEm.Name.ToFriendly();

        junctionEm.AddFieldIfNotExist(sourceFieldName, "reference",
            new ObjDictionary
            {
                ["reference"] = sourceEm.Name,
            }
        );

        junctionEm.AddFieldIfNotExist(targetFieldName, "reference",
                new ObjDictionary
                {
                    ["reference"] = targetEm.Name,
                }
            );
    }


    public static void AddJunctionFields(VirtualRelationN2NDbFieldMetadata fm)
    {
        fm.ParentMetadata.NotNull("Parent metadata cannot be null");

        IDbEntityMetadata junctionEm = fm.ParentMetadata.Parent.DbScope.Metadata.Get(fm.JunctionEntityName) ?? fm.ParentMetadata.Parent.DbScope.Metadata.AddPremature(fm.JunctionEntityName);

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        junctionEm.AddFieldIfNotExist(sourceFieldName, "reference",
            new ObjDictionary
            {
                ["reference"] = fm.ParentMetadata.Name.ToFriendly(),
            }
        );

        junctionEm.AddFieldIfNotExist(targetFieldName, "reference",
                new ObjDictionary
                {
                    ["reference"] = fm.TargetEntityName,
                }
            );
    }

    public static bool Exist(IDbSchemaScope dbScema, IDbEntityMetadata jEm, string sourceFieldName, string targetFieldName, long entityAId, long entityBId) //TODO: Additional query
    {
        //Self relation?
        bool isExist = dbScema.GetQuery(jEm)
              .Or(
                  q => q.And(
                      q1 => q1.Eq(sourceFieldName, entityAId),
                      q2 => q2.Eq(targetFieldName, entityBId)
                  ),
                  q => q.And(
                      q1 => q1.Eq(sourceFieldName, entityBId),
                      q2 => q2.Eq(targetFieldName, entityAId)
                  )
              )
              .Exist();

        return isExist;
    }

    public static bool Exist(IDbSchemaScope dbSchema, IDbEntityMetadata jEm, string sourceFieldName, string targetFieldName, IEntity entityA, IEntity entityB) //TODO: Additional query
    {
        //Self relation?
        bool isExist = dbSchema.GetQuery(jEm)
              .Or(
                  q => q.And(
                      q1 => q1.Eq(sourceFieldName, entityA),
                      q2 => q2.Eq(targetFieldName, entityB)
                  ),
                  q => q.And(
                      q1 => q1.Eq(sourceFieldName, entityB),
                      q2 => q2.Eq(targetFieldName, entityA)
                  )
              )
              .Exist();

        return isExist;
    }

    public static bool Exist(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IEntity entityB)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
        targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);

        return Exist(dbSchema, jEm, sourceFieldName, targetFieldName, entityA, entityB);
    }

    public static IEntity AddRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, long entityAId, long entityBID, bool directSave)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
        targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);

        bool isExist = Exist(dbSchema, jEm, sourceFieldName, targetFieldName, entityAId, entityBID);

        if (!isExist)
        {
            var junctionEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);
            IEntity jEntity = Database.EntityFactory.Create(junctionEm, dbSchema, true);
            jEntity[sourceFieldName] = entityAId;
            jEntity[targetFieldName] = entityBID;

            jEntity.EnsureDataTypeInitialization();

            if (directSave)
            {
                dbSchema.CurrentWork.Attach(jEntity);
                jEntity.Save();
            }

            return jEntity;
        }

        return null;
    }

    public static IEntity AddRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IEntity entityB, bool directSave = false)
    {
        if (entityA == null || entityB == null)
            throw new ArgumentNullException("Entity A or Entity B cannot be null");

        return AddRelation(dbSchema, fm, entityA.GetId().As<long>(), entityB.GetId().As<long>(), directSave);

    }

    public static IEntity Get(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, long entityAId, long entityBId)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
        targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);

        IEntity ent = dbSchema.GetQuery(jEm)
             .Or(
                 q => q.And(
                     q1 => q1.Eq(sourceFieldName, entityAId),
                     q2 => q2.Eq(targetFieldName, entityBId)
                 ),
                 q => q.And(
                     q1 => q1.Eq(sourceFieldName, entityBId),
                     q2 => q2.Eq(targetFieldName, entityAId)
                 )
             )
             .First();

        return ent;
    }


    public static IQuery GetJunctionQuery(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, long entityAId, long[] entityBIds)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
        targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);


        var query = dbSchema
              .GetQuery(jEm)
              .EnterUpdateMode()
              .Or(
                   q => q.And(
                       q1 => q1.Eq(sourceFieldName, entityAId),
                       q2 => q2.In(targetFieldName, entityBIds)
                   ),
                   q => q.And(
                       q1 => q1.In(sourceFieldName, entityBIds),
                       q2 => q2.Eq(targetFieldName, entityAId)
                   )
               );

        return query;
    }

    public static IQuery GetJunctionQuery(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, long entityAId)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
        targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);


        var query = dbSchema
              .GetQuery(jEm)
              .EnterUpdateMode()
              .Or(
                   q => q.And(
                       q1 => q1.Eq(sourceFieldName, entityAId),
                       q2 => q2.NotEq(targetFieldName, null)
                   ),
                   q => q.And(
                       q1 => q1.NotEq(sourceFieldName, null),
                       q2 => q2.Eq(targetFieldName, entityAId)
                   )
               );

        return query;
    }

    public static IPartialEntity RemoveRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, long entityAId, long entityBId, bool directSave)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);

        var ent = Get(dbSchema, fm, entityAId, entityBId);

        if (ent != null)
        {
            if (directSave)
            {
                dbSchema.CurrentWork.Delete(ent);
            }
            else
            {
                IPartialEntity entForDelete = Database.EntityFactory.CreatePartial(jEm, dbSchema, false, true).NotNull(); //TODO: PartialEntityFactory kullanılacak
                entForDelete.SetId(entForDelete.GetId());

                return entForDelete;
            }

        }

        return null;
    }

    public static void RemoveRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IEntity entityB, bool directSave)
    {
        if (entityA == null || entityB == null)
            throw new ArgumentNullException("Entity A or Entity B cannot be null");

        RemoveRelation(dbSchema, fm, entityA.GetId().As<long>(), entityB.GetId().As<long>(), directSave);
    }


    public static IEntityLoadResult GetJunctionEntities(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, Action<IQueryCriteria> additionalCriterias = null)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName);

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
        targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);

        IQuery query = dbSchema.GetQuery(jEm)
              .And(
                     q1 => q1.Eq(sourceFieldName, entityA),
                     q2 => q2.NotEq(targetFieldName, null)
                 );

        additionalCriterias?.Invoke(query);

        var loadResult = query.Load();

        return loadResult;
    }

    public static void SetEntitiesCriteria(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IQueryCriteria query, Action<IQueryCriteria> additionalCriterias = null)
    {
        var jEm = dbSchema.ParentDbScope.Metadata.Get(fm.JunctionEntityName).NotNull();
        var tEm = dbSchema.ParentDbScope.Metadata.Get(fm.TargetEntityName).NotNull();

        string sourceFieldName = GetJunctionSourceFieldName(fm);
        string targetFieldName = GetJunctionTargetFieldName(fm);

        sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
        targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);
        string primaryKeyName = dbSchema.Structure.CheckObjectName(tEm.PrimaryKey.Name);

        query
                .Nested(primaryKeyName, targetFieldName, jEm,
                    q => q.And(
                         q1 => q1.Eq(sourceFieldName, entityA),
                         q2 => q2.NotEq(targetFieldName, null)
                     ));

        additionalCriterias?.Invoke(query);

    }

    public static IEntityLoadResult GetEntities(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, Action<IQueryCriteria> additionalCriterias = null)
    {
        var tEm = dbSchema.ParentDbScope.Metadata.Get(fm.TargetEntityName).NotNull();
        IQueryCriteria query = dbSchema.GetQuery(tEm);
        SetEntitiesCriteria(dbSchema, fm, entityA, query, additionalCriterias);

        var loadResult = ((IQuery)query).Load();

        return loadResult;
    }

}
