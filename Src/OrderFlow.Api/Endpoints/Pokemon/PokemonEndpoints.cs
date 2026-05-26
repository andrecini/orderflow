using System.Diagnostics.CodeAnalysis;
using OrderFlow.Api.AppServices.Interfaces;

namespace OrderFlow.Api.Endpoints.Pokemon;

[ExcludeFromCodeCoverage]
public static class PokemonEndpoints
{
    public static IEndpointRouteBuilder MapPokemonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/pokemon")
            .WithTags("Pokemon");

        group.MapGet("/{name}", GetByNameAsync)
            .WithName("GetPokemonByName")
            .WithSummary("Busca um pokemon pelo nome via PokeAPI com resiliência Polly v8")
            .Produces<OrderFlow.Api.DTOs.Responses.PokemonResponse>(200)
            .ProducesProblem(404)
            .ProducesProblem(503);

        return app;
    }

    private static async Task<IResult> GetByNameAsync(
        string name,
        IPokemonAppService pokemonAppService,
        CancellationToken cancellationToken)
    {
        return await pokemonAppService.GetByNameAsync(name, cancellationToken);
    }
}
