namespace Order.API.Dtos
{
    public class CreateOrderResultDto
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
