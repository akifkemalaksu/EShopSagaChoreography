using EventBus.Events;
using Stock.API.IntegrationEvents.Events;

namespace Stock.API.IntegrationEvents.EventHandlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        public Task HandleAsync(OrderCreatedEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
