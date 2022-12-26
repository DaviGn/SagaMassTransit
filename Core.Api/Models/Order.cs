using System;

namespace Core.Api.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
