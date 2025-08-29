using Rapidex.Theading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data.DataModification;
internal class DataModificationStaticHost : DataModificationReadScopeBase, IDbDataModificationStaticHost
{
    private readonly int _debugTracker;
    private AsyncLocalStack<IDbDataModificationScope> _localStack = new AsyncLocalStack<IDbDataModificationScope>();
    private ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    public IDbDataModificationScope CurrentWork
    {
        get
        {
            _rwLock.EnterReadLock();
            try
            {
                IDbDataModificationScope current = _localStack.Peek();
                current.NotNull<IDbDataModificationScope, WorkScopeNotAvailableException>("Active scope not available. Are you use 'BeginWork()'? See: abc"); //TODO: Create doc and add link
                return current;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
    }

    public DataModificationStaticHost(IDbSchemaScope parentScope) : base(parentScope)
    {
        this._debugTracker = RandomHelper.Random(1000000);
    }

    public IDbDataModificationScope BeginWork()
    {
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
        _rwLock.EnterWriteLock();
        try
        {
            _localStack.Push(scope);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public void UnRegister(IDbDataModificationScope scope)
    {
        _rwLock.EnterWriteLock();
        try
        {
            var top = _localStack.Peek();
            if (top != scope)
                throw new InvalidOperationException("Scope collision detected. " +
                    "\r\nThis finalized scope is not top on the stack. " +
                    "\r\nAre you using 'Task.Run'?" +
                    "\r\nClose scopes with reverse order");

            _localStack.Pop();
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    (bool Found, string? Desc) IDbDataModificationStaticHost.FindAndAnalyseInScopes(IDbEntityMetadata em, long id)
    {
        var scopes = _localStack.ToList();
        foreach (var scope in scopes)
        {
            var result = scope.FindAndAnalyse(em, id);
            if (result.Found)
                return result;
        }

        return (false, null);
    }
}
