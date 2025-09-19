using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;
public static class SignalHelpers
{
    public static ISignalHandlingResult CreateResult(this ISignalArguments args)
    {
        var res = new SignalHandlingResult(args.HandlerId);
        return res;

    }
}
