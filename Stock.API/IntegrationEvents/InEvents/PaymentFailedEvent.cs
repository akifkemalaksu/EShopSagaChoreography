using EventBus.Events;
using Stock.API.IntegrationEvents.Messages;

namespace Stock.API.IntegrationEvents.InEvents
{
    public class PaymentFailedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public string Message { get; set; }
        public IEnumerable<OrderItemMessage> OrderItems { get; set; } = new List<OrderItemMessage>();
    }
}
