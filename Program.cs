using UsedAndReliableCars.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC (covers both MVC and Web API controllers)
builder.Services.AddControllersWithViews();

// Register MarketCheckApiService as a typed HttpClient
builder.Services.AddHttpClient<IMarketCheckApiService, MarketCheckApiService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<CarAiAssistant>();

var app = builder.Build();

app.UseStaticFiles();   // Serve files from wwwroot
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();