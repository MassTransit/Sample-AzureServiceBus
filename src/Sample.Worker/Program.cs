using System;
using MassTransit.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Worker
{
    using System.Threading.Tasks;
    using Consumers;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using StateMachines;


    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //AddMT(services, hostContext);
                    services.AddHostedService<SbHostedService>();
                });
        }

        private static void AddMT(IServiceCollection services, HostBuilderContext hostContext)
        {
            services.AddMassTransit(x =>
            {
                x.AddServiceBusMessageScheduler();

                x.SetKebabCaseEndpointNameFormatter();

                x.AddConsumer<OrderSubmittedConsumer>(w =>
                {
                });
                //x.AddConsumer<OrderSubmittedConsumer>();

                //x.AddSagaStateMachine<OrderShipmentStateMachine, OrderShipmentState, OrderShipmentSagaDefinition>()
                //    .MessageSessionRepository();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(hostContext.Configuration.GetConnectionString("AzureServiceBus"));

                    cfg.UseServiceBusMessageScheduler();

                    cfg.Send<OrderSubmitted>(s => s.UseSessionIdFormatter(c => c.Message.OrderId.ToString("D")));
                    cfg.Send<MonitorOrderShipmentTimeout>(s => s.UseSessionIdFormatter(c => c.Message.OrderId.ToString("D")));

                    // Subscribe to OrderSubmitted directly on the topic, instead of configuring a queue
                    cfg.SubscriptionEndpoint<OrderSubmitted>("order-submitted-consumer", e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        e.AutoStart = true;
                        e.AutoDeleteOnIdle = TimeSpan.FromDays(30);
                        e.MaxDeliveryCount = 3;
                        e.RequiresSession = true;
                        e.MaxConcurrentCalls = 3;
                        e.PrefetchCount = 0;
                        e.ConfigureConsumer<OrderSubmittedConsumer>(context);
                    });

                    //cfg.ConfigureEndpoints(context);
                    cfg.UseRateLimit(500, TimeSpan.FromMinutes(1)); // max for bus
                    cfg.UseConcurrencyLimit(1); // does nothing
                    cfg.PrefetchCount = 0;
                });
            });
        }
    }
}