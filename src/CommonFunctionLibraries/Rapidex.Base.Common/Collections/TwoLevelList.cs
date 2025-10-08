using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace Rapidex;

//From ProCore
public class TwoLevelList<TKey, TValue> : Dictionary<TKey, List<TValue>>
    where TKey : notnull
{


    ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
    public List<TValue> Set([NotNull] TKey key, TValue value)
    {
        key.NotNull();

        locker.EnterUpgradeableReadLock();
        try
        {
            if (!this.ContainsKey(key))
            {
                try
                {
                    locker.EnterWriteLock();
                    this.Add(key, new List<TValue>());
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            List<TValue> list = this[key];
            list.Add(value);
            return list;
        }
        finally
        {
            locker.ExitUpgradeableReadLock();
        }
    }

    public void Set([NotNull] TKey key, IEnumerable<TValue> values)
    {
        key.NotNull();

        locker.EnterUpgradeableReadLock();
        try
        {
            if (!this.ContainsKey(key))
            {
                try
                {
                    locker.EnterWriteLock();
                    this.Add(key, new List<TValue>());
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            List<TValue> list = this[key];
            list.AddRange(values);
        }
        finally
        {
            locker.ExitUpgradeableReadLock();
        }
    }

    public List<TValue> Get([NotNull] TKey key)
    {
        key.NotNull();

        locker.EnterUpgradeableReadLock();
        try
        {
            if (this.ContainsKey(key))
                return this[key];

            try
            {
                locker.EnterWriteLock();
                List<TValue> list = new List<TValue>();
                this.Add(key, list);
                return list;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
        finally
        {
            locker.ExitUpgradeableReadLock();
        }
    }

    public void Add(TKey key, TValue value)
    {
        this.Set(key, value);
    }
}
