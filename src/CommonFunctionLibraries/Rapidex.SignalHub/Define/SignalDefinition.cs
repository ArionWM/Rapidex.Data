using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;
public class SignalDefinition : ISignalDefinition
{
    public string SignalName { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public bool IsEntityReleated { get; set; } = false; //Is the signal related to an entity. If true, the signal will be raised with an entity id and name. If false, the signal will be raised without an entity id and name.
    public bool IsSynchronous { get; set; } = false;

    public SignalDefinition(string signalName, string category, bool isEntityReleated = false, bool isSynchronous = false)
    {
        SignalName = signalName;
        Category = category;
        IsEntityReleated = isEntityReleated;
        IsSynchronous = isSynchronous;
    }

    public SignalDefinition(string signalName, string description, string category, bool isEntityReleated = false, bool isSynchronous = false)
    {
        SignalName = signalName;
        Description = description;
        Category = category;
        IsEntityReleated = isEntityReleated;
        IsSynchronous = isSynchronous;
    }
}
