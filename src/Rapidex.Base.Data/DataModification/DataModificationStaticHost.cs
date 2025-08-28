using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationStaticHost : DataModificationScopeBase, IDbDataModificationStaticHost
{
    protected ThreadLocal<IDbChangesCollection> dbChangesCollection;
    

    public DataModificationStaticHost(IDbSchemaScope parentScope, IDbDataModificationPovider dmProvider) : base(parentScope, dmProvider)
    {
        this.dbChangesCollection = new ThreadLocal<IDbChangesCollection>();
    }

    public IDbDataModificationTransactionedScope BeginWork()
    {
        return new DataModificationScope(this.ParentScope, this.DmProvider);
    }

    protected override IDbChangesCollection GetChangesCollection()
    {
        if (this.dbChangesCollection.Value == null)
            this.dbChangesCollection.Value = new DbChangesCollection();

        return this.dbChangesCollection.Value;
    }



    protected override void ApplyFinalized()
    {


    }

    public IIntSequence Sequence(string name)
    {
        return this.DmProvider.Sequence(name);
    }

    public IEntityUpdateResult ApplyChanges()
    {
        try
        {
            return this.CommitOrApplyChangesInternal();
        }
        catch(Exception ex)
        {
            ex.Log();
            throw;
        }
        finally
        {
            this.dbChangesCollection.Value = null;
        }
    }
}
