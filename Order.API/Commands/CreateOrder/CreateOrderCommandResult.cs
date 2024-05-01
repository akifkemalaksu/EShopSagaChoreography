namespace Order.API.Commands.CreateOrder
{
    public class CreateOrderCommandResult
    {
        public int? OrderId { get; set; }
        public bool Status
        {
            get
            {
                return OrderId.HasValue;
            }
        }
        public string Message { get; set; } = string.Empty;
    }
}
