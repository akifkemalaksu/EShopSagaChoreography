namespace Order.API.Dtos
{
    public class CreateOrderDto
    {
        public string BuyerId { get; set; }
        public IEnumerable<OrderItemDto> Items { get; set; }
        public PaymentDto Payment { get; set; }
        public AddressDto Address { get; set; }
    }
}
