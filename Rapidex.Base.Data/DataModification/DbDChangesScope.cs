using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading;
using System.Xml;
using static Rapidex.Data.DbEntityFactory;
using static Rapidex.Data.Reference;

namespace Rapidex.Data.DataModification
{
    internal class DbDChangesScope : IDbChangesScope
    {
        internal class EntityDependency
        {
            public IEntity From { get; set; }
            public IEntity To { get; set; }

            public string FromPropertyName { get; set; }

            public EntityDependency(IEntity from, IEntity to, string fromPropertyName)
            {
                From = from;
                To = to;
                FromPropertyName = fromPropertyName;
            }
        }

        internal class EntityDependencyCollection
        {
            public List<EntityDependency> Dependencies { get; } = new List<EntityDependency>();
            public Dictionary<IEntity, int> Score { get; } = new Dictionary<IEntity, int>();

            public ThreeLevelList<IEntity, IEntity, EntityDependency> CyclicControlList = new ThreeLevelList<IEntity, IEntity, EntityDependency>();

            public void Add(EntityDependency dep)
            {
                var availableDeps = this.CyclicControlList.Get(dep.From)?.Get(dep.To);
                if (availableDeps != null && availableDeps.Any(dd => dd.FromPropertyName == dep.FromPropertyName))
                {
                    return;
                }

                this.Dependencies.Add(dep);
                if (!Score.ContainsKey(dep.To))
                    Score.Add(dep.To, 0);

                this.Score[dep.To]++;

                this.CyclicControlList.Set(dep.From, dep.To, dep);
            }

            public void Add(IEntity from, IEntity to, string fromPropertyName)
            {
                this.Add(new EntityDependency(from, to, fromPropertyName));
            }

            public EntityDependency[] FindDependedEntities(IEntity entityTo)
            {
                return this.Dependencies.Where(d => d.To == entityTo).ToArray();
            }

            public void UpdateDependedEntities(IEntity entityTo)
            {
                EntityDependency[] deps = this.FindDependedEntities(entityTo);

                foreach (EntityDependency dep in deps)
                {
                    dep.From.SetValue(dep.FromPropertyName, entityTo.GetId());
                }
            }

        }


        HashSet<IEntity> _deletedEntities = new HashSet<IEntity>();

        HashSet<IEntity> _changedEntities = new HashSet<IEntity>();

        HashSet<IEntity> _newEntities = new HashSet<IEntity>();
        TwoLevelDictionary<IDbEntityMetadata, long, long> _prematureIds = new();

        List<IQueryUpdater> _bulkUpdates = new List<IQueryUpdater>();

        EntityDependencyCollection _newEntityDependencies = new EntityDependencyCollection();

        ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public IReadOnlyCollection<IEntity> ChangedEntities => _changedEntities.ToArray();

        public IReadOnlyCollection<IEntity> DeletedEntities => _deletedEntities.ToArray();

        public IReadOnlyCollection<IQueryUpdater> BulkUpdates => _bulkUpdates.ToArray();

        public DbDChangesScope() { }

        public DbDChangesScope(IEnumerable<IEntity> changedEntities, IEnumerable<IEntity> deletedEntities, IEnumerable<IQueryUpdater> bulkUpdates)
        {
            if (changedEntities.IsNOTNullOrEmpty())
                this.Add(changedEntities);

            if (deletedEntities.IsNOTNullOrEmpty())
                this.Delete(deletedEntities);

            if (bulkUpdates.IsNOTNullOrEmpty())
                this._bulkUpdates.AddRange(bulkUpdates);
        }

        protected IEntity FindNewEntity(IDbEntityMetadata em, long id)
        {
            if (id.IsEmptyId())
                return null;

            try
            {
                foreach (var entity in _newEntities)
                {
                    //TODO: daha verimli bir yöntem
                    //WARN: Referanslar eşit değil, çünkü birisi prematüre vs. olabilir
                    //TODO: IEM için IComparable ya da equal override ...
                    if (entity.GetMetadata().Name == em.Name && id == (long)entity.GetId())
                        return entity;
                }

                throw new InvalidOperationException($"Entity {em.Name}/{id} is not found in new entitites");
            }
            finally
            {
            }
        }

        protected void CheckDependencies(IEntity entity)
        {
            //Reference olanlar
            IDbEntityMetadata em = entity.GetMetadata();
            bool isPartial = entity is IPartialEntity;

            foreach (var fm in em.Fields.Values)
            {
                if (fm.Type.IsSupportTo<IReference>())
                {
                    if (isPartial && entity[fm.Name] == null)
                        continue;

                    //Bir cached type list tutsak? Her defasında tümünü dolanmasak? 

                    ReferenceDbFieldMetadata refFm = (ReferenceDbFieldMetadata)fm;
                    IReference _ref = (IReference)fm.ValueGetterUpper(entity, fm.Name);
                    long targetId = (long)_ref.TargetId;
                    if (targetId.IsPrematureId())
                    {
                        //yani hedeflediğimiz henüz veritabanına kaydedilmemiş, bu durumda referansını izlemeli ve id'leri güncellemeliyiz

                        var refEm = refFm.ReferencedEntityMetadata;
                        //Bu bizim scope içerisinde olmalı..
                        IEntity targetEntity = this.FindNewEntity(refEm, targetId);

                        _newEntityDependencies.Add(entity, targetEntity, fm.Name);
                    }
                }
            }

        }

        protected void InternalAdd(IEntity entity)
        {
            if (entity._IsNew)
            {
                _newEntities.Add(entity);
            }

            this.CheckDependencies(entity);
            _changedEntities.Add(entity);
        }

        public void Add(IEntity entity)
        {
            _locker.EnterWriteLock();
            try
            {
                this.InternalAdd(entity);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void Add(IEnumerable<IEntity> entities)
        {
            _locker.EnterWriteLock();
            try
            {
                foreach (var entity in entities)
                {
                    this.InternalAdd(entity);
                }
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        protected void InternalAdd(IQueryUpdater updater)
        {
            if (this._bulkUpdates.Contains(updater))
                throw new InvalidOperationException("Updater already added");

            this._bulkUpdates.Add(updater);
        }

        public void Add(IQueryUpdater updater)
        {
            _locker.EnterWriteLock();
            try
            {
                this.InternalAdd(updater);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        protected void InternalDelete(IEntity entity)
        {
            this._deletedEntities.Add(entity);
        }


        public void Delete(IEntity entity)
        {
            _locker.EnterWriteLock();
            try
            {
                this.InternalDelete(entity);
            }
            finally
            {
                _locker.ExitWriteLock();
            }


        }

        public void Delete(IEnumerable<IEntity> entities)
        {
            _locker.EnterWriteLock();
            try
            {
                foreach (var entity in entities)
                {
                    this.InternalDelete(entity);
                }
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }


        protected void CheckNewEntity(IEntity entity)
        {
            if (entity._IsNew)
            {
                var em = entity.GetMetadata();
                TemplateInfo info = Database.EntityFactory.GetTemplate(em, entity._Scope);

                long oldId = (long)entity.GetId();
                if (entity.HasPrematureId())
                {
                    long newId = entity._Scope.Data.Sequence(info.PersistentSequence).GetNext();
                    entity.SetId(newId);

                    this._newEntityDependencies.UpdateDependedEntities(entity);

                    _prematureIds.Set(em, oldId, newId);
                }
            }
        }

        public void CheckNewEntities()
        {
            _locker.EnterWriteLock();
            try
            {
                foreach (var entity in _newEntities)
                {
                    this.CheckNewEntity(entity);
                }

                foreach (var entity in _changedEntities)
                {
                    if (entity.HasPrematureId() && !entity._IsNew)
                    {
                        long entityId = entity.GetId().As<long>();
                        var em = entity.GetMetadata();
                        //Eğer değişiklik scope içerisinde ise, yeni Id'yi atayalım
                        long newId = this._prematureIds.Get(em)
                            .NotNull()
                            .Get(entityId);

                        if (newId < 1)
                            throw new InvalidOperationException($"Entity {em.Name} with Id {entityId} is not found in new entities or has not been assigned a valid Id yet.");

                        entity.SetId(newId);
                    }
                }
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }


        public IDbChangesScope[] SplitForTypesAndDependencies()
        {
            _locker.EnterReadLock();
            try
            {
                List<IDbChangesScope> list = new List<IDbChangesScope>();

                list.AddRange(this.ChangedEntities.GroupBy(e => e._TypeName).Select(g => new DbDChangesScope(g, null, null)).ToArray());
                list.AddRange(this.DeletedEntities.GroupBy(e => e._TypeName).Select(g => new DbDChangesScope(null, g, null)).ToArray());
                list.AddRange(this.BulkUpdates.GroupBy(e => e.EntityMetadata.Name).Select(g => new DbDChangesScope(null, null, g)).ToArray());

                return list.ToArray();
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        public void Clear()
        {
            _locker.EnterWriteLock();
            try
            {
                this._changedEntities.Clear();
                this._deletedEntities.Clear();
                this._newEntities.Clear();
                this._prematureIds.Clear();
                this._newEntityDependencies = new EntityDependencyCollection();
            }
            finally
            {
                _locker.ExitWriteLock();
            }

        }


    }
}
