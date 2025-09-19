using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rapidex;

//From Pro-Core
public class SortedListWithSameKeys<TItem>
{
    protected SortedDictionary<long, TItem> _dict = new SortedDictionary<long, TItem>();
    ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public IEnumerable<TItem> List { get { return _dict.Values; } }

    public int Count { get { return _dict.Count; } }

    public void Add(int index, TItem item)
    {
        _lock.EnterWriteLock();
        try
        {
            long _index = Convert.ToInt64(index) * 1000;
            while (_dict.ContainsKey(_index))
                _index++;
            _dict[_index] = item;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public TItem[] GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return this._dict.Values.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _dict.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
