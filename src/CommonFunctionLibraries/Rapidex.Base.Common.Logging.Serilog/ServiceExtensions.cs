using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.File;
using System.Collections.Concurrent;

namespace Rapidex.Base.Common.Logging.Serilog.Core8;

/// <summary>
/// Rapidex Serilog entegrasyonu için extension metodlarý
/// </summary>
public static class ServiceExtensions
{
    private static LoggingLevelSwitch? GlobalLevelSwitch;
    private static ConcurrentDictionary<string, LoggingLevelSwitch>? CategoryLevelSwitches;

    public static IServiceCollection UseRapidexSerilog(this IServiceCollection services, ILoggingBuilder loggingBuilder, IConfiguration configuration = null, IHostEnvironment environment = null, Action<RapidexLoggingConfiguration>? configureOptions = null)
    {
        services.NotNull();

        configuration = configuration ?? Rapidex.Common.Configuration;

        // Konfigürasyonu yükle
        var config = new RapidexLoggingConfiguration();
        configuration.GetSection("RapidexLogging").Bind(config);
        configureOptions?.Invoke(config);

        // Level switch'leri oluþtur
        GlobalLevelSwitch = new LoggingLevelSwitch(ConvertToSerilogLevel(config.DefaultMinimumLevel));
        CategoryLevelSwitches = new ConcurrentDictionary<string, LoggingLevelSwitch>();

        // Kategori bazlý level switch'leri oluþtur
        foreach (var kvp in config.CategoryMinimumLevels)
        {
            var levelSwitch = new LoggingLevelSwitch(ConvertToSerilogLevel(kvp.Value));
            CategoryLevelSwitches.TryAdd(kvp.Key, levelSwitch);
        }

        // Serilog konfigürasyonunu oluþtur
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(GlobalLevelSwitch)
            .Enrich.FromLogContext();

        if (environment != null)
        { 
            loggerConfig 
                .Enrich.WithProperty("Application", environment.ApplicationName)
                .Enrich.WithProperty("Environment", environment.EnvironmentName);
        }

        // Kategori bazlý override'lar
        foreach (var kvp in CategoryLevelSwitches)
        {
            loggerConfig.MinimumLevel.Override(kvp.Key, kvp.Value);
        }

        ConfigureFileSinks(loggerConfig, config);


        // Serilog'u ayarla
        var serilogLogger = loggerConfig.CreateLogger();

        //Log.Logger = serilogLogger;

        // ASP.NET Core logging'i Serilog'a yönlendir
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(serilogLogger, dispose: true);

        //// Service'leri kaydet
        //builder.Services.AddSingleton<ILoggingHelper>(sp => 
        //    new SerilogLoggingHelper(sp.GetRequiredService<ILoggerFactory>()));

        services.AddSingleton<ILogLevelController>(sp =>
            new SerilogLevelController(GlobalLevelSwitch, CategoryLevelSwitches));

        // Log static class'ýný ayarla
        //var serviceProvider = builder.Services.BuildServiceProvider();
        //Rapidex.Log.Logger = serviceProvider.GetRequiredService<ILoggingHelper>();

        return services;
    }


    /// <summary>
    /// Rapidex Serilog implementasyonunu kullanýr
    /// </summary>
    public static IHostApplicationBuilder UseRapidexSerilog(this IHostApplicationBuilder builder, Action<RapidexLoggingConfiguration>? configureOptions = null)
    {
        builder.NotNull();

        builder.Services.UseRapidexSerilog(builder.Logging, builder.Configuration, builder.Environment, configureOptions);

        return builder;
    }

    private static void ConfigureFileSinks(LoggerConfiguration loggerConfig, RapidexLoggingConfiguration config)
    {
        var logDir = System.IO.Path.Combine(AppContext.BaseDirectory, config.LogDirectory);
        System.IO.Directory.CreateDirectory(logDir);

        // Ana log dosyasý (tüm loglar - buffered)
        var mainLogPath = System.IO.Path.Combine(logDir, $"{config.LogFilePrefix}-.log");

        if (config.UseBufferForNonErrors)
        {
            loggerConfig.WriteTo.Async(a => a.File(
                mainLogPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: config.OutputTemplate,
                fileSizeLimitBytes: config.MaxLogFileSize,
                retainedFileCountLimit: config.RetainedFileCountLimit,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromSeconds(5)
            ));
        }
        else
        {
            loggerConfig.WriteTo.File(
                mainLogPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: config.OutputTemplate,
                fileSizeLimitBytes: config.MaxLogFileSize,
                retainedFileCountLimit: config.RetainedFileCountLimit
            );
        }

        // Error loglarý için ayrý dosya (unbuffered - immediate write)
        if (config.UseSeperateErrorLogFile)
        {
            var errorLogPath = System.IO.Path.Combine(logDir, $"{config.LogFilePrefix}-error-.log");
            loggerConfig.WriteTo.File(
                errorLogPath,
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                outputTemplate: config.OutputTemplate,
                fileSizeLimitBytes: config.MaxLogFileSize,
                retainedFileCountLimit: config.RetainedFileCountLimit,
                buffered: false // Error'lar için buffer yok
            );
        }

        // Warning loglarý için ayrý dosya (unbuffered)
        if (config.UseSeperateWarningLogFile)
        {
            var warningLogPath = System.IO.Path.Combine(logDir, $"{config.LogFilePrefix}-warning-.log");
            loggerConfig.WriteTo.File(
                warningLogPath,
                restrictedToMinimumLevel: LogEventLevel.Warning,
                levelSwitch: new LoggingLevelSwitch(LogEventLevel.Warning)
                {
                    MinimumLevel = LogEventLevel.Warning
                },
                rollingInterval: RollingInterval.Day,
                outputTemplate: config.OutputTemplate,
                fileSizeLimitBytes: config.MaxLogFileSize,
                retainedFileCountLimit: config.RetainedFileCountLimit,
                buffered: false
            );
        }

        // Kategori bazlý ayrý dosyalar
        foreach (var kvp in config.CategorySeparateFiles.Where(x => x.Value))
        {
            var category = kvp.Key;
            var categoryLogPath = System.IO.Path.Combine(logDir, $"{SanitizeFileName(category)}-.log");

            loggerConfig.WriteTo.Map(
                le => le.Properties.TryGetValue("SourceContext", out var sourceContext)
                      && sourceContext.ToString().Trim('"').StartsWith(category)
                    ? category
                    : "default",
                (name, wt) =>
                {
                    if (name == category)
                    {
                        wt.File(
                            categoryLogPath,
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: config.OutputTemplate,
                            fileSizeLimitBytes: config.MaxLogFileSize,
                            retainedFileCountLimit: config.RetainedFileCountLimit,
                            buffered: config.UseBufferForNonErrors
                        );
                    }
                });
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalids = System.IO.Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries));
    }

    private static LogEventLevel ConvertToSerilogLevel(int level)
    {
        return level switch
        {
            0 => LogEventLevel.Verbose,
            1 => LogEventLevel.Debug,
            2 => LogEventLevel.Information,
            3 => LogEventLevel.Warning,
            4 => LogEventLevel.Error,
            5 => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
