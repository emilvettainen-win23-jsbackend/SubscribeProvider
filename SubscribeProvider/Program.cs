using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubscribeProvider.Infrastructure.Data.Contexts;
using SubscribeProvider.Infrastructure.Data.Repositories;
using SubscribeProvider.Infrastructure.Services;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context,services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddPooledDbContextFactory<SubscribeDataContext>(x => x.UseSqlServer(context.Configuration["AzureDb"]));
        services.AddScoped<SubscribeService>();
        services.AddScoped<SubscribeRepository>();
        services.AddSingleton(new ServiceBusClient(context.Configuration["SERVICEBUS_CONNECTION"]));


    })
    .Build();

host.Run();