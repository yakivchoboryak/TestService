using System;
using System.Diagnostics.Metrics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using MarketPriceService.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TestService.DTO;
using TestService.Services;

namespace MarketPriceService.Services
{
    public class FintachartsSocketService : IHostedService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ClientWebSocket _webSocket;

        public FintachartsSocketService(IConfiguration configuration,
                                        IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _webSocket = new ClientWebSocket();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var webSocketUrl = _configuration["Fintacharts:WebSocketUrl"];
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var fintachartService = scope.ServiceProvider.GetRequiredService<IFintachartsService>();
                string accessToken = await fintachartService.GetAccessTokenAsync();
                webSocketUrl += $"?token={accessToken}";

                await _webSocket.ConnectAsync(new Uri(webSocketUrl), cancellationToken);
                Console.WriteLine("Connected to WebSocket");

                Task.Run(async () => await ReceiveMessagesAsync(cancellationToken));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            var buffer = new byte[1024 * 4];

            await SubscribeAssets(token);

            while (_webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", token);
                    break;
                }
                var jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var assetDto = JsonConvert.DeserializeObject<AssetSocketDTO>(jsonString);
                assetDto.Bid = assetDto.Bid ?? assetDto.Ask ?? assetDto.Last;
                if (assetDto != null && assetDto.Bid!=null && !assetDto.InstrumentId.IsNullOrEmpty())
                {
                    var asset = new Asset
                    {
                        InstrumentId = assetDto.InstrumentId,
                        Timestamp = assetDto.Bid.Timestamp,
                        Price = assetDto.Bid.Price,
                        Volume = assetDto.Bid.Volume
                    };

                    await UpdateAssetPriceAsync(asset);
                }
            }
        }

        private async Task SubscribeAssets(CancellationToken token)
        {
            var assets = await GetAllAssetsAsync();
            int i = 1;
            foreach (var asset in assets)
            {
                var subscriptionMessage = new
                {
                    type = "l1-subscription",
                    id = i.ToString(),
                    instrumentId = asset.InstrumentId,
                    provider = "simulation",
                    subscribe = true,
                    kinds = new[] { "ask", "bid", "last" }
                };
                i++;
                var messageString = JsonConvert.SerializeObject(subscriptionMessage);
                var messageBytes = Encoding.UTF8.GetBytes(messageString);
                await _webSocket.SendAsync(messageBytes, WebSocketMessageType.Binary, true, token);
            }
        }

        private async Task UpdateAssetPriceAsync(Asset asset)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var marketPriceRepository = scope.ServiceProvider.GetRequiredService<IMarketPriceRepository>();
            await marketPriceRepository.UpdateAssetPriceAsync(asset);
        }

        private async Task<List<Asset>> GetAllAssetsAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var marketPriceRepository = scope.ServiceProvider.GetRequiredService<IMarketPriceRepository>();
            return await marketPriceRepository.GetAllAssetsAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _webSocket?.Abort();
            _webSocket?.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _webSocket?.Dispose();
        }
    }
}

