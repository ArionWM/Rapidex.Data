using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rapidex.Data;
public static class DependencyInjectionExtensions
{
    public static void AddRapidexDataLevel(this IServiceCollection services, string rootFolder = null, string binaryFolder = null, IConfiguration configuration = null)
    {
        try
        {
            if (rootFolder.IsNullOrEmpty())
                rootFolder = AppContext.BaseDirectory;

            if (binaryFolder.IsNullOrEmpty())
                binaryFolder = AppContext.BaseDirectory;

            Rapidex.Common.Setup(rootFolder, binaryFolder, services, configuration);
            Rapidex.Common.Assembly.SetupAssemblyServices(services);
            Rapidex.Data.Database.Setup();
        }
        catch (Exception ex)
        {
            ex.Log();
            Thread.Sleep(3000); //give some time to log flush
            throw;
        }
    }

    public static void StartRapidexDataLevel(this IServiceProvider sp)
    {
        try
        {
            Rapidex.Common.Start(sp);
            Rapidex.Data.Database.Start(sp);
            Rapidex.Common.Assembly.InitializeAssemblies(sp);

            var dbScope = Database.Dbs.AddMainDbIfNotExists();
            dbScope.Metadata.ScanConcreteDefinitions();
            dbScope.Metadata.ScanSoftDefinitions();
            dbScope.Structure.ApplyAllStructure();

            Rapidex.Common.Assembly.StartAssemblies(sp);
        }
        catch (Exception ex)
        {
            ex.Log();
            Thread.Sleep(3000); //give some time to log flush
            throw;
        }
    }
}
