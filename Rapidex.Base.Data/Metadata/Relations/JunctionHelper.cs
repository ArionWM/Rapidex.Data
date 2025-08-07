using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Rapidex.Data.RelationN2N;

namespace Rapidex.Data.Metadata.Relations
{
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
            IDbEntityMetadata junctionEm = Database.Metadata.Get(junctionEntityName).NotNull();
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
            IDbEntityMetadata junctionEm = Database.Metadata.Get(fm.JunctionEntityName) ?? Database.Metadata.AddPremature(fm.JunctionEntityName);

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

        public static async Task<bool> Exist(IDbSchemaScope dbScema, IDbEntityMetadata jEm, string sourceFieldName, string targetFieldName, long entityAId, long entityBId) //TODO: Additional query
        {
            //Self relation?
            bool isExist = await dbScema.GetQuery(jEm)
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

        public static async Task<bool> Exist(IDbSchemaScope dbSchema, IDbEntityMetadata jEm, string sourceFieldName, string targetFieldName, IEntity entityA, IEntity entityB) //TODO: Additional query
        {
            //Self relation?
            bool isExist = await dbSchema.GetQuery(jEm)
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

        public static Task<bool> Exist(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IEntity entityB)
        {
            var jEm = Database.Metadata.Get(fm.JunctionEntityName);

            string sourceFieldName = GetJunctionSourceFieldName(fm);
            string targetFieldName = GetJunctionTargetFieldName(fm);

            sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
            targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);

            return Exist(dbSchema, jEm, sourceFieldName, targetFieldName, entityA, entityB);
        }

        public static async Task<IEntity> AddRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, long entityAId, long entityBID, bool directSave)
        {
            var jEm = Database.Metadata.Get(fm.JunctionEntityName);

            string sourceFieldName = GetJunctionSourceFieldName(fm);
            string targetFieldName = GetJunctionTargetFieldName(fm);

            sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
            targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);

            bool isExist = await Exist(dbSchema, jEm, sourceFieldName, targetFieldName, entityAId, entityBID);

            if (!isExist)
            {
                IEntity jEntity = dbSchema.New(fm.JunctionEntityName);
                jEntity[sourceFieldName] = entityAId;
                jEntity[targetFieldName] = entityBID;

                jEntity.EnsureDataTypeInitialization();

                if (directSave)
                    jEntity.Save();

                return jEntity;
            }

            return null;
        }

        public static Task<IEntity> AddRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IEntity entityB, bool directSave)
        {
            if (entityA == null || entityB == null)
                throw new ArgumentNullException("Entity A or Entity B cannot be null");

            return AddRelation(dbSchema, fm, entityA.GetId().As<long>(), entityB.GetId().As<long>(), directSave);
            
        }

        public static async Task<IPartialEntity> RemoveRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, long entityAId, long entityBId, bool directSave)
        {
            var jEm = Database.Metadata.Get(fm.JunctionEntityName);

            string sourceFieldName = GetJunctionSourceFieldName(fm);
            string targetFieldName = GetJunctionTargetFieldName(fm);

            sourceFieldName = dbSchema.Structure.CheckObjectName(sourceFieldName);
            targetFieldName = dbSchema.Structure.CheckObjectName(targetFieldName);

            IEntity ent = await dbSchema.GetQuery(jEm)
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

            if (ent != null)
            {
                if (directSave)
                {
                    dbSchema.Delete(ent);
                }
                else
                {
                    IPartialEntity entForDelete = Database.EntityFactory.CreatePartial(dbSchema, jEm, false, true).NotNull(); //TODO: PartialEntityFactory kullanılacak
                    entForDelete.SetId(entForDelete.GetId());

                    return entForDelete;
                }

            }

            return null;
        }

        public static async Task RemoveRelation(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IEntity entityB, bool directSave)
        {
            if (entityA == null || entityB == null)
                throw new ArgumentNullException("Entity A or Entity B cannot be null");

            await RemoveRelation(dbSchema, fm, entityA.GetId().As<long>(), entityB.GetId().As<long>(), directSave);


        }


        public static async Task<IEntityLoadResult> GetJunctionEntities(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, Action<IQueryCriteria> additionalCriterias = null)
        {
            var jEm = Database.Metadata.Get(fm.JunctionEntityName);

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

            var loadResult = await query.Load();

            return loadResult;
        }

        public static void SetEntitiesCriteria(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, IQueryCriteria query, Action<IQueryCriteria> additionalCriterias = null)
        {
            var jEm = Database.Metadata.Get(fm.JunctionEntityName).NotNull();
            var tEm = Database.Metadata.Get(fm.TargetEntityName).NotNull();

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

        public static async Task<IEntityLoadResult> GetEntities(IDbSchemaScope dbSchema, VirtualRelationN2NDbFieldMetadata fm, IEntity entityA, Action<IQueryCriteria> additionalCriterias = null)
        {
            var tEm = Database.Metadata.Get(fm.TargetEntityName).NotNull();
            IQueryCriteria query = dbSchema.GetQuery(tEm);
            SetEntitiesCriteria(dbSchema, fm, entityA, query, additionalCriterias);

            var loadResult = await ((IQuery)query).Load();

            return loadResult;
        }

        //public static bool IsExist(IDbSchemaScope dbScema, VirtualRelationN2NDbFieldMetadata fm, long entityAId, long entityBId)
        //{
        //    var jEm = Database.Metadata.Get(fm.JunctionEntityName).NotNull();


        //    string sourceFieldName = GetJunctionSourceFieldName(fm);
        //    string targetFieldName = GetJunctionTargetFieldName(fm);


        //    IQuery query = dbScema.GetQuery(jEm);
        //    query
        //        .Or(
        //            q => q.And(
        //                q1 => q1.Eq(sourceFieldName, entityAId),
        //                q2 => q2.Eq(targetFieldName, entityBId)
        //            ),
        //            q => q.And(
        //                q1 => q1.Eq(sourceFieldName, entityBId),
        //                q2 => q2.Eq(targetFieldName, entityAId)
        //            )
        //        );

        //    return query.Exist();

        //}

    }
}
