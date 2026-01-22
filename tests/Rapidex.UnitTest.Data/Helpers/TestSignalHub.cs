using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.Helpers;

internal class TestSignalHub : Rapidex.SignalHub.SignalHub
{
    public TestSignalHub(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override bool IsSynchronousSignal(SignalTopic topic, ISignalArguments args)
    {
        return true;
    }
}
