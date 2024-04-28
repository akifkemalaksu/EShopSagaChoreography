using Common.Interfaces;
using EventBus.Bus;
using Mapster;
using Order.API.Contexts;
using Order.API.Dtos;
using Order.API.IntegrationEvents.Events;

namespace Order.API.Handlers.CommandHandlers
{
    public class CreateOrderHandler : ICommandHandler<CreateOrderDto, CreateOrderResultDto>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IEventBus _eventBus;

        public CreateOrderHandler(AppDbContext appDbContext, IEventBus eventBus)
        {
            _appDbContext = appDbContext;
            _eventBus = eventBus;
        }

        public async Task<CreateOrderResultDto> Handle(CreateOrderDto command, CancellationToken token)
        {
            var order = command.Adapt<Order.API.Models.Order>();

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

            return new CreateOrderResultDto
            {
                OrderId = order.Id,
            };
        }
    }
}
