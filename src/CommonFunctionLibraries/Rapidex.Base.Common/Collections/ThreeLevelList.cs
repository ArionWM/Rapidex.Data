using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex;

public class ThreeLevelList<TKey1, TKey2, TValue> : Dictionary<TKey1, TwoLevelList<TKey2, TValue>>
{
    ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
    public void Set(TKey1 key1, TKey2 key2, TValue value)
    {
        _locker.EnterUpgradeableReadLock();
        try
        {
            if (!this.ContainsKey(key1))
            {
                try
                {
                    _locker.EnterWriteLock();
                    this.Add(key1, new TwoLevelList<TKey2, TValue>());
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }

            TwoLevelList<TKey2, TValue> list = this[key1];
            list.Set(key2, value);
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
    }

    public void Set(TKey1 key1, TKey2 key2, IEnumerable<TValue> values)
    {
        _locker.EnterUpgradeableReadLock();
        try
        {
            if (!this.ContainsKey(key1))
            {
                try
                {
                    _locker.EnterWriteLock();
                    this.Add(key1, new TwoLevelList<TKey2, TValue>());
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }

            TwoLevelList<TKey2, TValue> list = this[key1];
            list.Set(key2, values);
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
    }

    public TwoLevelList<TKey2, TValue> Get(TKey1 key)
    {
        _locker.EnterUpgradeableReadLock();
        try
        {
            if (this.ContainsKey(key))
                return this[key];

            try
            {
                _locker.EnterWriteLock();
                var list = new TwoLevelList<TKey2, TValue>();
                this.Add(key, list);
                return list;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }
        finally
        {
            _locker.ExitUpgradeableReadLock();
        }
    }
}
