using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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


    protected abstract Task<IEntity> FindWithCacheInternal(IDbEntityMetadata em, DbEntityId id);

    public void Setup(IDbSchemaScope schema, IDbDataModificationPovider baseDmProvider)
    {
        this.ParentScope = schema;
        this.BaseDmProvider = baseDmProvider;
        this.SqlKataCompiler = this.BaseDmProvider.GetCompiler();
    }

    protected virtual async Task<IEntity[]> LoadWithCacheInternal(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        List<IEntity> available = new List<IEntity>();
        List<DbEntityId> notAvailable = new List<DbEntityId>();

        foreach (var id in ids)
        {
            var entity = await this.FindWithCacheInternal(em, id);
            if (entity == null)
                notAvailable.Add(id);
            else
                available.Add(entity);
        }

        if (notAvailable.Any())
        {
            var loadResult = await this.BaseDmProvider.Load(em, notAvailable); //TODO: Multiple load with multiple ids
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


    public virtual async Task<IEntityLoadResult> Load(IDbEntityMetadata em, IEnumerable<DbEntityId> ids)
    {
        IEntity[] loaded = await this.LoadWithCacheInternal(em, ids);
        EntityLoadResult values = new EntityLoadResult(loaded);
        return values;
    }

    public abstract Task<ILoadResult<DataRow>> LoadRawInternal(IQueryLoader loader);

    public virtual async Task<ILoadResult<DataRow>> LoadRaw(IQueryLoader loader)
    {
        return await this.LoadRawInternal(loader);
    }


    //TODO: Load(IDbEntityMetadata em, SqlResult result) metotlarını kaldır, Load(IQueryLoader loader) ile devam et
    //DbEntityWithCacheLoader içerisinde ayrıca SqlResult result = this.SqlKataCompiler.NotNull().Compile(loader.Query); kullan
    //IQueryLoader içerisinde "UseQueryCache" + "DontUseQueryCache"

    public abstract Task<IEntityLoadResult> Load(IDbEntityMetadata em, IQueryLoader loader, SqlResult compiledSql);

    public virtual async Task<IEntityLoadResult> Load(IQueryLoader loader)
    {
        SqlResult result = this.SqlKataCompiler.NotNull().Compile(loader.Query);
        return await this.Load(loader.EntityMetadata, loader, result);
    }



}
