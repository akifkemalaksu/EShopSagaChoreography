namespace Stock.API.IntegrationEvents.Events
{
    public class OrderItemMessage
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
