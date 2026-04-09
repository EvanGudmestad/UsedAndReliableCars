using UsedAndReliableCars.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC (covers both MVC and Web API controllers)
builder.Services.AddControllersWithViews();

// Register MarketCheckApiService as a typed HttpClient
builder.Services.AddHttpClient<IMarketCheckApiService, MarketCheckApiService>();

var app = builder.Build();

app.UseStaticFiles();   // Serve files from wwwroot
app.UseRouting();

// This is how we route to our MVC controllers, which are decorated with [Controller] and [Action] attributes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//This is how we route to our API controllers, which are decorated with [Route("api/[controller]")]
app.MapControllerRoute(
    name: "api",
    pattern: "api/[controller]");

app.Run();