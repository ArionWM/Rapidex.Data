using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex
{
    //From ProCore
    public class TwoLevelStringKeyList<TValue> : DictionaryA< List<TValue>>
    {
        ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        public List<TValue> Set(string key, TValue value)
        {
            _locker.EnterUpgradeableReadLock();
            try
            {
                if (!this.ContainsKey(key))
                {
                    try
                    {
                        _locker.EnterWriteLock();
                        this.Add(key, new List<TValue>());
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }
                }

                List<TValue> list = this[key];
                list.Add(value);
                return list;
            }
            finally
            {
                _locker.ExitUpgradeableReadLock();
            }
        }

        public void Set(string key, IEnumerable<TValue> values)
        {
            _locker.EnterUpgradeableReadLock();
            try
            {
                if (!this.ContainsKey(key))
                {
                    try
                    {
                        _locker.EnterWriteLock();
                        this.Add(key, new List<TValue>());
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }
                }

                List<TValue> list = this[key];
                list.AddRange(values);
            }
            finally
            {
                _locker.ExitUpgradeableReadLock();
            }
        }

        public List<TValue> Get(string key)
        {
            _locker.EnterUpgradeableReadLock();
            try
            {
                if (this.ContainsKey(key))
                    return this[key];

                try
                {
                    _locker.EnterWriteLock();
                    List<TValue> list = new List<TValue>();
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
