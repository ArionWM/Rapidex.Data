using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;
public class Signal
{
    private static ISignalHub SIGNAL_HUB;

    public static ISignalHub Hub
    {
        get
        {
            return SIGNAL_HUB;
        }

        internal set
        {
            if (SIGNAL_HUB != null)
            {
                throw new InvalidOperationException("MessageHub already set");
            }
            SIGNAL_HUB = value;
        }
    }

    internal static void ClearHubForTest()
    {
        SIGNAL_HUB = null;
    }
}
