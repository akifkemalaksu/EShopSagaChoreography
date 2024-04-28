using EventBus.Events;

namespace Stock.API.IntegrationEvents.OutEvents
{
    public class StockNotReservedEvent : Event
    {
        public int OrderId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
