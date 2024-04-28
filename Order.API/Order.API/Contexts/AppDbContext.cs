using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using OrderModel = Order.API.Models.Order;

namespace Order.API.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
