using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal abstract class DataModificationReadScopeBase : IDbDataReadScope, IDisposable
{
    public IDbSchemaScope ParentSchema { get; protected set; }
    protected virtual IDbDataModificationPovider DmProvider { get; set; }

    public DataModificationReadScopeBase(IDbSchemaScope parentScope)
    {
        this.ParentSchema = parentScope;
        this.DmProvider = parentScope.DbProvider.GetDataModificationProvider(); 

        this.Initialize();
    }



    protected virtual void Initialize()
    {

    }

    protected IDbEntityLoader SelectLoader(IDbEntityMetadata em)
    {
        //TODO: Select loader 
        //DbEntityInMemoryCacheLoader cacheLoader = new DbEntityInMemoryCacheLoader();
        //cacheLoader.Setup(this.DmProvider);

        return this.DmProvider;
    }


    public IQuery GetQuery(IDbEntityMetadata em)
    {
        em.NotNull("Metadata can't be null");
        return new Rapidex.Data.Query.DbQuery(this.ParentSchema, em);
    }

    public IQuery<T> GetQuery<T>() where T : IConcreteEntity
    {
        return new Rapidex.Data.Query.Query<T>(this.ParentSchema);
    }

    public IEntityLoadResult Load(IQueryLoader queryLoader)
    {
        var em = queryLoader.EntityMetadata;

        //TODO: count
        //TODO: SelectLoader'a query i verelim, critere bakarak;
        //1- Count ya da 2- Id list kullansın
        //3- count ya da id sayısı X'in üzerinde ise doğrudan yükle
        // - değil ise id listesi ile yükle (Cache)

        IDbEntityLoader entityLoader = this.SelectLoader(em);

        IEntityLoadResult loadedResult = entityLoader.Load(queryLoader);

        if (queryLoader.Paging.IsPagingSet())
        {
            loadedResult.StartIndex = queryLoader.Paging.StartIndex;
            loadedResult.PageSize = queryLoader.Paging.PageSize;
            loadedResult.PageIndex = loadedResult.StartIndex / queryLoader.Paging.PageSize;

            if (queryLoader.Paging.IsPagingSet() && queryLoader.Paging.IncludeTotalItemCount)
            {
                IQueryAggregate totalCounter = (IQueryAggregate)queryLoader.Clone();
                totalCounter.Alias = queryLoader.Alias;
                totalCounter.ClearPaging();
                //totalCounter.Page(int.MaxValue, 0);
                loadedResult.TotalItemCount = totalCounter.Count();
                loadedResult.IncludeTotalItemCount = true;
                loadedResult.PageCount = (long)Math.Ceiling(Convert.ToDecimal(loadedResult.TotalItemCount) / Convert.ToDecimal(loadedResult.PageSize.Value));

            }
        }

        return loadedResult;

    }

    public ILoadResult<DataRow> LoadRaw(IQueryLoader queryLoader)
    {
        return this.DmProvider.LoadRaw(queryLoader);
    }


    public IEntity Find(IDbEntityMetadata em, long id)
    {
        if (em.OnlyBaseSchema && this.ParentSchema.SchemaName != DatabaseConstants.DEFAULT_SCHEMA_NAME)
            return this.ParentSchema.ParentDbScope.Find(em, id);

        DbEntityId eid = new DbEntityId(id, -1);
        IDbEntityLoader loader = this.SelectLoader(em);
        IEntityLoadResult result = loader.Load(em, new DbEntityId[] { eid });
        return result.FirstOrDefault();
    }

    public virtual void Dispose()
    {
        this.DmProvider?.Dispose();
        this.DmProvider = null;
    }
}
