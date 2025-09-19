
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex
{
    //From ProCore
    public class TwoLevelDictionary<K1, K2, TValue> : Dictionary<K1, Dictionary<K2, TValue>>
    {
        ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        public TwoLevelDictionary()
            : base(typeof(K1).IsSupportTo<string>() ? (IEqualityComparer<K1>)StringComparer.InvariantCultureIgnoreCase : null)
        {

        }

        protected void SetInternal(K1 key1, K2 key2, TValue value)
        {
            try
            {
                if (!this.ContainsKey(key1))
                {
                    //StringComparer.InvariantCultureIgnoreCase

                    Dictionary<K2, TValue> subDict = null;

                    if (typeof(K2).IsSupportTo<string>())
                        subDict = new Dictionary<K2, TValue>((IEqualityComparer<K2>)StringComparer.InvariantCultureIgnoreCase);
                    else
                        subDict = new Dictionary<K2, TValue>();

                    this.Add(key1, subDict);
                }

                this[key1].Set(key2, value);
            }
            finally
            {

            }
        }

        public void Set(K1 key1, K2 key2, TValue value)
        {
            rwLock.EnterWriteLock();

            try
            {
                this.SetInternal(key1, key2, value);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        protected TValue GetInternal(K1 key1, K2 key2)
        {
            Dictionary<K2, TValue> dict = this.Get(key1);
            if (dict == null)
                return default(TValue);

            return dict.Get(key2);
        }

        public TValue Get(K1 key1, K2 key2)
        {
            rwLock.EnterReadLock();
            try
            {
                return this.GetInternal(key1, key2);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public TValue GetOr(K1 key1, K2 key2, Func<K1, K2, TValue> nofunc)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                TValue val = this.GetInternal(key1, key2);
                if (val == null)
                {
                    rwLock.EnterWriteLock();
                    try
                    {
                        val = nofunc(key1, key2);
                        this.SetInternal(key1, key2, val);
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
                return val;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        public TValue GetOr(K1 key1, K2 key2, Func<TValue> nofunc)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                TValue val = this.GetInternal(key1, key2);
                if (val == null)
                {
                    rwLock.EnterWriteLock();
                    try
                    {
                        val = nofunc();
                        this.SetInternal(key1, key2, val);
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();
                    }
                }
                return val;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }
    }
}
