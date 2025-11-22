using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex;

public class LoadResult<T> : ILoadResult<T>
{
    // ✅ Public property olarak değiştir - JSON serialization için kritik
    [JsonPropertyName("items")]
    [JsonPropertyOrder(1)]
    public List<T> Items { get; set; } = new List<T>();

    private long totalCount;

    public T this[int index]
    {
        get { return this.Items[index]; }
        set { this.Items[index] = value; }
    }

    [JsonPropertyOrder(2)]
    public long TotalItemCount
    {
        get
        {
            if (this.PageSize < 0 || this.PageSize == int.MaxValue)
            {
                return this.ItemCount;
            }
            return totalCount;
        }
        set { totalCount = value; }
    }

    [JsonPropertyOrder(3)]
    public long? StartIndex { get; set; }
    
    [JsonPropertyOrder(4)]
    public long? PageCount { get; set; }
    
    [JsonPropertyOrder(5)]
    public long? PageIndex { get; set; }
    
    [JsonPropertyOrder(6)]
    public long? PageSize { get; set; }

    // ✅ Computed property - serialize edilmemeli
    [JsonIgnore]
    public long ItemCount => Items.Count;

    // ✅ Public property - explicit interface'den kaldır
    [JsonPropertyOrder(7)]
    public bool IncludeTotalItemCount { get; set; }

    // ✅ Explicit interface implementation - JSON'da görünmez (doğru)
    [JsonIgnore]
    int IReadOnlyCollection<T>.Count => this.Items.Count;

    // ✅ Computed property - serialize edilmemeli
    [JsonIgnore]
    public bool IsEmpty => !this.Items.Any();

    public LoadResult()
    {

    }

    public LoadResult(IEnumerable<T> items)
    {
        this.Items.AddRange(items);
        this.TotalItemCount = this.Items.Count;
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
        return this.Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.Items.GetEnumerator();
    }
}
