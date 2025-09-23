using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Transform;
public class EntityDataDtoCollection<T> : IReadOnlyCollection<T> where T : EntityDataDto
{
    private readonly IReadOnlyCollection<T> data;
    public Dictionary<string, object> Properties { get; set; }
    public int Count => data.Count;

    public EntityDataDtoCollection(IReadOnlyCollection<T> data)
    {
        this.data = data.NotNull();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this.data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)this.data).GetEnumerator();
    }
}
