using EventBus.Events;

namespace Order.API.IntegrationEvents.InEvents
{
    public class PaymentCompletedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
    }
}
