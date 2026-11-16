using BookingPlatformInsiders.Domain.Rooms;
using BookingPlatformInsiders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BookingPlatformInsiders.Infrastructure.Rooms;

internal sealed class RoomSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RoomSeeder> _logger;

    public RoomSeeder(IServiceScopeFactory scopeFactory, ILogger<RoomSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var added = 0;

        foreach (var seed in RoomSeedData.Rooms)
        {
            var name = new RoomName(seed.Name);

            var exists = await context.Rooms
                .AnyAsync(room => room.Name == name, cancellationToken);

            if (exists)
            {
                continue;
            }

            var room = Room.Create(name, seed.Capacity, seed.Location, seed.Description);

            await context.Rooms.AddAsync(room, cancellationToken);
            added++;
        }

        if (added == 0)
        {
            _logger.LogInformation("Room seeding finished. Added: 0.");
            return;
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Room seeding finished. Added: {Added}.", added);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            _logger.LogWarning("Room seeding skipped: rooms were created by another instance.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
