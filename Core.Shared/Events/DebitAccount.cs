﻿using System;

namespace Core.Shared.Events
{
    public class DebitAccount
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; }
        public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
