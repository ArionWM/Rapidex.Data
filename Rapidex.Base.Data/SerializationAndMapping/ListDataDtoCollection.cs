using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public class ListDataDtoCollection<T> : IReadOnlyCollection<T> where T : EntityDataDtoBase
{
    private readonly IReadOnlyCollection<T> _data;
    public Dictionary<string, object> Properties { get; set; }
    public int Count => _data.Count;

    public ListDataDtoCollection(IReadOnlyCollection<T> data)
    {
        _data = data.NotNull();
    }



    public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _data.GetEnumerator();
}
