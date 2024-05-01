using Order.API.Dtos;

namespace Order.API.Commands.CreateOrder
{
    public class CreateOrderCommand
    {
        public string BuyerId { get; set; }
        public IEnumerable<OrderItemDto> Items { get; set; }
        public PaymentDto Payment { get; set; }
        public AddressDto Address { get; set; }
    }
}
