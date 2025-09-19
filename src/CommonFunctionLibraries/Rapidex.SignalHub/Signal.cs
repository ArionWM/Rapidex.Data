using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;
public class Signal
{
    private static ISignalHub signalHub;

    public static ISignalHub Hub
    {
        get
        {
            return signalHub;
        }

        internal set
        {
            if (signalHub != null)
            {
                throw new InvalidOperationException("MessageHub already set");
            }
            signalHub = value;
        }
    }

    internal static void ClearHubForTest()
    {
        signalHub = null;
    }
}
