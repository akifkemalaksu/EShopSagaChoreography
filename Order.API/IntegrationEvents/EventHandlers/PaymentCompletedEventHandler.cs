using EventBus.Events;
using Microsoft.EntityFrameworkCore;
using Order.API.Contexts;
using Order.API.IntegrationEvents.InEvents;

namespace Order.API.IntegrationEvents.EventHandlers
{
    public class PaymentCompletedEventHandler : IEventHandler<PaymentCompletedEvent>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<PaymentCompletedEventHandler> _logger;

        public PaymentCompletedEventHandler(AppDbContext dbContext, ILogger<PaymentCompletedEventHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task HandleAsync(PaymentCompletedEvent @event)
        {
            var order = await _dbContext.Orders.FindAsync(@event.OrderId);

            if (order != null)
            {
                order.Status = Constants.OrderStatus.Completed;
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
