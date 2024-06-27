using MarketPriceService.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using TestService.DTO;

namespace MarketPriceService.Services
{
    public interface IFintachartsService
    {
        Task<List<Asset>> GetSupportedAssetsAsync();
        Task<string> GetAccessTokenAsync();
    }

    public class FintachartsService : IFintachartsService
    {
        private readonly IConfiguration _configuration;
        private readonly MarketPriceServiceDbContext _context;

        public FintachartsService(IConfiguration configuration, MarketPriceServiceDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            using HttpClient client = new();

            var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", _configuration["Fintacharts:Credentials:grant_type"]!),
            new KeyValuePair<string, string>("client_id", _configuration["Fintacharts:Credentials:client_id"]!),
            new KeyValuePair<string, string>("username", _configuration["Fintacharts:Credentials:username"]!),
            new KeyValuePair<string, string>("password", _configuration["Fintacharts:Credentials:password"]!)
        });

            var response = await client.PostAsync(_configuration["Fintacharts:Uri"] + _configuration["Fintacharts:GetTokenUrl"], content);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

            return tokenResponse!.AccessToken;
        }

        public async Task<List<Asset>> GetSupportedAssetsAsync()
        {
            using HttpClient client = new();
            List<Asset> fetchedAssets = new();

            string requesturl = _configuration["Fintacharts:Uri"] + _configuration["Fintacharts:AssetsListUrl"];
            int pageIndex = 1;
            int pagesToFetch = 1;

            while (pageIndex < pagesToFetch)
            {
                HttpResponseMessage response = await client.GetAsync(requesturl + $"?page={pageIndex}");

                if (!response.IsSuccessStatusCode)
                {
                    // Handle unsuccessful API call
                    throw new HttpRequestException($"Failed to retrieve assets: {response.StatusCode}");
                }
                var jsonString = await response.Content.ReadAsStringAsync();
                var assetsResponse = JsonConvert.DeserializeObject<AssetsResponse>(jsonString);
                pagesToFetch = assetsResponse.Paging.Pages;
                pageIndex++;
                var assets = assetsResponse.Data.Select(apiAsset => new Asset
                {
                    InstrumentId = apiAsset.Id,
                    Symbol = apiAsset.Symbol,
                });
                fetchedAssets.AddRange(assets);

            }
            return fetchedAssets;
        }
    }
}