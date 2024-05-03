using EventBus.Events;

namespace Payment.API.IntegrationEvents.OutEvents
{
    public class PaymentFailedEvent : Event
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public string Message { get; set; }
    }
}
