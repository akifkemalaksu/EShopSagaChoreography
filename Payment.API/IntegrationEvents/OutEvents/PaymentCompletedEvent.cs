using EventBus.Events;

namespace Payment.API.IntegrationEvents.OutEvents
{
    public class PaymentCompletedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
    }
}
