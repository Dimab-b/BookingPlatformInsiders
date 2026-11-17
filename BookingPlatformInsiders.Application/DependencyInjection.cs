using System.Reflection;
using BookingPlatformInsiders.Application.Auth.Abstractions;
using BookingPlatformInsiders.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookingPlatformInsiders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddOptions<JwtOptions>();
        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();

        return services;
    }
}
