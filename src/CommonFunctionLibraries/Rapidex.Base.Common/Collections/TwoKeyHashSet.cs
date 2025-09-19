using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex
{
    public class TwoKeyHashSet<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
    {
        ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        public void Set(TKey key, TValue value)
        {
            _locker.EnterUpgradeableReadLock();
            try
            {
                if (!this.ContainsKey(key))
                {
                    try
                    {
                        _locker.EnterWriteLock();
                        this.Add(key, new HashSet<TValue>());
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }
                }

                HashSet<TValue> list = this[key];
                list.Add(value);
            }
            finally
            {
                _locker.ExitUpgradeableReadLock();
            }
        }

        public void Set(TKey key, IEnumerable<TValue> values)
        {
            _locker.EnterUpgradeableReadLock();
            try
            {
                if (!this.ContainsKey(key))
                {
                    try
                    {
                        _locker.EnterWriteLock();
                        this.Add(key, new HashSet<TValue>());
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }
                }

                HashSet<TValue> list = this[key];
                list.Add(values);
            }
            finally
            {
                _locker.ExitUpgradeableReadLock();
            }
        }

        public void Set(TwoKeyHashSet<TKey, TValue> anotherSet)
        {
            foreach (TKey key in anotherSet.Keys)
                this.Set(key, anotherSet.Get(key));
        }

        public HashSet<TValue> Get(TKey key)
        {
            _locker.EnterUpgradeableReadLock();
            try
            {
                if (this.ContainsKey(key))
                    return this[key];

                try
                {
                    _locker.EnterWriteLock();
                    HashSet<TValue> list = new HashSet<TValue>();
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
}
