namespace Core.Shared.Contracts
{
    public class RollbackDebit
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
