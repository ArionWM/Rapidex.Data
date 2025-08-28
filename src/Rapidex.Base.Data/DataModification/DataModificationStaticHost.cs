using Rapidex.Theading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationStaticHost : DataModificationScopeBase, IDbDataModificationStaticHost
{
    AsyncLocalStack<IDbDataModificationScope> localStack = new AsyncLocalStack<IDbDataModificationScope>();
    public IDbDataModificationScope CurrentWork
    {
        get
        {
            return localStack.Peek();
        }
    }


    public DataModificationStaticHost(IDbSchemaScope parentScope) : base(parentScope)
    {

    }

    public IDbDataModificationScope BeginWork()
    {

        //var dmProvider = this.dbProvider.GetDataModificationPovider();
        IDbDataModificationScope scope = new DataModificationScope(this);
        this.Register(scope);
        return scope;
    }


    public IIntSequence Sequence(string name)
    {
        return this.DmProvider.Sequence(name);
    }

    protected void Register(IDbDataModificationScope scope)
    {
        localStack.Push(scope);
    }

    public void UnRegister(IDbDataModificationScope scope)
    {
        var top = localStack.Peek();
        if (top != scope)
            throw new InvalidOperationException("The scope is not top on the stack. Close scopes with reverse order");

        localStack.Pop();
    }
}
