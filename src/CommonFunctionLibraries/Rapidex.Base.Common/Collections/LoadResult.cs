using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex
{
    public class LoadResult<T> : ILoadResult<T>
    {
        private List<T> _items = new List<T>();

        private long _totalCount;

        public T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public long TotalItemCount
        {
            get
            {
                if (this.PageSize < 0 || this.PageSize == int.MaxValue)
                {
                    return this.ItemCount;
                }
                return _totalCount;
            }
            set { _totalCount = value; }
        }




        public long? StartIndex { get; set; }
        public long? PageCount { get; set; }
        public long? PageIndex { get; set; }
        public long? PageSize { get; set; }

        public long ItemCount => _items.Count;

        bool IPaging.IncludeTotalItemCount { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        int IReadOnlyCollection<T>.Count => this._items.Count;

        public bool IsEmpty => !this._items.Any();

        public LoadResult()
        {

        }

        public LoadResult(IEnumerable<T> items)
        {
            _items.AddRange(items);
            this.TotalItemCount = _items.Count;
        }

        public LoadResult(IEnumerable<T> items, long pageSize, long pageIndex, long pageCount, long totalCount) : this(items)
        {
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.PageCount = pageCount;
            this.TotalItemCount = totalCount;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
