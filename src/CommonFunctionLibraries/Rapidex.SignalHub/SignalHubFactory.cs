using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;

public class SignalHubFactory
{
    public static ISignalHub Create()
    {
        Rapidex.SignalHub.SignalHub hub = new Rapidex.SignalHub.SignalHub();
        return hub;
    }
}
