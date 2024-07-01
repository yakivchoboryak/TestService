namespace TestService.DTO
{
    public class BidDTO
    {
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }
        public int Volume { get; set; }
    }

    public class AssetSocketDTO
    {
        public string InstrumentId { get; set; }
        public BidDTO Bid { get; set; }
        public BidDTO Last { get; set; }
        public BidDTO Ask { get; set; }
    }
}
