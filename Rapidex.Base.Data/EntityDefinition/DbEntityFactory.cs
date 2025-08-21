
using Rapidex.Data.Scopes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex.Data
{
    public class DbEntityFactory
    {
        public class TemplateInfo
        {
            public IEntity Entity { get; set; }
            public string PersistentSequence { get; set; }
            public string PrematureSequence { get; set; }
        }

        protected Dictionary<string, TemplateInfo> Templates { get; set; } = new Dictionary<string, TemplateInfo>();
        protected ReaderWriterLockSlim _templateLock = new ReaderWriterLockSlim();


        protected DbEntity Create(IDbEntityMetadata entityMetadata, IDbSchemaScope? scope)
        {
            DbEntity dbEntity = new DbEntity();
            dbEntity._Scope = scope;
            dbEntity._TypeName = entityMetadata.Name;
            dbEntity._DbName = scope?.ParentDbScope.Name;
            dbEntity._SchemaName = scope?.SchemaName;
            dbEntity.Id = DatabaseConstants.DEFAULT_EMPTY_ID;
            dbEntity.ExternalId = null;
            dbEntity.DbVersion = 0;
            return dbEntity;
        }

        protected PartialEntity CreatePartial(IDbEntityMetadata entityMetadata, IDbSchemaScope? scope)
        {
            PartialEntity dbEntity = new PartialEntity();
            dbEntity._Scope = scope;
            dbEntity._TypeName = entityMetadata.Name;
            dbEntity._DbName = scope?.ParentDbScope.Name;
            dbEntity._SchemaName = scope?.SchemaName;
            dbEntity.Id = DatabaseConstants.DEFAULT_EMPTY_ID;
            dbEntity.ExternalId = null;
            dbEntity.DbVersion = 0;
            return dbEntity;
        }

        protected TemplateInfo CreateTemplate(IDbEntityMetadata entityMetadata, IDbSchemaScope scope)
        {
            DbEntity dbEntity = this.Create(entityMetadata, scope);

            TemplateInfo templateInfo = new TemplateInfo();
            templateInfo.Entity = dbEntity;

            templateInfo.PersistentSequence = "id_" + entityMetadata.Name;
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PersistentSequence, -1, 10000);

            templateInfo.PrematureSequence = templateInfo.PersistentSequence + "_prm";
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PrematureSequence, -1, 10000);

            return templateInfo;
        }

        protected IEntity CreateConcrete(IDbEntityMetadata entityMetadata, IDbSchemaScope? scope)
        {
            Type concreteType = Common.Assembly.FindType(entityMetadata.ConcreteTypeName);
            IEntity dbEntity = TypeHelper.CreateInstance<IEntity>(concreteType);

            dbEntity._Scope = scope;
            dbEntity._TypeName = entityMetadata.Name;
            dbEntity._DbName = scope?.ParentDbScope.Name;
            dbEntity._SchemaName = scope?.SchemaName;
            dbEntity.SetId(0L);
            dbEntity.ExternalId = null;
            dbEntity.DbVersion = 0;
            return dbEntity;
        }

        protected TemplateInfo CreateConcreteTemplate(IDbEntityMetadata entityMetadata, IDbSchemaScope scope)
        {
            IEntity dbEntity = this.CreateConcrete(entityMetadata, scope);

            TemplateInfo templateInfo = new TemplateInfo();
            templateInfo.Entity = dbEntity;

            templateInfo.PersistentSequence = "id_" + entityMetadata.Name;
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PersistentSequence, -1, 10000);

            templateInfo.PrematureSequence = templateInfo.PersistentSequence + "_prm";
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PrematureSequence, -1, 10000);

            return templateInfo;
        }

        public TemplateInfo GetTemplate(IDbEntityMetadata em, IDbSchemaScope dbScope)
        {
            string key = dbScope.ParentDbScope.Name + "." + dbScope.SchemaName + "." + em.Name;

            _templateLock.EnterUpgradeableReadLock();
            try
            {
                if (!this.Templates.ContainsKey(key))
                {
                    _templateLock.EnterWriteLock();
                    try
                    {
                        if (em.ConcreteTypeName.IsNullOrEmpty())
                            this.Templates.Add(key, this.CreateTemplate(em, dbScope));
                        else
                            this.Templates.Add(key, this.CreateConcreteTemplate(em, dbScope));
                    }
                    finally
                    {
                        _templateLock.ExitWriteLock();
                    }
                }
                return this.Templates[key];
            }

            finally
            {
                _templateLock.ExitUpgradeableReadLock();
            }
        }

        public IEntity Create(IDbEntityMetadata em, IDbSchemaScope dbScope, bool forNew)
        {
            em.NotNull();
            dbScope.NotNull();

            IEntity newEntity = null;

            if (em.ConcreteTypeName.IsNullOrEmpty())
            {
                newEntity = this.Create(em, dbScope);
            }
            else
            {
                newEntity = this.CreateConcrete(em, dbScope);
            }

            newEntity._Metadata = em;

            if (forNew)
            {
                //dbScope.NotNull("Scope required for new entitites");

                TemplateInfo templateInfo = GetTemplate(em, dbScope);

                newEntity._IsNew = true;
                long id = dbScope.Data.Sequence(templateInfo.PrematureSequence).GetNext();
                id = id * -1;
                newEntity.SetId(id);
            }

            newEntity.EnsureDataTypeInitialization();

            return newEntity;
        }

        public IEntity Create<T>(IDbSchemaScope dbScope, bool forNew) where T : IConcreteEntity
        {
            IDbEntityMetadata em = dbScope.ParentDbScope.Metadata.Get<T>();
            return this.Create(em, dbScope, forNew);
        }


        //TODO: Test, pEntity.Values boş olmalı 
        public IPartialEntity CreatePartial(IDbEntityMetadata em, IDbSchemaScope dbScope, bool forNew, bool forDeleted)
        {
            em.NotNull();

            IPartialEntity newEntity = null;

            newEntity = this.CreatePartial(em, dbScope);

            newEntity._Metadata = em;

            if (forNew)
            {
                TemplateInfo templateInfo = GetTemplate(em, dbScope);

                newEntity._IsNew = true;
                long id = dbScope.Data.Sequence(templateInfo.PrematureSequence).GetNext();
                id = id * -1;
                newEntity.SetId(id);
            }

            if (forDeleted)
            {
                newEntity.IsDeleted = true;
            }

            //ObjDictionary objDict = newEntity.GetAllValues();
            ////newEntity.EnsureInitialization();

            return newEntity;
        }


        public void Clear()
        {
            _templateLock.EnterWriteLock();
            try
            {
                this.Templates.Clear();
            }
            finally
            {
                _templateLock.ExitWriteLock();
            }
        }
    }
}
