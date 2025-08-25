using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationStaticScope : DataModificationManagerBase, IDbDataModificationStaticScope
{
    protected ThreadLocal<IDbChangesCollection> dbChangesScope;

    public DataModificationStaticScope(IDbSchemaScope parentScope, IDbDataModificationPovider dmProvider) : base(parentScope, dmProvider)
    {
    }

    public IDbDataModificationScope Begin()
    {
        throw new NotImplementedException();
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

    public Task<IEntityUpdateResult> ApplyChanges()
    {
        return this.CommitOrApplyChangesInternal();
    }
}
