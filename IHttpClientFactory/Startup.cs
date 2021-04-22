using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Blog.Jepsen.Ninja.Startup))]

namespace Blog.Jepsen.Ninja
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("blog", c =>
                {
                    c.BaseAddress = new Uri("https://blog.jepsen.ninja");
                    c.DefaultRequestHeaders.Add("User-Agent", "Blog.Jepsen.Ninja.Functions");
                });
        }
    }
}