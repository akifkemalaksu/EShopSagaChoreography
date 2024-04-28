using Microsoft.EntityFrameworkCore;


namespace Stock.API.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Stock.API.Models.Stock> Stocks { get; set; }
    }
}
