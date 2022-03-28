namespace Sample.Worker.StateMachines
{
    using System;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class OrderShipmentStateMachine :
        MassTransitStateMachine<OrderShipmentState>
    {
        public OrderShipmentStateMachine(ILogger<OrderShipmentStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderSubmitted, x => x.CorrelateById(context => context.Message.OrderId));

            Request(() => Foo,
                config =>
                {
                    config.Timeout = TimeSpan.FromSeconds(10);
                });

            Initially(
                When(OrderSubmitted)
                    .Request(Foo, c=>c.Init<Foo>(new {c.Message.OrderId})),
                When(Foo.Faulted)
                    .Then(x=>Console.WriteLine("Faulted")),
                When(Foo.TimeoutExpired)
                    .Then(x=>Console.WriteLine("Timeout")),
                When(Foo.Completed)
                    .Then(x =>
                    {
                        Console.WriteLine("Foo Completed");
                    })
            );
        }

        public Request<OrderShipmentState, Foo, FooResponse> Foo { get; }
        
        //
        // ReSharper disable UnassignedGetOnlyAutoProperty
        public Event<OrderSubmitted> OrderSubmitted { get; }

    }
}