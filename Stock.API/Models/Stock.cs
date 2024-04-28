using System.ComponentModel.DataAnnotations;

namespace Stock.API.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
