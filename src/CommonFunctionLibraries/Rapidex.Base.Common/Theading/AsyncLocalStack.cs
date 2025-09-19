using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Theading;

public class AsyncLocalStack<T> : IEmptyCheckObject
    where T : class
{
    protected AsyncLocal<List<T>> localStack;

    public bool IsEmpty => this.localStack.Value == null || this.localStack.Value.Count == 0;
    public int Count => this.localStack.Value?.Count ?? 0;

    public AsyncLocalStack()
    {
        this.localStack = new AsyncLocal<List<T>>();
    }

    protected void CheckStack()
    {
        if (this.localStack.Value == null)
            this.localStack.Value = new List<T>();
    }

    public void Push(T item)
    {
        item.NotNull("Item can't be null");
        this.CheckStack();
        this.localStack.Value.Insert(0, item);
    }

    public T Pop()
    {
        this.CheckStack();

        if (this.Count == 0)
            return null;

        var item = this.localStack.Value[0];
        this.localStack.Value.RemoveAt(0);
        return item;
    }

    public T Peek()
    {
        this.CheckStack();

        if (this.Count == 0)
            return null;

        return this.localStack.Value.FirstOrDefault();
    }

    public void Clear()
    {
        this.localStack.Value?.Clear();
    }

    public bool Contains(T item)
    {
        this.CheckStack();
        return this.localStack.Value.Contains(item);
    }

    public IEnumerable<T> ToList()
    {
        this.CheckStack();
        return this.localStack.Value.ToList();
    }
}
