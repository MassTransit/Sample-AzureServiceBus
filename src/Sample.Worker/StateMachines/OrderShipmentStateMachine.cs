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
            Event(() => OrderShipped, x => x.CorrelateById(context => context.Message.OrderId));

            Schedule(() => MonitorTimeout, x => x.MonitorTimeoutTokenId, x =>
            {
                x.Delay = TimeSpan.FromSeconds(20);
                x.Received = config =>
                {
                    config.ConfigureConsumeTopology = false;
                    config.CorrelateById(context => context.Message.OrderId);
                };
            });

            Initially(
                When(OrderSubmitted)
                    .Then(context => logger.LogInformation("Monitoring Order Shipment: {OrderId}", context.Saga.CorrelationId))
                    .Schedule(MonitorTimeout, context => new MonitorOrderShipmentTimeout { OrderId = context.Saga.CorrelationId })
                    .TransitionTo(WaitingForShipment)
            );

            During(Initial, WaitingForShipment,
                When(MonitorTimeout.Received)
                    .Then(context => logger.LogInformation("Shipment Overdue: {OrderId}", context.Saga.CorrelationId))
                    .TransitionTo(ShipmentOverdue),
                When(OrderShipped)
                    .Then(context => logger.LogInformation("Shipment Completed: {OrderId}", context.Saga.CorrelationId))
                    .Unschedule(MonitorTimeout)
                    .TransitionTo(ShipmentComplete)
            );

            During(ShipmentOverdue,
                Ignore(MonitorTimeout.Received),
                When(OrderShipped)
                    .Then(context => logger.LogInformation("Shipment Completed (overdue): {OrderId}", context.Saga.CorrelationId))
                    .TransitionTo(ShipmentComplete)
            );

            During(ShipmentComplete,
                Ignore(MonitorTimeout.Received),
                When(OrderSubmitted)
                    .Then(context => logger.LogInformation("Order Shipment (already shipped): {OrderId}", context.Saga.CorrelationId))
            );
        }

        //
        // ReSharper disable UnassignedGetOnlyAutoProperty
        public Event<OrderSubmitted> OrderSubmitted { get; }
        public Event<OrderShipped> OrderShipped { get; }

        public Schedule<OrderShipmentState, MonitorOrderShipmentTimeout> MonitorTimeout { get; }

        public State WaitingForShipment { get; }
        public State ShipmentOverdue { get; }
        public State ShipmentComplete { get; }
    }
}