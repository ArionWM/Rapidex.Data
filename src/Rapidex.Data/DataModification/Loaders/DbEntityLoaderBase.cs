using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data;

namespace Rapidex.Data.DataModification.Loaders;

public abstract class DbEntityLoaderBase : IDbEntityLoader
{
    protected IDbEntityLoader[] SecondaryLoaders { get; set; }
    public IDbSchemaScope ParentScope { get; private set; }

    public DbEntityLoaderBase()
    {
    }


    protected abstract IEntity GetInternal(IDbEntityMetadata em, DbEntityId id);



    public void Setup(IDbSchemaScope schema, params IDbEntityLoader[] loaders)
    {
        this.ParentScope = schema;
        this.SecondaryLoaders = loaders;
    }

    protected virtual IEntity[] LoadInternal(IDbEntityMetadata em, IEnumerable<DbEntityId> ids, IDbEntityLoader[] secondaryLoaders)
    {
        List<IEntity> available = new List<IEntity>();
        List<DbEntityId> notAvailable = new List<DbEntityId>();

        foreach (var id in ids)
        {
            var entity = this.GetInternal(em, id);
            if (entity == null)
                notAvailable.Add(id);
            else
                available.Add(entity);
        }

        foreach (var loader in secondaryLoaders)
        {
            var loadResult = loader.Load(em, notAvailable); //TODO: Multiple load with multiple ids
            if (loadResult.Any())
            {
                available.AddRange(loadResult);

                var availableIds = loadResult.ToDbEntityIds();

                //TODO: Load result ile sadece Id değil, version karşılaştırması da yapılmalı
                notAvailable = notAvailable.Except(availableIds, new DbEntityIdEqualityComparerById()).ToList();
            }

            if (!notAvailable.Any())
                break;
        }

        if (notAvailable.Any())
        {
            throw new InvalidOperationException($"Not all entities are loaded ({em.Name})");
        }

        return available.ToArray();
    }


    public virtual IEntityLoadResult Load(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        IEntity[] loaded = this.LoadInternal(em, ids, this.SecondaryLoaders);
        EntityLoadResult values = new EntityLoadResult(loaded);
        return values;
    }

    public abstract ILoadResult<DataRow> LoadRawInternal(IQueryLoader loader);

    public virtual ILoadResult<DataRow> LoadRaw(IQueryLoader loader)
    {
        return this.LoadRawInternal(loader);
    }

    public abstract IEntityLoadResult LoadInternal(IQueryLoader loader);

    public virtual IEntityLoadResult Load(IQueryLoader loader)
    {
        return this.LoadInternal(loader);
    }



}
