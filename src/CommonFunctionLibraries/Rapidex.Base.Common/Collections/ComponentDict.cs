using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex
{
    //TODO: Dictionary -> ConcurrentDictionary
    public class ComponentDictionary<TItem> : DictionaryA<TItem>, IList<TItem>, IList, IEnumerable<TItem>
        where TItem : IComponent
    {
        //ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public TItem this[int index]
        {
            get => this.List[index];
            set => this.List[index] = value;
        }
        object? IList.this[int index]
        {
            get => this.List[index];
            set => this.List[index] = (TItem)value;
        }

        public IList<TItem> List { get { return this.Values.ToList(); } }

        public bool IsReadOnly => false;

        bool IList.IsFixedSize => false;

        public void Add(TItem item)
        {
            item.NotNull();

            item.NavigationName.NotEmpty(string.Format("{0} item NavigationName is empty", item.GetType().Name));

            this.Set(item.NavigationName, item);
        }

        #region IList
        public bool Contains(TItem item)
        {
            return this.Values.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(TItem item)
        {
            return this.List.IndexOf(item);
        }

        public void Insert(int index, TItem item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(TItem item)
        {
            return this.Remove(item.NavigationName);
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object? value)
        {
            this.Add((TItem)value);
            return this.Count - 1;
        }

        bool IList.Contains(object? value)
        {
            return this.Contains((TItem)value);
        }

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
        {
            return this.List.GetEnumerator();
        }

        int IList.IndexOf(object? value)
        {
            return this.IndexOf((TItem)value);
        }

        void IList.Insert(int index, object? value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object? value)
        {
            this.Remove((TItem)value);
        }

        void IList.Clear()
        {
            this.Clear();
        }
        #endregion


        public R Get<R>() where R : TItem
        {
            return (R)this.Values.FirstOrDefault(f => f.IsSupportTo<R>());
        }

        //public R Get(Func<T, bool> func)
        //{
        //    return this._dict.Values.FirstOrDefault(func);
        //}
    }
}
