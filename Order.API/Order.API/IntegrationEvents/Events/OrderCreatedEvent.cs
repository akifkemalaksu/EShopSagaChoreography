
using EventBus.Events;
using Order.API.IntegrationEvents.Messages;

namespace Order.API.IntegrationEvents.Events
{
    public class OrderCreatedEvent:Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public PaymentMessage Payment { get; set; }
        public IEnumerable<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
