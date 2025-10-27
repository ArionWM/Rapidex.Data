using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading;
using System.Xml;
using static Rapidex.Data.DbEntityFactory;
using static Rapidex.Data.Reference;

namespace Rapidex.Data.DataModification;

internal class DbChangesCollection : IDbChangesCollection
{
    internal class EntityDependency
    {
        public IEntity From { get; set; }
        public IEntity To { get; set; }

        public string FromPropertyName { get; set; }

        public EntityDependency(IEntity from, IEntity to, string fromPropertyName)
        {
            this.From = from;
            this.To = to;
            this.FromPropertyName = fromPropertyName;
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
            if (!this.Score.ContainsKey(dep.To))
                this.Score.Add(dep.To, 0);

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
            object toId = entityTo.GetId();
            EntityDependency[] deps = this.FindDependedEntities(entityTo);

            foreach (EntityDependency dep in deps)
            {
                dep.From.SetValue(dep.FromPropertyName, toId);
            }
        }

    }


    HashSet<IEntity> deletedEntities = new HashSet<IEntity>();

    HashSet<IEntity> changedEntities = new HashSet<IEntity>();

    HashSet<IEntity> newEntities = new HashSet<IEntity>();
    TwoLevelDictionary<IDbEntityMetadata, long, long> prematureIds = new();

    List<IQueryUpdater> bulkUpdates = new List<IQueryUpdater>();

    EntityDependencyCollection newEntityDependencies = new EntityDependencyCollection();

    ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

    public IReadOnlyCollection<IEntity> ChangedEntities => changedEntities.ToArray();

    public IReadOnlyCollection<IEntity> DeletedEntities => deletedEntities.ToArray();

    public IReadOnlyCollection<IQueryUpdater> BulkUpdates => bulkUpdates.ToArray();

    public bool IsEmpty => !changedEntities.Any() && !deletedEntities.Any() && !bulkUpdates.Any();

    public DbChangesCollection()
    {
    }

    public DbChangesCollection(IEnumerable<IEntity> changedEntities, IEnumerable<IEntity> deletedEntities, IEnumerable<IQueryUpdater> bulkUpdates)
    {
        if (changedEntities.IsNOTNullOrEmpty())
            this.Add(changedEntities);

        if (deletedEntities.IsNOTNullOrEmpty())
            this.Delete(deletedEntities);

        if (bulkUpdates.IsNOTNullOrEmpty())
            this.bulkUpdates.AddRange(bulkUpdates);
    }

    protected void ValidateEntity(IEntity entity)
    {
        entity.NotNull();

        if (entity._IsNew && entity._IsDeleted)
            throw new InvalidOperationException("Entity cannot be both new and deleted");

        if (entity._IsDeleted && changedEntities.Contains(entity))
            throw new InvalidOperationException("Entity cannot be both changed and deleted");
    }

    protected IEntity FindNewEntity(IDbEntityMetadata em, long id)
    {
        if (id.IsEmptyId())
            return null;

        try
        {
            foreach (var entity in newEntities)
            {
                //TODO: daha verimli bir yöntem
                //WARN: Referanslar eşit değil, çünkü birisi prematüre vs. olabilir
                //TODO: IEM için IComparable ya da equal override ...
                if (entity.GetMetadata().Name == em.Name && id == (long)entity.GetId())
                    return entity;
            }

            //TODO: Analyze ?
            throw new EntityNotFoundException(em.Name, id, $"Entity {em.Name}/{id} is not found in new entitites");
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

                    newEntityDependencies.Add(entity, targetEntity, fm.Name);
                }
            }
        }

    }

    protected void InternalAdd(IEntity entity)
    {
        if (entity._IsDeleted)
        {
            this.InternalDelete(entity);
            return;
        }

        if (entity._IsNew)
        {
            newEntities.Add(entity);
        }

        this.CheckDependencies(entity);
        changedEntities.Add(entity);
    }

    public void Add(IEntity entity)
    {
        this.ValidateEntity(entity);

        locker.EnterWriteLock();
        try
        {
            this.InternalAdd(entity);
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    public void Add(IEnumerable<IEntity> entities)
    {
        locker.EnterWriteLock();
        try
        {
            foreach (var entity in entities)
            {
                this.InternalAdd(entity);
            }
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    protected void InternalAdd(IQueryUpdater updater)
    {
        if (this.bulkUpdates.Contains(updater))
            throw new InvalidOperationException("Updater already added");

        this.bulkUpdates.Add(updater);
    }

    public void Add(IQueryUpdater updater)
    {
        locker.EnterWriteLock();
        try
        {
            this.InternalAdd(updater);
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    protected void InternalDelete(IEntity entity)
    {
        this.deletedEntities.Add(entity);
    }


    public void Delete(IEntity entity)
    {
        locker.EnterWriteLock();
        try
        {
            entity._IsDeleted = true;
            this.InternalDelete(entity);
        }
        finally
        {
            locker.ExitWriteLock();
        }


    }

    public void Delete(IEnumerable<IEntity> entities)
    {
        locker.EnterWriteLock();
        try
        {
            foreach (var entity in entities)
            {
                this.InternalDelete(entity);
            }
        }
        finally
        {
            locker.ExitWriteLock();
        }
    }

    protected void CheckNewEntity(IEntity entity)
    {
        if (entity._IsNew)
        {
            var em = entity.GetMetadata();
            TemplateInfo info = Database.EntityFactory.GetTemplate(em, entity._Schema);

            long oldId = (long)entity.GetId();
            if (entity.HasPrematureId())
            {
                List<long> ids = this.newEntityIds.Get(em);
                long newId = 0;
                if (ids.Any())
                {
                    newId = ids.First();
                    ids.RemoveAt(0);
                }
                else
                {
                    newId = entity._Schema.Data.Sequence(info.PersistentSequence).GetNext();
                }

                entity.SetId(newId);
                entity._VirtualId = oldId;

                this.newEntityDependencies.UpdateDependedEntities(entity);

                prematureIds.Set(em, oldId, newId);
            }
        }
    }

    TwoLevelList<IDbEntityMetadata, long> newEntityIds = new TwoLevelList<IDbEntityMetadata, long>();

    protected void ReserveIds()
    {
        this.newEntityIds.Clear();
        var groupForTypeName = this.newEntities.Where(e => e.HasPrematureId()).GroupBy(e => e._TypeName);
        foreach (var group in groupForTypeName)
        {
            var dbScope = group.First()._Schema;
            var em = group.First().GetMetadata();
            TemplateInfo info = Database.EntityFactory.GetTemplate(em, group.First()._Schema);
            int requiredIdCount = group.Count();
            var reqIds = dbScope.Data.Sequence(info.PersistentSequence).GetNextN(requiredIdCount);
            this.newEntityIds.Set(em, reqIds);
        }
    }

    public void CheckNewEntities()
    {
        locker.EnterWriteLock();
        try
        {
            this.ReserveIds();

            foreach (var entity in newEntities)
            {
                this.CheckNewEntity(entity);
            }

            foreach (var entity in changedEntities)
            {
                if (entity.HasPrematureId() && !entity._IsNew)
                {
                    //WARN: Is this possible?
                    long entityId = entity.GetId().As<long>();
                    var em = entity.GetMetadata();
                    //Eğer değişiklik scope içerisinde ise, yeni Id'yi atayalım
                    long newId = this.prematureIds.Get(em)
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
            locker.ExitWriteLock();
        }
    }


    public IDbChangesCollection[] SplitForTypesAndDependencies()
    {
        locker.EnterReadLock();
        try
        {
            List<IDbChangesCollection> list = new List<IDbChangesCollection>();

            list.AddRange(this.ChangedEntities.GroupBy(e => e._TypeName).Select(g => new DbChangesCollection(g, null, null)).ToArray());
            list.AddRange(this.DeletedEntities.GroupBy(e => e._TypeName).Select(g => new DbChangesCollection(null, g, null)).ToArray());
            list.AddRange(this.BulkUpdates.GroupBy(e => e.EntityMetadata.Name).Select(g => new DbChangesCollection(null, null, g)).ToArray());

            return list.ToArray();
        }
        finally
        {
            locker.ExitReadLock();
        }
    }

    public void Clear()
    {
        locker.EnterWriteLock();
        try
        {
            this.changedEntities.Clear();
            this.deletedEntities.Clear();
            this.newEntities.Clear();
            this.prematureIds.Clear();
            this.newEntityDependencies = new EntityDependencyCollection();
        }
        finally
        {
            locker.ExitWriteLock();
        }

    }

    public (bool Found, string? Desc) FindAndAnalyse(IDbEntityMetadata em, long id)
    {
        if (id.IsEmptyId())
            return (false, null);


        try
        {
            foreach (var entity in newEntities)
            {
                //TODO: daha verimli bir yöntem
                //WARN: Referanslar eşit değil, çünkü birisi prematüre vs. olabilir
                //TODO: IEM için IComparable ya da equal override ...
                if (entity.GetMetadata().Name == em.Name && id == (long)entity.GetId())
                    return (true, "in new entities");
            }

            foreach (var entity in changedEntities)
            {
                //TODO: daha verimli bir yöntem
                //WARN: Referanslar eşit değil, çünkü birisi prematüre vs. olabilir
                //TODO: IEM için IComparable ya da equal override ...
                if (entity.GetMetadata().Name == em.Name && id == (long)entity.GetId())
                    return (true, "in changed entities");
            }

            return (false, null);
        }
        finally
        {
        }

    }
}
