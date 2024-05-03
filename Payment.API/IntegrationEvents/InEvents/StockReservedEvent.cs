using EventBus.Events;
using Payment.API.IntegrationEvents.Messages;

namespace Payment.API.IntegrationEvents.InEvents
{
    public class StockReservedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public PaymentMessage Payment { get; set; }
        public IEnumerable<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
