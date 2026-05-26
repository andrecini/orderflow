using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Services.Pokemon;
using OrderFlow.Domain.Interfaces.Services;

namespace OrderFlow.Application;

[ExcludeFromCodeCoverage]
public static class ApplicationDependency
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPokemonService, PokemonService>();

        return services;
    }
}
