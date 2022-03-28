namespace Sample.Contracts
{
    using System;


    public record SubmitOrder
    {
        public Guid OrderId { get; init; }
        public DateTimeOffset Timestamp { get; init; }

        public string OrderNumber { get; init; }
    }

    public record Foo
    {
        public Guid OrderId { get; init; }
    }

    public record FooResponse
    {
        public Guid OrderId { get; init; }
    }
}