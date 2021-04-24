# Reusing Initialization Code in Startup.cs

Today we are going to have some more DI fun and extend the `ServiceCollection` used in .NET to inject services. 

Some back ground info.. I am working on a big project with lots of different projects that all are sharing some of the same code base. This is all good in regards to reduce code duplication, but since these projects also use DI it means that we had a lot of the same code in `Startup.cs`. For example all our services would required logging, they pretty much all need some database access and then they also used some libraries, and so on. So, to not duplicate the initialization code in `Startup.cs` we made some small extensions on `ServiceCollection`.

I will keep this example simple, so it's easier to understand the design behind it, so I hope you can see it's a thought example 😀

## Solution

To start with we need 3 projects in a VS solution: 

- App.Shared - Class library
- App1 - Console App
- App2 - Console App

![solution-setup.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1619297548421/KdbV8SNzd.png)

## Implementation

Then we need to add a few Nugets to the `App.Shared` project:

```ps1
install-package Microsoft.Extensions.DependencyInjection
```

```ps1
install-package Microsoft.Extensions.Logging
```

```ps1
install-package Microsoft.Extensions.Logging.Console
```

### Extending `ServiceCollection`

Here's the code for that:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequiredService(this IServiceCollection services, bool isAwesome = true)
    {
        services.AddLogging(configure =>
            configure.AddConsole());
     
        services.AddSingleton(new AwesomeServiceOptions { IsAwesome = isAwesome });
        services.AddSingleton<AwesomeService>();
        return services;
    }
}
```

This code does:

1. Configure logging
2. Add options for our service
3. Add our service

### Configuring a Second Service

To see how this works we need a second service were we can inject the above service:

```csharp
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
```

As you can see, this is simple, the `constructor` is just accepting an `ILogger` and our `AwesomeService`.

### Finally, Our 2 Apps

So we have 2 different apps that we want to use DI in, here's the first one:

```csharp
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
```

And the second:

```csharp
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
```

Notice the difference? It's just the `isAwesome: true/false`.

If we run the first one, this is what it outputs:
![image.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1619298164572/Gp1od30xS.png)

Then look what happens if we run the second one:
![image.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1619298168910/hY1Gd--6m.png)

## Wrap Up

So, that's it for today's effort of making the world a little less duplicated place to live in 😁

As with all my other articles, you can find [the code](https://github.com/nicklasjepsen/blog.jepsen.ninja/tree/main/articles/Reusing%20Initialization%20Code%20in%20Startup.cs/code/ReusingInitializationCode) in this Github repo:

%[https://github.com/nicklasjepsen/blog.jepsen.ninja]