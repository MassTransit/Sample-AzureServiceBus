namespace Sample.Worker.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>
    {
        readonly ILogger _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.LogInformation("Order Submission Received: {OrderId}", context.Message.OrderId);

            await context.Publish(new OrderSubmitted
            {
                OrderId = context.Message.OrderId,
                OrderNumber = context.Message.OrderNumber,
                OrderTimestamp = context.Message.Timestamp,
            });

            if (context.IsResponseAccepted<OrderSubmissionAccepted>())
            {
                await context.RespondAsync(new OrderSubmissionAccepted
                {
                    OrderId = context.Message.OrderId,
                    OrderNumber = context.Message.OrderNumber
                });
            }
        }
    }
}