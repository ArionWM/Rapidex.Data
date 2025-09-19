using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class SortedComponentList<T> : SortedListWithSameKeys<T> where T : IOrderedComponent
    {
        public void Add(T item)
        {
            base.Add(item.Index, item);
        }

        public T Get(string name)
        {
            return this._dict.Values.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0 || string.Compare(f.NavigationName, name, true) == 0);
        }

        public T Get<R>() where R : T
        {
            return this._dict.Values.FirstOrDefault(f => f.IsSupportTo<R>());
        }

        public T Get(Func<T, bool> func)
        {
            return this._dict.Values.FirstOrDefault(func);
        }

    }
}
