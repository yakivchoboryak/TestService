using MarketPriceService.Models;

namespace TestService.Services
{
    public interface IMarketPriceRepository
    {
        public Task CreateAssets(List<Asset> assets);
    }

    public class MarketPriceRepository : IMarketPriceRepository
    {
        public Task CreateAssets(List<Asset> assets)
        {
            throw new NotImplementedException();
        }
    }
}
