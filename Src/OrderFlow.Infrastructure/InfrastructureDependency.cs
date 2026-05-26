using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Integrations.Apis.PokeApi.Interfaces;
using OrderFlow.Infrastructure.Integrations.Apis.PokeApi;
using OrderFlow.Infrastructure.Policies;

namespace OrderFlow.Infrastructure;

[ExcludeFromCodeCoverage]
public static class InfrastructureDependency
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string pokeApiBaseUrl)
    {
        services
            .AddHttpClient(nameof(PokeApiClient), client =>
            {
                client.BaseAddress = new Uri(pokeApiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddResilienceHandler(
                "orderflow-pokeapi",
                (builder, context) =>
                {
                    var logger = context.ServiceProvider
                        .GetRequiredService<ILogger<PokeApiClient>>();

                    CircuitBreakerPolicy.ConfigureOrderFlowResilienceHandler(builder, logger);
                });

        services.AddScoped<IPokeApiClient, PokeApiClient>();

        return services;
    }
}
