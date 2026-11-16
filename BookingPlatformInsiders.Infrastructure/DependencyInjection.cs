using BookingPlatformInsiders.Application.Bookings;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Application.Rooms;
using BookingPlatformInsiders.Infrastructure.Bookings;
using BookingPlatformInsiders.Infrastructure.Persistence;
using BookingPlatformInsiders.Infrastructure.Rooms;
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
        services.AddHostedService<RoomSeeder>();

        return services;
    }
}
