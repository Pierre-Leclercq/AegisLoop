using AegisLoop.Application.Interfaces;
using AegisLoop.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AegisLoop.Application;

public static class Dependencies
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INormalizationService, RawItemNormalizationService>();
        services.AddScoped<IIngestionService, IngestionService>();

        return services;
    }
}