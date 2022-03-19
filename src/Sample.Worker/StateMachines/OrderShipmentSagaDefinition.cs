namespace Sample.Worker.StateMachines
{
    using MassTransit;


    public class OrderShipmentSagaDefinition :
        SagaDefinition<OrderShipmentState>
    {
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderShipmentState> sagaConfigurator)
        {
            if (endpointConfigurator is IServiceBusReceiveEndpointConfigurator sb)
                sb.RequiresSession = true;
        }
    }
}