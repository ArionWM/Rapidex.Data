using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;



public interface IExceptionTranslator
{
    Exception Translate(Exception ex, string? additionalInfo = null);

}

public interface IExceptionManager
{
    Exception Translate(Exception ex);
}




public delegate void TimerEventDelegate(string name, DateTimeOffset time, object state);

public interface ITimeProvider
{
    DateTimeOffset Now { get; }

    event TimerEventDelegate OnEvent;

    void Setup();
    void CallAfter(int msLater, Action<DateTimeOffset> callback);
    void StopCall();
}
