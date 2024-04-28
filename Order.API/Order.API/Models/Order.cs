using System.ComponentModel.DataAnnotations;
using Order.API.Constants;

namespace Order.API.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BuyerId { get; set; } = null!;
        public Address Address { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public OrderStatus Status { get; set; }
        public string FailMessage { get; set; } = string.Empty;
    }
}
