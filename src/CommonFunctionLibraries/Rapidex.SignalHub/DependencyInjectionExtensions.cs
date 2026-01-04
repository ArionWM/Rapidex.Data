using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Rapidex.SignalHub;

namespace Rapidex;
public static class DependencyInjectionExtensions
{
    public static void AddRapidexSignalHub(this IServiceCollection services)
    {
        services.AddSingleton<ISignalHub>((sp) =>
        {
            Rapidex.Signal.Hub = SignalHubFactory.Create();
            return Rapidex.Signal.Hub;
        });

    }

    public static void StartRapidexSignalHub(this IServiceProvider serviceProvider)
    {
        ISignalHub hub = serviceProvider.GetRapidexService<ISignalHub>();
        hub.Start(serviceProvider);
    }
}
