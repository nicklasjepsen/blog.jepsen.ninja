using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace App.Shared
{
    public class AwesomeService
    {
        private readonly ILogger<AwesomeService> log;
        private readonly AwesomeServiceOptions options;

        public AwesomeService(ILogger<AwesomeService> log, AwesomeServiceOptions options)
        {
            this.log = log;
            this.options = options;

            if(!this.options.IsAwesome)
                log.LogCritical($"Service is not awesome!");
        }

        public bool IsAwesome => options.IsAwesome;
    }
}
