using AegisLoop.Application.Interfaces;
using AegisLoop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AegisLoop.Infrastructure;

/// <summary>
/// Enregistrement des services infrastructure V1.
/// </summary>
public static class Dependencies
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AegisLoopDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAegisLoopStore, EfAegisLoopStore>();
        services.AddScoped<IScoringService, EfScoringService>();
        services.AddScoped<IEventCaseService, EfEventCaseService>();

        return services;
    }
}