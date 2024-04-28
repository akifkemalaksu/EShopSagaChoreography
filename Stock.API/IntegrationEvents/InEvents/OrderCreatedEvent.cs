using EventBus.Events;
using Stock.API.IntegrationEvents.Messages;

namespace Stock.API.IntegrationEvents.InEvents
{
    public class OrderCreatedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public PaymentMessage Payment { get; set; }
        public IEnumerable<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
