using EventBus.Events;

namespace Order.API.IntegrationEvents.InEvents
{
    public class StockNotReservedEvent : Event
    {
        public int OrderId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
