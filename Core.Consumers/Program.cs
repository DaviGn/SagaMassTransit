using Core.Consu;
using MassTransit;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            x.AddConsumers(entryAssembly);
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, cfg) => { cfg.ConfigureEndpoints(context); });
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
