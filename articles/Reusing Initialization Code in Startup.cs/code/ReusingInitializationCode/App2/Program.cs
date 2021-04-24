using System;
using App.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace App2
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddRequiredServices(isAwesome: false)
                .AddTransient<Service>();

            var serviceProvider = services.BuildServiceProvider();
            var service = serviceProvider.GetService<Service>();
            service?.Run();

            Console.ReadKey();
        }
    }
}
