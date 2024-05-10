using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubscribeProvider.Data.Contexts;
using SubscribeProvider.Data.Repositories;
using SubscribeProvider.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context,services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContext<SubscribeDataContext>(x => x.UseSqlServer(context.Configuration.GetConnectionString("AzureDb")));
        services.AddScoped<SubscribeService>();
        services.AddScoped<SubscribeRepository>();
    })
    .Build();

host.Run();
