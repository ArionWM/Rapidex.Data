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
#if DEBUG
#pragma warning disable IDE1006 // Naming Styles
    private readonly int _debugTracker;
#pragma warning restore IDE1006 // Naming Styles
#endif

    private AsyncLocalStack<IDbDataModificationScope> localStack = new AsyncLocalStack<IDbDataModificationScope>();
    private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
    private ILogger logger;

    public IDbDataModificationScope CurrentWork
    {
        get
        {
            rwLock.EnterReadLock();
            try
            {
                IDbDataModificationScope current = localStack.Peek();
                current.NotNull<IDbDataModificationScope, WorkScopeNotAvailableException>("Active scope not available. Are you use 'BeginWork()'? See: abc"); //TODO: Create doc and add link
                return current;
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }
    }

    public DataModificationStaticHost(IDbSchemaScope parentScope, IServiceProvider serviceProvider) : base(parentScope)
    {
#if DEBUG
        this._debugTracker = RandomHelper.Random(1000000);
#endif
        this.logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<DataModificationStaticHost>();
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
        rwLock.EnterWriteLock();
        try
        {
            localStack.Push(scope);
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }

    public void UnRegister(IDbDataModificationScope scope)
    {
        rwLock.EnterWriteLock();
        try
        {
            var top = localStack.Peek();
            if (top != scope)
            {
                this.logger?.LogWarning("Scope collision detected. " +
                    "\r\nThis finalized scope is not top on the stack. " +
                    "\r\nAre you using 'Task.Run'?" +
                    "\r\nClose scopes with reverse order");
            }

            if (!localStack.TryRemove(scope))
            {
#if DEBUG
                AssertionHelper.DebugBreak();
#endif

                throw new InvalidOperationException("Scope removal failed. Scope not found in the stack.");

            }
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }

    (bool Found, string? Desc) IDbDataModificationStaticHost.FindAndAnalyseInScopes(IDbEntityMetadata em, long id)
    {
        var scopes = localStack.ToList();
        foreach (var scope in scopes)
        {
            var result = scope.FindAndAnalyse(em, id);
            if (result.Found)
                return result;
        }

        return (false, null);
    }
}
