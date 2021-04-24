using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace App.Shared
{
    public class Service
    {
        private readonly ILogger<Service> log;
        private readonly AwesomeService awesomeService;

        public Service(ILogger<Service> log, AwesomeService awesomeService)
        {
            log.LogDebug($"{nameof(Service)} initializing...");

            this.log = log;
            this.awesomeService = awesomeService;

            log.LogDebug($"{nameof(Service)} initialized.");
        }

        public void Run()
        {
            Console.WriteLine($"{nameof(AwesomeService.IsAwesome)}: {awesomeService.IsAwesome}");
        }
    }
}
