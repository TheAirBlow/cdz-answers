using AspNetCoreRateLimit;
using TheAirBlow.Solver.Library;

new QueueSystem();
var builder = WebApplication.CreateBuilder(args);
SkySmart.Authenticate("REDACTED", 
    "REDACTED");

builder.Services.AddControllersWithViews();
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder
    .Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder
    .Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddMvc();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseIpRateLimiting();

app.Run();
