using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rapidex;

//[JsonDerivedBase]
public class SignalArguments : ISignalArguments
{
    public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
    public Guid Id { get; set; }
    public SignalTopic Topic { get; set; }
    public int HandlerId { get; set; }
    public string SignalName { get; set; }
    public bool? IsSynchronous { get; set; }
    public string Data { get; set; }
    public string ContentType { get; set; }
    public string Tags { get; set; }

    public object Clone()
    {
        Type type = this.GetType();
        return this.Adapt(type, type);
    }

    public ISignalArguments CloneFor(int handlerId)
    {
        var clone = (SignalArguments)this.Clone();
        clone.HandlerId = handlerId;
        return clone;
    }

    public static SignalArguments Create(object data, string? tags = null)
    {
        var args = new SignalArguments();
        if(data is  string strData)
        {
            args.Data = strData;
        }
        else
        {
            args.Data = data.ToJson(); 
            //JsonSerializer.Serialize(data);
            //data.ToJson();
        }

        args.Tags = tags;
        return args;
    }
}
