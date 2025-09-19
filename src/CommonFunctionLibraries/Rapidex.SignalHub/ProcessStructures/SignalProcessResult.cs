using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;
public class SignalProcessResult : ValidationResult, ISignalProcessResult
{
    public SignalProcessStatus Status { get; set; }

    public bool IsAsync { get; set; }

    public ISignalArguments Arguments { get; set; }
    public object Data { get; set; }

    public SignalProcessResult(SignalProcessStatus status, ISignalArguments args, bool? isAsync = false, object? data = null, IValidationResult vr = null) : base(vr)
    {
        this.Status = status;
        this.Arguments = args;
        this.Data = data;
        if (isAsync.HasValue)
            this.IsAsync = isAsync.Value;
    }

}
