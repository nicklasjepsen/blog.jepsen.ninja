using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRequiredServices(this IServiceCollection services, bool isAwesome = true)
        {
            services.AddLogging(configure =>
                configure.AddConsole());
         
            services.AddSingleton(new AwesomeServiceOptions { IsAwesome = isAwesome });
            services.AddSingleton<AwesomeService>();

            return services;
        }
    }
}
