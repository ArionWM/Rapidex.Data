using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System.Collections.Concurrent;
using SerilogEvents = Serilog.Events;

namespace Rapidex.Base.Common.Logging.Serilog.Core8;

/// <summary>
/// Runtime'da Serilog loglama seviyelerini kontrol eden sýnýf
/// </summary>
public class SerilogLevelController : ILogLevelController
{
    private readonly LoggingLevelSwitch globalLevelSwitch;
    private readonly ConcurrentDictionary<string, LoggingLevelSwitch> categoryLevelSwitches;

    public SerilogLevelController(LoggingLevelSwitch globalLevelSwitch, ConcurrentDictionary<string, LoggingLevelSwitch> categoryLevelSwitches)
    {
        this.globalLevelSwitch = globalLevelSwitch.NotNull();
        this.categoryLevelSwitches = categoryLevelSwitches.NotNull();
    }

    public void SetMinimumLevel(LogLevel logLevel)
    {
        globalLevelSwitch.MinimumLevel = ConvertToSerilogLevel(logLevel);
    }

    public void SetMinimumLevel(string sourceContext, LogLevel logLevel)
    {
        sourceContext.NotNull();
        
        if (categoryLevelSwitches.TryGetValue(sourceContext, out var levelSwitch))
        {
            levelSwitch.MinimumLevel = ConvertToSerilogLevel(logLevel);
        }
        else
        {
            var newSwitch = new LoggingLevelSwitch(ConvertToSerilogLevel(logLevel));
            categoryLevelSwitches.TryAdd(sourceContext, newSwitch);
        }
    }

    public LogLevel GetMinimumLevel()
    {
        return ConvertFromSerilogLevel(globalLevelSwitch.MinimumLevel);
    }

    public LogLevel? GetMinimumLevel(string sourceContext)
    {
        sourceContext.NotNull();
        
        if (categoryLevelSwitches.TryGetValue(sourceContext, out var levelSwitch))
        {
            return ConvertFromSerilogLevel(levelSwitch.MinimumLevel);
        }
        return null;
    }

    public void ResetAllLevels()
    {
        categoryLevelSwitches.Clear();
    }

    private static SerilogEvents.LogEventLevel ConvertToSerilogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => SerilogEvents.LogEventLevel.Verbose,
            LogLevel.Debug => SerilogEvents.LogEventLevel.Debug,
            LogLevel.Information => SerilogEvents.LogEventLevel.Information,
            LogLevel.Warning => SerilogEvents.LogEventLevel.Warning,
            LogLevel.Error => SerilogEvents.LogEventLevel.Error,
            LogLevel.Critical => SerilogEvents.LogEventLevel.Fatal,
            LogLevel.None => SerilogEvents.LogEventLevel.Fatal,
            _ => SerilogEvents.LogEventLevel.Information
        };
    }

    private static LogLevel ConvertFromSerilogLevel(SerilogEvents.LogEventLevel level)
    {
        return level switch
        {
            SerilogEvents.LogEventLevel.Verbose => LogLevel.Trace,
            SerilogEvents.LogEventLevel.Debug => LogLevel.Debug,
            SerilogEvents.LogEventLevel.Information => LogLevel.Information,
            SerilogEvents.LogEventLevel.Warning => LogLevel.Warning,
            SerilogEvents.LogEventLevel.Error => LogLevel.Error,
            SerilogEvents.LogEventLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }
}
