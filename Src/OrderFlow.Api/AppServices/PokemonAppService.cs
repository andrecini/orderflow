using AutoMapper;
using OrderFlow.Api.AppServices.Interfaces;
using OrderFlow.Api.DTOs.Responses;
using OrderFlow.Domain.Interfaces.Services;

namespace OrderFlow.Api.AppServices;

public class PokemonAppService(
    IPokemonService pokemonService,
    IMapper mapper) : IPokemonAppService
{
    public async Task<IResult> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var result = await pokemonService.GetByNameAsync(name, cancellationToken);

        if (result.IsFailure)
            return TypedResults.Problem(
                detail: result.Message,
                statusCode: result.StatusCode ?? 500);

        var response = mapper.Map<PokemonResponse>(result.Value);

        return TypedResults.Ok(response);
    }
}
