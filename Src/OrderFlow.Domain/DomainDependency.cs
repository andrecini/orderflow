using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Domain.Mappings;

namespace OrderFlow.Domain;

[ExcludeFromCodeCoverage]
public static class DomainDependency
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<PokemonMappingProfile>();
        });

        return services;
    }
}
