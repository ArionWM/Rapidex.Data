using Microsoft.Extensions.Configuration;
using Rapidex.Base.Common.Assemblies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

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
            Common.Configuration = new ConfigurationBuilder()
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", false, true)
                            .Build();
        }

        /// <summary>
        /// Load configuration and prepare for work
        /// </summary>
        /// <param name="configuration"></param>
        public static void Setup(string rootFolder, string binaryFolder, IServiceCollection services, IConfiguration configuration = null)
        {
            if (configuration != null)
                Common.Configuration = configuration;

            RootFolder = rootFolder;
            BinaryFolder = binaryFolder;
            DataFolder = Path.Combine(rootFolder, "App_Data");

            if (configuration == null && Common.Configuration == null)
                LoadConfiguration();

            AssemblyManager asman = new AssemblyManager();
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
        }

    }
}
