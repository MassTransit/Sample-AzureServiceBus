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
    public class OrderController :
        ControllerBase
    {
        readonly IRequestClient<SubmitOrder> _client;
        readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger, IRequestClient<SubmitOrder> client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpPut]
        public async Task<IActionResult> Put(OrderModel order)
        {

            for (int i = 0; i < 100; i++)
            {
                await _client.GetResponse<OrderSubmissionAccepted>(new SubmitOrder
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    Timestamp = DateTimeOffset.Now
                }).ConfigureAwait(false);
            }
            var response = await _client.GetResponse<OrderSubmissionAccepted>(new SubmitOrder
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                Timestamp = DateTimeOffset.Now
            });

            return Accepted(response.Message);
        }
    }


    public record OrderModel
    {
        public Guid OrderId { get; init; }
        public string OrderNumber { get; init; }
    }
}