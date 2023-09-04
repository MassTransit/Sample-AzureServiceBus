namespace Sample.Contracts
{
    using System;


    public record OrderShippedBase
    {
        public Guid OrderId { get; init; }
        public DateTimeOffset Timestamp { get; init; }
    }

    public record OrderShipped : OrderShippedBase
    {
    }
}