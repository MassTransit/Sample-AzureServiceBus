using System;
using System.Threading;

namespace Sample.Worker.Consumers
{
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;


    public class OrderSubmittedConsumer :
        IConsumer<OrderSubmitted>
    {
        public static uint _index;

        readonly ILogger _logger;

        public OrderSubmittedConsumer(ILogger<OrderSubmittedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderSubmitted> context)
        {
            var id= Interlocked.Increment(ref _index);

            _logger.LogInformation("OrderSubmittedConsumer: Started Order Submitted: {OrderId}. DateTime: {now}. Correlation: {correlationId}. ID: {id}"
                , context.Message.OrderId, DateTime.Now.ToString("o"), context.CorrelationId,id);
            Thread.Sleep(1000);
        }
    }
}