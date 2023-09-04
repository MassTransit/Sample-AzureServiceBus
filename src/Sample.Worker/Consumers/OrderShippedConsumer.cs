using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace Sample.Worker.Consumers
{
    public class OrderShippedConsumer :
        IConsumer<OrderShipped>
    {
        private readonly ILogger<OrderShippedConsumer> _logger;

        public OrderShippedConsumer(ILogger<OrderShippedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderShipped> context)
        {
            _logger.LogInformation("Order Shipped with OrderId: {OrderId}. SessionId {SessionId}.", 
                context.Message.OrderId, context.SessionId());
            return Task.CompletedTask;
        }
    }
}