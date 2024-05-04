using EventBus.Events;

namespace Order.API.IntegrationEvents.InEvents
{
    public class PaymentFailedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public string Message { get; set; }
    }
}
