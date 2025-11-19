using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rapidex.Base.Common.Assemblies;

namespace Rapidex
{
    public class Common
    {
#if DEBUG
        public const string ENV = CommonConstants.ENV_DEVELOPMENT;
#else
        public const string ENV = CommonConstants.ENV_PRODUCTION;
#endif


#if UNITTEST
        public const string ENV = CommonConstants.ENV_UNITTEST;
#endif

        internal static IServiceProvider InternalServiceProvider;

        /// <summary>
        /// Alabileceği değerler; 
        /// - Test (birim testleri) 
        /// - Development (ya da Development)
        /// -- Production (ya da boş ise 'prod' olarak kabul edilir)
        /// See: CommonConstants
        /// See: IHostingEnvironment.HostingEnvironment
        /// See: launchSettings.json
        /// </summary>
        /// <see cref="https://stackoverflow.com/questions/28258227/how-to-set-environment-name-ihostingenvironment-environmentname"/>
        /// <see cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-5.0"/>"/>

        public static string EnviromentCode { get; set; } = Common.ENV;
        public static string RootFolder { get; set; }
        public static string BinaryFolder { get; set; }
        public static string DataFolder { get; set; }

        public static IConfiguration Configuration { get; set; }
        public static ILogger DefaultLogger { get; internal set; }
        public static RapidexTypeConverter Converter { get; internal set; }
        public static AssemblyManager Assembly { get; set; }// = new AssemblyManager();

        public static ITimeProvider Time { get; internal set; }

        public static IExceptionManager ExceptionManager { get; internal set; } = new ExceptionManagerBase();
        public static IServiceProvider ServiceProvider
        {
            get
            {
                return InternalServiceProvider
                    .NotNull("Common service provider not set");
            }
            set
            {
                InternalServiceProvider = value;
            }
        }
        public static void LoadConfiguration()
        {
            var cBuilder = new ConfigurationBuilder()
                             .SetBasePath(AppContext.BaseDirectory)
                             .AddJsonFile("appsettings.json", true, true)
                             .AddJsonFile($"appsettings.{Common.EnviromentCode}.json", true, true);

            Common.Configuration = cBuilder.Build();
        }

        /// <summary>
        /// Load configuration and prepare for work
        /// </summary>
        /// <param name="configuration"></param>
        public static void Setup(string rootFolder, string binaryFolder, IServiceCollection services, IConfiguration configuration = null, ILogger defaultLogger = null)
        {
            if (configuration != null)
                Common.Configuration = configuration;

            RootFolder = rootFolder;
            BinaryFolder = binaryFolder;
            DataFolder = Path.Combine(rootFolder, "App_Data");

            if (configuration == null && Common.Configuration == null)
                LoadConfiguration();

            DefaultLogger = defaultLogger;

            AssemblyManager asman = new AssemblyManager(defaultLogger);
            Common.Assembly = asman;
            services.AddSingleton<AssemblyManager>(asman);
            Common.Assembly.Setup(services);

            Common.Converter = new RapidexTypeConverter();
            Common.Converter.Setup(services);
            Rapidex.Common.Time = new DefaultTimeProvider(); //TODO: ServiceProvider?

            MappingHelper.Setup();

            //Geçici olarak boş bir sp yerleştiriyoruz
            //Rapidex.Common.ServiceProvider = services.BuildServiceProvider();



            //MappingHelper.Setup();

        }

        public static void Start(IServiceProvider serviceProvider)
        {
            MappingHelper.Start();

            ExceptionManager = serviceProvider.GetRapidexService<IExceptionManager>();
            ServiceProvider = serviceProvider;

            if (Common.DefaultLogger == null)
            {
                Common.DefaultLogger = serviceProvider.GetService<ILogger<Common>>();
            }
        }

    }
}
