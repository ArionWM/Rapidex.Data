using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.SignalHub;

namespace Rapidex.Data.Behaviors.Behaviors;
internal class SignalHelper
{
    public static  void RegisterDefinitions(ISignalHub hub)
    {
        hub.RegisterSignalDefinition(new SignalDefinition(SignalConstants.Signal_Archived, "Archived", "Entity + Behavior", false));
        hub.RegisterSignalDefinition(new SignalDefinition(SignalConstants.Signal_Unarchived, "Unarchived", "Entity + Behavior", false));

    }
}
