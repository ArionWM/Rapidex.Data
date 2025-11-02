using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex;

public class DefaultTimeProvider : ITimeProvider
{
    protected ConcurrentDictionary<string, System.Threading.Timer> timers = new ConcurrentDictionary<string, Timer>(StringComparer.InvariantCultureIgnoreCase);

    public virtual DateTimeOffset Now => DateTimeOffset.Now;
    public virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public virtual void CallAfter(int msLater, Action<DateTimeOffset> callback)
    {
        callback.NotNull();

        string uniqueKey = Guid.NewGuid().ToString();

        var stateTimer = new System.Threading.Timer(
            (state) =>
            {
                callback.Invoke(this.Now);
            },
            null,
            msLater,
            msLater
            );

        timers.TryAdd(uniqueKey, stateTimer);
    }

    public void Setup()
    {
        
    }

    public virtual void StopCall()
    {
        var _timers = timers.ToArray();
        foreach (var timerKv in _timers)
        {
            timers.TryRemove(timerKv.Key, out Timer _);
            timerKv.Value.Dispose();
        }
    }
}
