using Sample.Worker.Consumers;

namespace Sample.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using MassTransit;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;


    [ApiController]
    [Route("[controller]")]
    public class OrderSubmittedController :
        ControllerBase
    {
        readonly ILogger<OrderSubmittedController> _logger;
        private readonly IPublishEndpoint _client;

        public OrderSubmittedController(ILogger<OrderSubmittedController> logger, IPublishEndpoint client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpPut]
        public async Task<IActionResult> Put(OrderModel order)
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            for (int i = 0; i < 1000; i++)
            {
                var orderId = guids[i%3];
                await _client.Publish(new OrderSubmitted
                {
                    OrderId = orderId,
                    OrderNumber = order.OrderNumber
                }, Pipe.Execute<SendContext>(sendContext =>
                {
                    sendContext.CorrelationId = orderId;
                }));
            }
            await _client.Publish(new OrderSubmitted
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber
            }, Pipe.Execute<SendContext>(sendContext =>
            {
                sendContext.CorrelationId = order.OrderId;
            }));

            return Accepted();
        }
    }
    
}