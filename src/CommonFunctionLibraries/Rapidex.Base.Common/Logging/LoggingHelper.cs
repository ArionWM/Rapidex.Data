using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Logging;

public class LoggingHelper
{
    public static void LogSystemInformation()
    {
        var logger = Rapidex.Common.DefaultLogger;
        try
        {
            if (logger != null)
            {
                logger.LogInformation("Logging system information...");
                logger.LogInformation("Operating System: {OSDescription}", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
                logger.LogInformation("OS Architecture: {OSArchitecture}", System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
                logger.LogInformation("Process Architecture: {ProcessArchitecture}", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture);
                logger.LogInformation("Framework Version: {FrameworkVersion}", System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion());
                logger.LogInformation("Framework Description: {FrameworkDescription}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
                logger.LogInformation("Machine Name: {MachineName}", Environment.MachineName);
                logger.LogInformation("Processor Count: {ProcessorCount}", Environment.ProcessorCount);
                logger.LogInformation("System Directory: {SystemDirectory}", Environment.SystemDirectory);
                logger.LogInformation("Current Directory: {CurrentDirectory}", Environment.CurrentDirectory);
                logger.LogInformation("User Domain Name: {UserDomainName}", Environment.UserDomainName);
                logger.LogInformation("User Name: {UserName}", Environment.UserName);
                logger.LogInformation("CLR Version: {CLRVersion}", Environment.Version.ToString());

                if (Rapidex.Common.Assembly != null)
                {
                    foreach (var assemblyInfo in Rapidex.Common.Assembly.AssemblyDefinitions)
                    {
                        logger.LogInformation("Assembly: {AssemblyName}, {RapidexName}, Version: {AssemblyVersion}", assemblyInfo.Assembly.GetName().Name, assemblyInfo.Name, assemblyInfo.Assembly.GetName().Version);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while logging system information.");
        }
    }

}
