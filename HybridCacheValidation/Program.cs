using HybridCacheValidation.Client.Pages;
using HybridCacheValidation.Components;

var builder = WebApplication.CreateBuilder(args);
var useHybridCache = builder.Configuration.GetValue("Validation:UseHybridCache", true);

// Add services to the container.
builder.Services.AddRazorComponents(options =>
    options.CacheViewSizeLimit = 10 * 1024 * 1024)
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

if (useHybridCache)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("RedisConnectionString");
        options.InstanceName = "cacheview-validation:";
    });
    builder.Services.AddHybridCache(options =>
    {
        options.MaximumPayloadBytes = 1024;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(HybridCacheValidation.Client._Imports).Assembly);

app.Run();
