using System;

namespace MarketPriceService.Models
{
    public class Asset
    {
        public string? InstrumentId { get; set; }
        public string? Symbol { get; set; }
        public DateTime Timestamp { get; set; }
        public float Price { get; set; }
        public int Volume { get; set; }
    }
}
