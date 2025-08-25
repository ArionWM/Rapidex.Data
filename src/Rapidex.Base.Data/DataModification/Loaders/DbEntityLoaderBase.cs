using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data;

namespace Rapidex.Data.DataModification.Loaders
{
    public abstract class DbEntityLoaderBase : IDbEntityLoader
    {
        protected IDbEntityLoader[] SearchLoaders { get; set; }


        public DbEntityLoaderBase()
        {
        }


        protected abstract IEntity GetInternal(DbEntityId id);

        public abstract ILoadResult<DataRow> GetInternalRaw(IQueryLoader loader);

        public abstract IEntityLoadResult GetInternal(IQueryLoader loader);

        public void Setup(params IDbEntityLoader[] loaders)
        {
            this.SearchLoaders = loaders;
        }




        protected virtual IEntity[] LoadInternal(IDbEntityMetadata em, IEnumerable<DbEntityId> ids, IDbEntityLoader[] secondaryLoaders)
        {
            List<IEntity> available = new List<IEntity>();
            List<DbEntityId> notAvailable = new List<DbEntityId>();

            foreach (var id in ids)
            {
                var entity = GetInternal(id);
                if (entity == null)
                    notAvailable.Add(id);
                else
                    available.Add(entity);
            }

            foreach (var loader in secondaryLoaders)
            {
                var loadResult = loader.Load(em, notAvailable);
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
            IEntity[] loaded = LoadInternal(em, ids, this.SearchLoaders);
            EntityLoadResult values = new EntityLoadResult(loaded);
            return values;
        }

        public ILoadResult<DataRow> LoadRaw(IQueryLoader loader)
        {
            throw new NotImplementedException();
        }

        public IEntityLoadResult Load(IQueryLoader loader)
        {
            throw new NotImplementedException();
        }



    }
}
