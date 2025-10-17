using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public static class DependencyInjectionExtensions
{
    public static void AddRapidexDataLevel(this IServiceCollection services, string rootFolder = null, string binaryFolder = null, IConfiguration configuration = null)
    {
        if (rootFolder.IsNullOrEmpty())
            rootFolder = AppContext.BaseDirectory;

        if (binaryFolder.IsNullOrEmpty())
            binaryFolder = AppContext.BaseDirectory;

        Rapidex.Common.Setup(rootFolder, binaryFolder, services, configuration);
        Rapidex.Common.Assembly.SetupAssemblyServices(services);
        Rapidex.Data.Database.Setup();
    }

    public static void StartRapidexDataLevel(this IServiceProvider sp)
    {
        Rapidex.Common.Start(sp);
        Rapidex.Data.Database.Start(sp);
        Rapidex.Common.Assembly.StartAssemblies(sp);

        var dbScope = Database.Dbs.AddMainDbIfNotExists();
        dbScope.Metadata.ScanConcreteDefinitions();
        dbScope.Metadata.ScanSoftDefinitions();
        dbScope.Structure.ApplyAllStructure();
    }
}
