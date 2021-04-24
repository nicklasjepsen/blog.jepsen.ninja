using System;
using App.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App1
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddRequiredServices()
                .AddTransient<Service>();

            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<Service>();
            service?.Run();

            Console.ReadKey();
        }
    }
}
