using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;
public class SignalHandlingResult : ValidationResult, ISignalHandlingResult
{
    public int HandlerId { get; set; }
    public object Data { get; set; }

    public SignalHandlingResult(int handlerId, object? data = null)
    {
        this.HandlerId = handlerId;
        this.Data = data;
    }
}
