using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using MarketPriceService.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MarketPriceService.Services
{
    public class FintachartsSocketService : IHostedService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ClientWebSocket _webSocket;

        public FintachartsSocketService(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
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

                await ReceiveMessagesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            var buffer = new byte[1024 * 4];

            var subscriptionMessage = new
            {
                type = "l1-subscription",
                id = "1",
                instrumentId = "ad9e5345-4c3b-41fc-9437-1d253f62db52",
                provider = "simulation",
                subscribe = true,
                kinds = new[] { "ask", "bid", "last" }
            };

            // Serialize the object to JSON string
            var messageString = JsonConvert.SerializeObject(subscriptionMessage);

            // Encode the JSON string as bytes
            var messageBytes = Encoding.UTF8.GetBytes(messageString);

            await _webSocket.SendAsync(messageBytes, WebSocketMessageType.Binary, true, token);

            while (_webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", token);
                    break;
                }
                var jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var asset = JsonConvert.DeserializeObject<Asset>(jsonString);
                if (asset != null)
                {
                    //await UpdateAssetPriceAsync(asset!);
                }
            }
        }

        private async Task UpdateAssetPriceAsync(Asset asset)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MarketPriceServiceDbContext>();
            var existingAsset = await context.Assets.FindAsync(asset.Symbol);
            if (existingAsset != null)
            {
                existingAsset.Price = asset.Price;
                existingAsset.LastUpdated = DateTime.UtcNow;
                context.Assets.Update(existingAsset);
            }
            else
            {
                asset.LastUpdated = DateTime.UtcNow;
                await context.Assets.AddAsync(asset);
            }
            await context.SaveChangesAsync();
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

