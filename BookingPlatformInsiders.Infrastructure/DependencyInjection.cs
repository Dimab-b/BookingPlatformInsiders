using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Application.Rooms;
using BookingPlatformInsiders.Infrastructure.Bookings;
using BookingPlatformInsiders.Infrastructure.Bookings.Jobs;
using BookingPlatformInsiders.Infrastructure.Common;
using BookingPlatformInsiders.Infrastructure.Persistence;
using BookingPlatformInsiders.Infrastructure.Rooms;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingPlatformInsiders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddSingleton<IClock, SystemClock>();

        services.AddHostedService<RoomSeeder>();
        AddHangfire(services, configuration, connectionString);

        return services;
    }

    private static void AddHangfire(
        IServiceCollection services,
        IConfiguration configuration,
        string defaultConnectionString)
    {
        var hangfireConnectionString =
            configuration.GetConnectionString("Hangfire") ?? defaultConnectionString;

        var schemaName = configuration["Hangfire:SchemaName"] ?? "hangfire";

        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                bootstrapOptions => bootstrapOptions.UseNpgsqlConnection(hangfireConnectionString),
                new PostgreSqlStorageOptions
                {
                    SchemaName = schemaName,
                    PrepareSchemaIfNecessary = true
                }));

        services.AddHangfireServer();
        services.AddHostedService<RecurringJobsRegistrar>();
    }
}
