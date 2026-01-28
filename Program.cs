var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var app = builder.Build();
app.MapStaticAssets();
app.MapDefaultControllerRoute();
app.Run();
