using System;

namespace Core.Shared.Events
{
    public interface IBaseEvent
    {
        Guid CorrelationId { get; }
        string OrderId { get; }
        int CustomerId { get; }
        int Quantity { get; }
        decimal Amount { get; }
    }
}
