﻿using EventBus.Events;
using Payment.API.IntegrationEvents.Messages;

namespace Payment.API.IntegrationEvents.OutEvents
{
    public class PaymentFailedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public string Message { get; set; }
        public IEnumerable<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
