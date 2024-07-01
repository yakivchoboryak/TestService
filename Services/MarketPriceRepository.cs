using MarketPriceService;
using MarketPriceService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace TestService.Services
{
    public interface IMarketPriceRepository
    {
        public Task CreateAssets(List<Asset> assets);
        public Task UpdateAssetPriceAsync(Asset asset);
        Task<Asset> GetAssetBySymbolAsync(string symbol);
        Task<List<Asset>> GetAllAssetsAsync();
    }

    public class MarketPriceRepository : IMarketPriceRepository
    {
        private readonly MarketPriceServiceDbContext _context;

        public MarketPriceRepository(MarketPriceServiceDbContext context)
        {
            _context = context;
        }

        public async Task CreateAssets(List<Asset> assets)
        {
            if (assets == null || !assets.Any())
                return;

            var existingAssetIds = await _context.Assets
                .Select(a => a.InstrumentId)
                .ToListAsync();

            var assetsToAdd = assets
                .Where(fetchedAsset => !existingAssetIds.Contains(fetchedAsset.InstrumentId))
                .ToList();

            if (assetsToAdd.Any())
            {
                _context.Assets.AddRange(assetsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAssetPriceAsync(Asset asset)
        {
            var existingAsset = await _context.Assets.FindAsync(asset.InstrumentId);

            if (existingAsset != null)
            {
                existingAsset.Timestamp = asset.Timestamp;
                existingAsset.Price = asset.Price;
                existingAsset.Volume = asset.Volume;
                _context.Assets.Update(existingAsset);
            }
            else
            {
                await _context.Assets.AddAsync(asset);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Asset>> GetAllAssetsAsync()
        {
            return await _context.Assets.ToListAsync();
        }

        public async Task<Asset> GetAssetBySymbolAsync(string symbol)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol);
            return asset;
        }
    }
}
