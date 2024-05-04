using EventBus.Events;
using Order.API.Contexts;
using Order.API.IntegrationEvents.InEvents;

namespace Order.API.IntegrationEvents.EventHandlers
{
    public class StockNotReservedEventHandler : IEventHandler<StockNotReservedEvent>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<StockNotReservedEventHandler> _logger;

        public StockNotReservedEventHandler(AppDbContext dbContext, ILogger<StockNotReservedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task HandleAsync(StockNotReservedEvent @event)
        {
            var order = await _dbContext.Orders.FindAsync(@event.OrderId);

            if (order != null)
            {
                order.Status = Constants.OrderStatus.Failed;
                order.FailMessage = @event.Message;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Order with {order.Id} id status changed. Status: {order.Status}");
            }
            else
            {
                _logger.LogError($"Order with {@event.OrderId} id not found.");
            }
        }
    }
}
