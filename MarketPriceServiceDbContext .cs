using Microsoft.EntityFrameworkCore;
using MarketPriceService.Models;
namespace MarketPriceService
{
    public class MarketPriceServiceDbContext : DbContext
    {
        public MarketPriceServiceDbContext(DbContextOptions<MarketPriceServiceDbContext> options) : base(options) { }

        public DbSet<Asset> Assets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .HasKey(a => a.InstrumentId);
            modelBuilder.Entity<Asset>()
                .Property(a => a.Price)
            .HasColumnType("decimal(18, 2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}
