using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationStaticScope : DataModificationManagerBase, IDbDataModificationStaticManager
{
    protected ThreadLocal<IDbChangesCollection> dbChangesScope;
    

    public DataModificationStaticScope(IDbSchemaScope parentScope, IDbDataModificationPovider dmProvider) : base(parentScope, dmProvider)
    {
        this.dbChangesScope = new ThreadLocal<IDbChangesCollection>();
    }

    public IDbDataModificationTransactionedManager Begin()
    {
        return new DataModificationScope(this.ParentScope, this.DmProvider);
    }

    protected override IDbChangesCollection GetChangesCollection()
    {
        if (this.dbChangesScope.Value == null)
            this.dbChangesScope.Value = new DbChangesCollection();

        return this.dbChangesScope.Value;
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
            this.dbChangesScope.Value = null;
        }
    }
}
