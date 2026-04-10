using OpenAI.Chat;
using UsedAndReliableCars.Agents;
using UsedAndReliableCars.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CarGuruAgent>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Typed HttpClient for MarketCheck — matches constructor signature
builder.Services.AddHttpClient<IMarketCheckApiService, MarketCheckApiService>();

var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrEmpty(openAiApiKey))
    throw new Exception("OPENAI_API_KEY environment variable is not set.");

// ChatClient registered directly to match CarGuruAgent constructor
builder.Services.AddSingleton(new ChatClient(model: "gpt-4o", apiKey: openAiApiKey));

builder.Services.AddScoped<CarGuruAgent>();

var app = builder.Build();

app.UseSession();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "api",
    pattern: "api/[controller]");

app.Run();