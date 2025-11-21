using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rapidex;
public static class LoggingExtensions
{

    //public static void SetMinimumLogLevelAndOthers(this RapidexLoggingConfiguration conf, string[] assemblyNamesOrNameStarts, LogLevel targetAssembliesLevel, LogLevel othersLevel)
    //{
    //    conf.DefaultMinimumLevel = targetAssembliesLevel;
    //    foreach (var asm in assemblyNamesOrNameStarts)
    //    {
    //        conf.CategoryMinimumLevels[asm] = targetAssembliesLevel;
    //    }
    //}

    public static void LogError(this ILogger logger, Exception? exception)
    {
        if (exception != null)
        {
            logger.LogError(exception, exception.Message);
        }
    }

    public static void LogError(this ILogger logger, Exception? exception, string message = null)
    {
        if (exception != null)
        {
            logger.LogError(exception, message ?? exception.Message);
        }
    }

    public static void Log(this Exception ex, string message = null)
    {
        Common.DefaultLogger?.LogError(ex, message);
    }

    public static void LogDebug(this ILogger logger, string category, string message, params object[] args)
    {
        logger.LogDebug(message, args);
    }

    public static void LogInformation(this ILogger logger, string category, string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }

    public static void LogWarning(this ILogger logger, string category, string message, params object[] args)
    {
        logger.LogWarning(message, args);
    }

    public static void LogCritical(this ILogger logger, string category, string message, params object[] args)
    {
        logger.LogCritical(message, args);
    }

    public static void LogError(this ILogger logger, string category, string message, params object[] args)
    {
        logger.LogError(message, args);
    }

    public static void LogTrace(this ILogger logger, string category, string message, params object[] args)
    {
        logger.LogTrace(message, args);
    }

}
