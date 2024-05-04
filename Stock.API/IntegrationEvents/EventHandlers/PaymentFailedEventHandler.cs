using EventBus.Events;
using Microsoft.EntityFrameworkCore;
using Stock.API.Contexts;
using Stock.API.IntegrationEvents.InEvents;

namespace Stock.API.IntegrationEvents.EventHandlers
{
    public class PaymentFailedEventHandler : IEventHandler<PaymentFailedEvent>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<PaymentFailedEventHandler> _logger;

        public PaymentFailedEventHandler(AppDbContext dbContext, ILogger<PaymentFailedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task HandleAsync(PaymentFailedEvent @event)
        {
            foreach (var item in @event.OrderItems)
            {
                var stock = await _dbContext.Stocks.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                if (stock != null)
                {
                    stock.Count += item.Count;
                    await _dbContext.SaveChangesAsync();
                }
            }

            _logger.LogInformation($"Stock was released for Order Id: {@event.OrderId}");
        }
    }
}
