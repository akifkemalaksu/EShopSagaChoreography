using Common.Interfaces;
using EventBus.Bus;
using EventBus.Events;
using Microsoft.EntityFrameworkCore;
using Stock.API.Contexts;
using Stock.API.IntegrationEvents.InEvents;
using Stock.API.IntegrationEvents.OutEvents;

namespace Stock.API.IntegrationEvents.EventHandlers
{
    public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(AppDbContext appDbContext, IEventBus eventBus, ILogger<OrderCreatedEventHandler> logger)
        {
            _appDbContext = appDbContext;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task HandleAsync(OrderCreatedEvent @event)
        {
            var stockResult = new List<bool>();

            foreach (var item in @event.OrderItems)
            {
                var isStockExist = _appDbContext.Stocks.Any(x => x.ProductId == item.ProductId && x.Count > item.Count);
                stockResult.Add(isStockExist);
            }

            if (stockResult.All(x => x.Equals(true)))
            {
                foreach (var item in @event.OrderItems)
                {
                    var stock = await _appDbContext.Stocks.FirstOrDefaultAsync(x => x.Id == item.ProductId);

                    if (stock != null)
                        stock.Count -= item.Count;

                    await _appDbContext.SaveChangesAsync();
                }

                _logger.LogInformation("Stock was reserved for BuyerId: {BuyerId}", @event.BuyerId);

                var stockReservedEvent = new StockReservedEvent()
                {
                    BuyerId = @event.BuyerId,
                    CreatedAt = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    OrderId = @event.OrderId,
                    OrderItems = @event.OrderItems,
                    Payment = @event.Payment
                };

                _eventBus.Publish(stockReservedEvent);
            }
            else
            {
                _logger.LogInformation("There is not enough stock. BuyerId: {BuyerId}, OrderId: {OrderId}", @event.BuyerId, @event.OrderId);

                var stockNotReservedEvent = new StockNotReservedEvent()
                {
                    OrderId = @event.OrderId,
                    Message = "There is not enough stock."
                };

                _eventBus.Publish(stockNotReservedEvent);
            }
        }
    }
}
