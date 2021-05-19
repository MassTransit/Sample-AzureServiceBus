namespace Sample.Contracts
{
    using System;


    public record SubmitOrder
    {
        public Guid OrderId { get; init; }
        public DateTimeOffset Timestamp { get; init; }

        public string OrderNumber { get; init; }
    }
}