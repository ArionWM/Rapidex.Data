
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rapidex
{
    public static class DependencyInjectionExtensions
    {

        //static RapidexResolveHandler _resolveHandler;

        /// <summary>
        /// Prod ortamı için servis ekler. 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <see cref="Common.EnviromentCode"/>
        /// <returns></returns>
        public static IServiceCollection AddTransientForProd<TService, TImplementation>(this IServiceCollection services)
            where TService : class where TImplementation : class, TService
        {
            services.AddKeyedTransient<TService, TImplementation>(null);
            services.AddKeyedTransient<TService, TImplementation>(CommonConstants.ENV_PRODUCTION);
            return services;
        }

        public static IServiceCollection ReplaceTransientForProd<TService, TImplementation>(this IServiceCollection services)
            where TService : class where TImplementation : class, TService
        {
            services.Replace(ServiceDescriptor.KeyedTransient<TService, TImplementation>(null));
            services.Replace(ServiceDescriptor.KeyedTransient<TService, TImplementation>(CommonConstants.ENV_PRODUCTION));
            return services;
        }


        /// <summary>
        /// Test ortamı için servis ekler. 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <see cref="Common.EnviromentCode"/>
        /// <returns></returns>
        public static IServiceCollection AddTransientForDevelopment<TService, TImplementation>(this IServiceCollection services)
            where TService : class where TImplementation : class, TService
        {
            services.AddKeyedTransient<TService, TImplementation>(CommonConstants.ENV_DEVELOPMENT);
            return services;
        }

        /// <summary>
        /// Birim test ortamı için servis ekler. 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <see cref="Common.EnviromentCode"/>
        /// <returns></returns>
        public static IServiceCollection AddTransientForUnitTests<TService, TImplementation>(this IServiceCollection services)
            where TService : class where TImplementation : class, TService
        {
            services.AddKeyedTransient<TService, TImplementation>(CommonConstants.ENV_UNITTEST);
            return services;
        }

        /// <summary>
        /// Prod ortamı için servis ekler. 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <see cref="Common.EnviromentCode"/>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForProd<TService, TImplementation>(this IServiceCollection services)
            where TService : class where TImplementation : class, TService
        {
            services.AddKeyedSingleton<TService, TImplementation>(null);
            services.AddKeyedSingleton<TService, TImplementation>(CommonConstants.ENV_PRODUCTION);
            return services;
        }

        /// <summary>
        /// Prod ortamı için servis ekler. 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <see cref="Common.EnviromentCode"/>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForProd<TService>(this IServiceCollection services, TService singletonInstance)
            where TService : class
        {
            services.AddKeyedSingleton<TService>(null, singletonInstance);
            services.AddKeyedSingleton<TService>(CommonConstants.ENV_PRODUCTION, singletonInstance);
            return services;
        }


        public static IServiceCollection AddSingletonForProd<TService>(this IServiceCollection services, Func<IServiceProvider, object?, TService> implementationFactory)
            where TService : class
        {
            services.AddKeyedSingleton<TService>(null, implementationFactory);
            services.AddKeyedSingleton<TService>(CommonConstants.ENV_PRODUCTION, implementationFactory);
            return services;
        }
        

        /// <summary>
        /// Test ortamı için servis ekler. 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <see cref="Common.EnviromentCode"/>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForDevelopment<TService, TImplementation>(this IServiceCollection services)
            where TService : class where TImplementation : class, TService
        {
            services.AddKeyedSingleton<TService, TImplementation>(CommonConstants.ENV_DEVELOPMENT);
            return services;
        }

        /// <summary>
        /// Birim test ortamı için servis ekler. 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <see cref="Common.EnviromentCode"/>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForUnitTests<TService, TImplementation>(this IServiceCollection services)
            where TService : class where TImplementation : class, TService
        {
            services.AddKeyedSingleton<TService, TImplementation>(CommonConstants.ENV_UNITTEST);
            return services;
        }

        public static IEnumerable<object?> GetRapidexServices(this IServiceProvider provider, Type type, object? serviceKey = null)
        {
            if (serviceKey == null || (serviceKey is string serviceCode && serviceCode.IsNullOrEmpty()))
                serviceKey = Common.EnviromentCode;

            IEnumerable<object?> services = provider.GetKeyedServices(type, serviceKey);

            if (services.IsNullOrEmpty() && serviceKey.IsNOTNullOrEmpty())
            {
                switch (serviceKey)
                {
                    case CommonConstants.ENV_UNITTEST: //ENV_UNITTEST ise ve bulunamaz ise ENV_DEVELOPMENT a bakarız
                        services = provider.GetRapidexServices(type, CommonConstants.ENV_DEVELOPMENT);
                        break;

                    case CommonConstants.ENV_DEVELOPMENT: // ENV_DEVELOPMENT ise ve bulunamaz ise ENV_PRODUCTION a bakarız
                        services = provider.GetRapidexServices(type, CommonConstants.ENV_PRODUCTION);
                        break;

                }
            }

            if (services.IsNullOrEmpty())
                services = provider.GetServices(type);

            return services;
        }

        public static IEnumerable<T> GetRapidexServices<T>(this IServiceProvider provider, object? serviceKey = null)
        {
            return provider.GetRapidexServices(typeof(T), serviceKey).Cast<T>();
        }

        public static object GetRapidexService(this IServiceProvider provider, Type type, object? serviceKey = null)
        {
            return provider.GetRapidexServices(type, serviceKey).FirstOrDefault();

        }

        public static T GetRapidexService<T>(this IServiceProvider provider, object? serviceKey = null)
        {
            return (T)provider.GetRapidexService(typeof(T), serviceKey);
        }

    }
}
