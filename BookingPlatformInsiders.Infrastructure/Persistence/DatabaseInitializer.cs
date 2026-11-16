using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingPlatformInsiders.Infrastructure.Persistence;

internal sealed class DatabaseInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DatabaseInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue("Database:MigrateOnStartup", false))
        {
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Database migrations applied on startup.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
