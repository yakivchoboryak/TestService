using MarketPriceService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TestService.Services;

namespace MarketPriceService.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetsController : ControllerBase
{
    private readonly IMarketPriceRepository _marketPriceRepository;

    public AssetsController(IMarketPriceRepository marketPriceRepository)
    {
        _marketPriceRepository = marketPriceRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Asset>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAssets()
    {
        List<Asset> assets = await _marketPriceRepository.GetAllAssetsAsync();
        return Ok(assets);
    }

    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(Asset),(int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetAssetPrice(string symbol)
    {
            Asset asset = await _marketPriceRepository.GetAssetBySymbolAsync(symbol);
            if (asset == null) return BadRequest($"No asset match for symbol {symbol}");
            return Ok(asset);
    }
}
