using Common.Interfaces;
using EventBus.Bus;
using Mapster;
using Order.API.Contexts;
using Order.API.IntegrationEvents.Messages;
using Order.API.IntegrationEvents.OutEvents;

namespace Order.API.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderCommandResult>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IEventBus _eventBus;

        public CreateOrderCommandHandler(AppDbContext appDbContext, IEventBus eventBus)
        {
            _appDbContext = appDbContext;
            _eventBus = eventBus;
        }

        public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand command, CancellationToken token)
        {
            var order = command.Adapt<Models.Order>();

            order.Status = Constants.OrderStatus.Suspend;

            await _appDbContext.Orders.AddAsync(order, token);

            await _appDbContext.SaveChangesAsync(token);

            var newOrderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = order.Id,
                BuyerId = order.BuyerId,
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid(),
                OrderItems = order.Items.Adapt<List<OrderItemMessage>>(),
                Payment = command.Payment.Adapt<PaymentMessage>()
            };

            newOrderCreatedEvent.Payment.TotalPrice = order.Items.Sum(x => x.Price * x.Count);

            _eventBus.Publish(newOrderCreatedEvent);

            return new CreateOrderCommandResult
            {
                OrderId = order.Id,
            };
        }
    }
}
