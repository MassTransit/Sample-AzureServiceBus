namespace Sample.Contracts
{
    using System;


    public record MonitorOrderShipmentTimeout
    {
        public Guid OrderId { get; init; }
    }
}