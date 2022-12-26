using MassTransit;
using System;

namespace Core.Api.Saga
{
    public class OrderState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; }
        public int Quantity { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string CurrentState { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
