
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


        protected DbEntity Create(IDbSchemaScope scope, IDbEntityMetadata entityMetadata)
        {
            DbEntity dbEntity = new DbEntity();
            dbEntity._Scope = scope;
            dbEntity._TypeName = entityMetadata.Name;
            dbEntity._DbName = scope.ParentDbScope.Name;
            dbEntity._SchemaName = scope.SchemaName;
            dbEntity.Id = DatabaseConstants.DEFAULT_EMPTY_ID;
            dbEntity.ExternalId = null;
            dbEntity.DbVersion = 0;
            return dbEntity;
        }

        protected PartialEntity CreatePartial(IDbSchemaScope scope, IDbEntityMetadata entityMetadata)
        {
            PartialEntity dbEntity = new PartialEntity();
            dbEntity._Scope = scope;
            dbEntity._TypeName = entityMetadata.Name;
            dbEntity._DbName = scope.ParentDbScope.Name;
            dbEntity._SchemaName = scope.SchemaName;
            dbEntity.Id = DatabaseConstants.DEFAULT_EMPTY_ID;
            dbEntity.ExternalId = null;
            dbEntity.DbVersion = 0;
            return dbEntity;
        }

        protected TemplateInfo CreateTemplate(IDbSchemaScope scope, IDbEntityMetadata entityMetadata)
        {
            DbEntity dbEntity = this.Create(scope, entityMetadata);

            TemplateInfo templateInfo = new TemplateInfo();
            templateInfo.Entity = dbEntity;

            templateInfo.PersistentSequence = "id_" + entityMetadata.Name;
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PersistentSequence, -1, 10000);

            templateInfo.PrematureSequence = templateInfo.PersistentSequence + "_prm";
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PrematureSequence, -1, 10000);

            return templateInfo;
        }

        protected IEntity CreateConcrete(IDbSchemaScope scope, IDbEntityMetadata entityMetadata)
        {
            Type concreteType = Common.Assembly.FindType(entityMetadata.ConcreteTypeName);
            IEntity dbEntity = TypeHelper.CreateInstance<IEntity>(concreteType);

            dbEntity._Scope = scope;
            dbEntity._TypeName = entityMetadata.Name;
            dbEntity._DbName = scope.ParentDbScope.Name;
            dbEntity._SchemaName = scope.SchemaName;
            dbEntity.SetId(0L);
            dbEntity.ExternalId = null;
            dbEntity.DbVersion = 0;
            return dbEntity;
        }

        protected TemplateInfo CreateConcreteTemplate(IDbSchemaScope scope, IDbEntityMetadata entityMetadata)
        {
            IEntity dbEntity = this.CreateConcrete(scope, entityMetadata);

            TemplateInfo templateInfo = new TemplateInfo();
            templateInfo.Entity = dbEntity;

            templateInfo.PersistentSequence = "id_" + entityMetadata.Name;
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PersistentSequence, -1, 10000);

            templateInfo.PrematureSequence = templateInfo.PersistentSequence + "_prm";
            scope.Structure.CreateSequenceIfNotExists(templateInfo.PrematureSequence, -1, 10000);

            return templateInfo;
        }

        public TemplateInfo GetTemplate(IDbSchemaScope dbScope, IDbEntityMetadata em)
        {
            string key = dbScope.ParentDbScope.Name + "." + dbScope.SchemaName + "." + em.Name;

            _templateLock.EnterUpgradeableReadLock();
            try
            {
                if (this.Templates.ContainsKey(key) == false)
                {
                    _templateLock.EnterWriteLock();
                    try
                    {
                        if (em.ConcreteTypeName.IsNullOrEmpty())
                            this.Templates.Add(key, this.CreateTemplate(dbScope, em));
                        else
                            this.Templates.Add(key, this.CreateConcreteTemplate(dbScope, em));
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

        public IEntity Create(IDbSchemaScope dbScope, IDbEntityMetadata em, bool forNew)
        {
            em.NotNull();

            IEntity newEntity = null;

            if (em.ConcreteTypeName.IsNullOrEmpty())
            {
                newEntity = this.Create(dbScope, em);
            }
            else
            {
                newEntity = this.CreateConcrete(dbScope, em);
            }

            ((IEntity)newEntity)._Metadata = em;

            if (forNew)
            {
                TemplateInfo templateInfo = GetTemplate(dbScope, em);

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
            IDbEntityMetadata em = Database.Metadata.Get<T>();
            return this.Create(dbScope, em, forNew);
        }


        //TODO: Test, pEntity.Values boş olmalı 
        public IPartialEntity CreatePartial(IDbSchemaScope dbScope, IDbEntityMetadata em, bool forNew, bool forDeleted)
        {
            em.NotNull();

            IPartialEntity newEntity = null;

            newEntity = this.CreatePartial(dbScope, em);

            ((IPartialEntity)newEntity)._Metadata = em;

            if (forNew)
            {
                TemplateInfo templateInfo = GetTemplate(dbScope, em);

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
