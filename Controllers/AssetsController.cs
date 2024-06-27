using MarketPriceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketPriceService.Controllers;

[ApiController]
[Route("api/assets")]
    public class AssetsController : ControllerBase
    {
        private readonly IFintachartsService _fintachartsService;

        public AssetsController(IFintachartsService fintachartsService)
        {
            _fintachartsService = fintachartsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _fintachartsService.GetSupportedAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> GetAssetPrice(string symbol)
        {
            var asset = await _fintachartsService.GetAssetPriceAsync(symbol);
            return Ok(asset);
        }
    }
