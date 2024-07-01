using MarketPriceService;
using MarketPriceService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MarketPriceServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFintachartsService, FintachartsService>();
builder.Services.AddScoped<IMarketPriceRepository, MarketPriceRepository>();
builder.Services.AddHostedService<FintachartsSocketService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketPriceServiceDbContext>();
    db.Database.EnsureCreated();

    var assets = await scope.ServiceProvider.GetRequiredService<IFintachartsService>().GetSupportedAssetsAsync();
    var repo = scope.ServiceProvider.GetRequiredService<IMarketPriceRepository>();
    await repo.CreateAssets(assets);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
 
app.Run();