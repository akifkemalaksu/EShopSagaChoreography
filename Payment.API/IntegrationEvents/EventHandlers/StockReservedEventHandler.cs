using EventBus.Bus;
using EventBus.Events;
using Payment.API.IntegrationEvents.InEvents;
using Payment.API.IntegrationEvents.OutEvents;

namespace Payment.API.IntegrationEvents.EventHandlers
{
    public class StockReservedEventHandler : IEventHandler<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventHandler> _logger;
        private readonly IEventBus _eventBus;

        public StockReservedEventHandler(ILogger<StockReservedEventHandler> logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;
        }

        public Task HandleAsync(StockReservedEvent @event)
        {
            var balance = 3000m;

            if (balance > @event.Payment.TotalPrice)
            {
                _logger.LogInformation($"{@event.Payment.TotalPrice} TL was withdrawn from credit card for user id: {@event.BuyerId}");

                var paymentSucceedEvent = new PaymentCompletedEvent
                {
                    BuyerId = @event.BuyerId,
                    OrderId = @event.OrderId,
                };

                _eventBus.Publish(paymentSucceedEvent);
            }
            else
            {
                _logger.LogInformation($"{@event.Payment.TotalPrice} TL was not withdrawn from credit card for user id: {@event.BuyerId}");

                var paymentFailedEvent = new PaymentFailedEvent
                {
                    BuyerId = @event.BuyerId,
                    OrderId = @event.OrderId,
                    Message = "Not enough balance.",
                    OrderItems = @event.OrderItems
                };

                _eventBus.Publish(paymentFailedEvent);
            }

            return Task.CompletedTask;
        }
    }
}
