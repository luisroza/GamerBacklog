using GameBacklog.Web.Components;
using GameBacklog.Application;
using GameBacklog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IGameBacklogService, InMemoryGameBacklogService>();
builder.Services.AddScoped<IAccountSessionService, InMemoryAccountSessionService>();
builder.Services.AddSingleton<MockGameMetadataProvider>();
builder.Services.AddSingleton<IgdbMetadataProvider>();
builder.Services.AddSingleton<IGameMetadataProvider, ConfiguredGameMetadataProvider>();
builder.Services.AddSingleton<IGameCatalogProvider, SteamCatalogProvider>();
builder.Services.AddSingleton<IGameCatalogProvider, PlayStation5CatalogProvider>();
builder.Services.AddSingleton<IExternalGameLibraryProvider, SteamLibraryProvider>();
builder.Services.AddSingleton<IExternalGameLibraryProvider, PlayStationLibraryProvider>();

var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error", createScopeForErrors: true); app.UseHsts(); }
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
