namespace Payment.API.IntegrationEvents.Messages
{
    public class OrderItemMessage
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
