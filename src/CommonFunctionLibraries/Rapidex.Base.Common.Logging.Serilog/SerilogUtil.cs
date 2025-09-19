using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using System;
using System.IO;
using Rapidex.Base;
using Microsoft.Extensions.DependencyInjection;
using Rapidex.Logging.Serilog.Core8;
using Microsoft.Extensions.Hosting;


namespace Rapidex
{
    public static class SerilogExtensions
    {
        public static Serilog.ILogger Setup(string logFolder, LogEventLevel minLevel = LogEventLevel.Information)
        {
            bool isDebug = Rapidex.Common.ENV != CommonConstants.ENV_PRODUCTION;

            //See: https://github.com/serilog/serilog/wiki/Formatting-Output
            //[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}
            //{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}

            string defaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}, [{Level:u3}], {Message:lj}{NewLine}{Exception}";

            Logger debugLogger = new LoggerConfiguration()
               //.MinimumLevel.Information()
               .MinimumLevel.Debug()
               .WriteTo.Async(a => a.File(Path.Combine(logFolder, "Debug_.log"), outputTemplate: defaultOutputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, retainedFileCountLimit: 20, rollOnFileSizeLimit: true, buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(isDebug ? 1 : 300)), bufferSize: isDebug ? 1 : 500)
               //.WriteTo.File(Path.Combine(logFolder, "debug.log"), outputTemplate: defaultOutputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, retainedFileCountLimit: 10, rollOnFileSizeLimit: true, buffered: true)
               .CreateLogger();

            Logger infoLogger = new LoggerConfiguration()
               .MinimumLevel.Information()
               .WriteTo.Async(a => a.File(Path.Combine(logFolder, "Info_.log"), outputTemplate: defaultOutputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, retainedFileCountLimit: 100, rollOnFileSizeLimit: true, buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(isDebug ? 1 : 60)), bufferSize: isDebug ? 1 : 50)
               .CreateLogger();

            Logger errorLogger = new LoggerConfiguration()
              .MinimumLevel.Error()
              .WriteTo.File(Path.Combine(logFolder, "Error_.log"), outputTemplate: defaultOutputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, retainedFileCountLimit: 370, rollOnFileSizeLimit: true, buffered: false)//, flushToDiskInterval: TimeSpan.FromSeconds(10))
              .CreateLogger();

            Logger jobSystemLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.File(Path.Combine(logFolder, "Jobs_.log"), outputTemplate: defaultOutputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, retainedFileCountLimit: 100, rollOnFileSizeLimit: true, buffered: true), bufferSize: isDebug ? 1 : 50)
                .CreateLogger();


            LoggerConfiguration finalConf = new LoggerConfiguration();

            finalConf.MinimumLevel.Is(minLevel);


            finalConf = finalConf
                .MinimumLevel.Override("Microsoft", minLevel) //LogEventLevel.Warning
                .MinimumLevel.Override("System", LogEventLevel.Warning);

            //finalConf = finalConf.Enrich.FromLogContext();

            var logger = finalConf

                .WriteTo.Logger(
                    l => l.Filter.ByExcluding(Matching.WithProperty("Category"))
                            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == Serilog.Events.LogEventLevel.Debug).WriteTo.Logger(debugLogger))
                            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == Serilog.Events.LogEventLevel.Information).WriteTo.Logger(infoLogger))
                            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == Serilog.Events.LogEventLevel.Error).WriteTo.Logger(errorLogger))
                            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(Matching.FromSource("ProCore.JobMan")).WriteTo.Logger(jobSystemLogger))
                        )
                        .WriteTo.Logger(
                            l => l.Filter.ByIncludingOnly(Matching.WithProperty("Category"))
                                    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == Serilog.Events.LogEventLevel.Error).WriteTo.Logger(errorLogger))
                                    .WriteTo.Map("Category", string.Empty,
                                        (category, wt) =>
                                        {
                                            string fileName = $"{category.ToFriendly()}_.log";
                                            string path = Path.Combine(logFolder, fileName);
                                            wt.Async(
                                                    a => a.File(path, outputTemplate: defaultOutputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, retainedFileCountLimit: 50, rollOnFileSizeLimit: true, buffered: true, flushToDiskInterval: TimeSpan.FromSeconds(isDebug ? 1 : 60)), bufferSize: isDebug ? 1 : 50);
                                        }
                                    )
                        )
                        .Enrich.FromLogContext()
                        .CreateLogger();

            return logger;

        }

        public static Serilog.ILogger UseSerilog(this IServiceCollection services, string logFolder, LogEventLevel minLevel = LogEventLevel.Information)
        {
            //LogEventLevel minLevel

            LogEventLevel internalMinLevel = LogEventLevel.Information;


#if DEBUG
            internalMinLevel = LogEventLevel.Debug;
#else
            internalMinLevel = LogEventLevel.Information;
#endif

#if VERBOSE
            internalMinLevel = LogEventLevel.Verbose;
#endif

            if (internalMinLevel > minLevel)
            {
                internalMinLevel = minLevel;
            }


            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var logger = SerilogExtensions.Setup(logFolder, internalMinLevel);

            Rapidex.Log.Logger = new LoggingHelper(logger);

            services.AddSerilog(logger);
            //builder.Host.UseSerilog(logger);
            services.AddSingleton<Serilog.ILogger>(logger);
            services.AddSingleton<ILoggingHelper>(Rapidex.Log.Logger);
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

            return logger;
        }

        public static void UseSerilog(this IHostApplicationBuilder builder, string logFolder, LogEventLevel minLevel = LogEventLevel.Information)
        {
            Serilog.ILogger logger = UseSerilog(builder.Services, logFolder, minLevel);

            builder.Logging
                .ClearProviders()
                .AddSerilog(logger);
        }

        public static void Flush()
        {
            //Serilog.Log.CloseAndFlush();
        }
    }


}
