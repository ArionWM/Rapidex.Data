using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;


public class SignalHubFactory
{
    private IServiceProvider serviceProvider;

    public SignalHubFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public ISignalHub Create()
    {
        Rapidex.SignalHub.SignalHub hub = new Rapidex.SignalHub.SignalHub(this.serviceProvider);
        return hub;
    }
}
