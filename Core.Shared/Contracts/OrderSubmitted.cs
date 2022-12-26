namespace Core.Shared.Contracts
{
    public class OrderSubmitted
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
