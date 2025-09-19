using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Theading;
public class ThreadLocalStack<T> : IEmptyCheckObject
    where T : class
{
    protected ThreadLocal<Stack<T>> localStack;

    public bool IsEmpty => this.localStack.Value.Count == 0;
    public int Count => this.localStack.Value.Count;

    public ThreadLocalStack()
    {
        this.localStack = new ThreadLocal<Stack<T>>(() => new Stack<T>());
    }

    public void Push(T item)
    {
        item.NotNull("Item can't be null");
        this.localStack.Value.Push(item);
    }

    public T Pop()
    {
        if (this.Count == 0)
            return null;

        return this.localStack.Value.Pop();
    }
    public T Peek()
    {
        if (this.Count == 0)
            return null;

        return this.localStack.Value.Peek();
    }

    public void Clear()
    {
        this.localStack.Value.Clear();
    }
}
