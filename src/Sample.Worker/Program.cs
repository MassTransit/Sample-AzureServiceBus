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
                    services.AddMassTransit(x =>
                    {
                        x.AddServiceBusMessageScheduler();

                        x.SetKebabCaseEndpointNameFormatter();

                        x.AddConsumer<SubmitOrderConsumer>();
                        x.AddConsumer<OrderSubmittedConsumer>();

                        x.AddSagaStateMachine<OrderShipmentStateMachine, OrderShipmentState, OrderShipmentSagaDefinition>()
                            .MessageSessionRepository();

                        x.UsingAzureServiceBus((context, cfg) =>
                        {
                            cfg.Host(hostContext.Configuration.GetConnectionString("AzureServiceBus"));

                            cfg.UseServiceBusMessageScheduler();

                            cfg.Send<OrderSubmitted>(s => s.UseSessionIdFormatter(c => c.Message.OrderId.ToString("D")));
                            cfg.Send<MonitorOrderShipmentTimeout>(s => s.UseSessionIdFormatter(c => c.Message.OrderId.ToString("D")));

                            // Subscribe to OrderSubmitted directly on the topic, instead of configuring a queue
                            cfg.SubscriptionEndpoint<OrderSubmitted>("order-submitted-consumer", e =>
                            {
                                e.ConfigureConsumer<OrderSubmittedConsumer>(context);
                            });

                            cfg.ConfigureEndpoints(context);
                        });
                    });
                });
        }
    }
}