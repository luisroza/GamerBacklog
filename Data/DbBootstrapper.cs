using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Data;

/// <summary>
/// Aplica as migrations e roda o seed automaticamente no boot da aplicação.
/// </summary>
public class DbBootstrapper : IHostedService
{
    private readonly IServiceProvider _services;

    public DbBootstrapper(IServiceProvider services)
    {
        _services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        await DbSeeder.SeedAsync(db, scope.ServiceProvider);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
