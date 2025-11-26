using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SqlKata;
using SqlKata.Compilers;

namespace Rapidex.Data.DataModification.Loaders;

public abstract class DbEntityLoaderBase : IDbEntityLoader
{
    protected IDbDataModificationPovider BaseDmProvider { get; set; }
    protected SqlKata.Compilers.Compiler SqlKataCompiler { get; set; }
    public IDbSchemaScope ParentScope { get; private set; }

    public DbEntityLoaderBase()
    {
    }


    protected abstract IEntity GetInternal(IDbEntityMetadata em, DbEntityId id);

    public void Setup(IDbSchemaScope schema, IDbDataModificationPovider baseDmProvider)
    {
        this.ParentScope = schema;
        this.BaseDmProvider = baseDmProvider;
        this.SqlKataCompiler = this.BaseDmProvider.GetCompiler();
    }

    protected virtual IEntity[] LoadInternal(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
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

        if (notAvailable.Any())
        {
            var loadResult = this.BaseDmProvider.Load(em, notAvailable); //TODO: Multiple load with multiple ids
            if (loadResult.Any())
            {
                available.AddRange(loadResult);

                var availableIds = loadResult.ToDbEntityIds();

                //TODO: Load result ile sadece Id değil, version karşılaştırması da yapılmalı
                notAvailable = notAvailable.Except(availableIds, new DbEntityIdEqualityComparerById()).ToList();
            }
        }

        return available.ToArray();
    }


    public virtual IEntityLoadResult Load(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        IEntity[] loaded = this.LoadInternal(em, ids);
        EntityLoadResult values = new EntityLoadResult(loaded);
        return values;
    }

    public abstract ILoadResult<DataRow> LoadRawInternal(IQueryLoader loader);

    public virtual ILoadResult<DataRow> LoadRaw(IQueryLoader loader)
    {
        return this.LoadRawInternal(loader);
    }


    //TODO: Load(IDbEntityMetadata em, SqlResult result) metotlarını kaldır, Load(IQueryLoader loader) ile devam et
    //DbEntityWithCacheLoader içerisinde ayrıca SqlResult result = this.SqlKataCompiler.NotNull().Compile(loader.Query); kullan
    //IQueryLoader içerisinde "UseQueryCache" + "DontUseQueryCache"

    public abstract IEntityLoadResult Load(IDbEntityMetadata em, SqlResult result);

    public virtual IEntityLoadResult Load(IQueryLoader loader)
    {
        SqlResult result = this.SqlKataCompiler.NotNull().Compile(loader.Query);
        return this.Load(loader.EntityMetadata, result);
    }



}
