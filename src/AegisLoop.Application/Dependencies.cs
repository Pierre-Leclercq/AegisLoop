using AegisLoop.Application.Interfaces;
using AegisLoop.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AegisLoop.Application;

/// <summary>
/// Enregistrement des services de la couche Application V1.
/// </summary>
public static class Dependencies
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INormalizationService, RawItemNormalizationService>();
        services.AddScoped<IIngestionService, IngestionService>();
        return services;
    }
}