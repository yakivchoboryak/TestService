using System.ComponentModel.DataAnnotations.Schema;

namespace MarketPriceService.Models
{
    public class Asset
    {
        public string? InstrumentId { get; set; }
        public string? Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }
        public int Volume { get; set; }
    }
}
