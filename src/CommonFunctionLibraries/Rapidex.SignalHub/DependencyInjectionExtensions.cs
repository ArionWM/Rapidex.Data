using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Rapidex;
public static class DependencyInjectionExtensions
{
    public static void AddRapidexSignalHub(this IServiceCollection services)
    {
        services.AddSingleton<ISignalHub>((sp) =>
        {
            SignalHub.SignalHub hub = new SignalHub.SignalHub();
            Rapidex.Signal.Hub = hub;
            return hub;
        });

    }

    public static void StartRapidexSignalHub(this IServiceProvider serviceProvider)
    {
        ISignalHub hub = serviceProvider.GetRapidexService<ISignalHub>();
        hub.Start(serviceProvider);
    }
}
