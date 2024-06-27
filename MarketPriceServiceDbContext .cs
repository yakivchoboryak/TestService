using Microsoft.EntityFrameworkCore;
using MarketPriceService.Models;
namespace MarketPriceService
{
    public class MarketPriceServiceDbContext : DbContext
    {
        public MarketPriceServiceDbContext(DbContextOptions<MarketPriceServiceDbContext> options) : base(options) { }

        public DbSet<Asset> Assets { get; set; }
    }
}
