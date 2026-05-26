using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Api.AppServices;
using OrderFlow.Api.AppServices.Interfaces;
using OrderFlow.Api.Mappings;
using OrderFlow.Api.Middlewares;

namespace OrderFlow.Api;

[ExcludeFromCodeCoverage]
public static class ApiDependency
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddScoped<IPokemonAppService, PokemonAppService>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<PokemonApiMappingProfile>();
        });

        return services;
    }

    public static IApplicationBuilder UseApiMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        return app;
    }
}
