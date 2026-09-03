using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Integrations;
using GamerBacklog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/Login";
    options.Cookie.Name = "GamerBacklog.Auth";
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("rawg", client =>
{
    client.BaseAddress = new Uri("https://api.rawg.io/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Integrações externas atrás de interfaces
// Catálogo: RAWG (gratuita, com notas do Metacritic) quando houver chave; senão catálogo demo embutido
builder.Services.AddScoped<RawgClient>(); // registrado sempre: usado para sincronizar o catálogo/novas notas
if (!string.IsNullOrWhiteSpace(builder.Configuration["Rawg:ApiKey"]))
{
    builder.Services.AddScoped<ICatalogClient, RawgClient>();
}
else
{
    builder.Services.AddScoped<ICatalogClient, DemoIgdbClient>();
}
builder.Services.AddScoped<DemoIgdbClient>(); // fallback do seeder quando a RAWG falha
builder.Services.AddScoped<ISteamClient, SteamClient>();
builder.Services.AddScoped<IPsnClient, PsnClient>();
builder.Services.AddScoped<IXboxClient, XboxClient>();

// Serviços de aplicação
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Aplica migrations + seed no boot
builder.Services.AddHostedService<DbBootstrapper>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
