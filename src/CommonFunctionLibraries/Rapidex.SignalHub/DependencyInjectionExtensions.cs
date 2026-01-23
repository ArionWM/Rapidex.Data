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
        services.AddTransient<SignalHubFactory>();
        services.AddSingleton<ISignalHub, Rapidex.SignalHub.SignalHub>(sp =>
        {
            var hub = new Rapidex.SignalHub.SignalHub(sp);
            Signal.Hub = hub;
            return hub;
        });
    }

    public static void StartRapidexSignalHub(this IServiceProvider serviceProvider)
    {
        ISignalHub hub = serviceProvider.GetRapidexService<ISignalHub>();
        hub.Start(serviceProvider);
    }
}
