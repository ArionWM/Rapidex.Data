using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex;

public class DefaultTimeProvider : ITimeProvider
{
    protected ConcurrentDictionary<int, System.Threading.Timer> timers = new();

    public virtual DateTimeOffset Now => DateTimeOffset.Now;
    public virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    protected int CreateHandle()
    {
        int handle = Guid.NewGuid().GetHashCode();
        return handle;
    }

    protected int CallAfterInternal(int msLater, Func<DateTimeOffset, Task> callback)
    {
        callback.NotNull();

        int handle = this.CreateHandle();

        var stateTimer = new System.Threading.Timer(
            (state) =>
            {
                callback.Invoke(DateTimeOffset.Now).Wait();
            },
            null,
            msLater,
            msLater
        );

        timers.TryAdd(handle, stateTimer);
        return handle;
    }

    public int CallAfter(int msLater, Func<DateTimeOffset, Task> callback)
    {
        return this.CallAfterInternal(msLater, callback);
    }

    public virtual int CallAfter(int msLater, Action<DateTimeOffset> callback)
    {
        callback.NotNull();

        return this.CallAfterInternal(msLater, (time) =>
        {
            callback.Invoke(time);
            return Task.CompletedTask;
        });
    }

    public void Setup()
    {

    }

    public virtual void CancelCall(int handle)
    {
        if (timers.TryRemove(handle, out Timer timer))
        {
            timer.Dispose();
        }
    }

    public virtual void CancelAllCalls()
    {
        var _timers = timers.ToArray();
        foreach (var timerKv in _timers)
        {
            timers.TryRemove(timerKv.Key, out Timer _);
            timerKv.Value.Dispose();
        }
    }


}
