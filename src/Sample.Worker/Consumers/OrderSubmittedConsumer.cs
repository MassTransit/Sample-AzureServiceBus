namespace Sample.Worker.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class OrderSubmittedConsumer :
        IConsumer<OrderSubmitted>
    {
        readonly ILogger _logger;

        public OrderSubmittedConsumer(ILogger<OrderSubmittedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderSubmitted> context)
        {
            _logger.LogInformation("Order Submitted: {OrderId}", context.Message.OrderId);
            return Task.CompletedTask;
        }
    }
}