using AutoMapper;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Integrations.Apis.PokeApi.Interfaces;
using OrderFlow.Domain.Interfaces.Services;
using OrderFlow.Domain.Models.Pokemon;
using OrderFlow.Domain.Result;

namespace OrderFlow.Application.Services.Pokemon;

public class PokemonService(
    IPokeApiClient pokeApiClient,
    IMapper mapper,
    ILogger<PokemonService> logger) : IPokemonService
{
    public async Task<Result<PokemonModel>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        logger.LogInformation("Buscando pokemon {PokemonName}", name);

        var result = await pokeApiClient.GetPokemonByNameAsync(name, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Falha ao buscar pokemon {PokemonName}: {Message}", name, result.Message);
            return Result<PokemonModel>.Failure(result.Code, result.Message!, result.StatusCode);
        }

        if (result.Value is null)
        {
            logger.LogWarning("Pokemon {PokemonName} não encontrado (fallback ativado)", name);
            return Result<PokemonModel>.Failure(ResultCode.NotFound, $"Pokemon '{name}' não encontrado.", 404);
        }

        var model = mapper.Map<PokemonModel>(result.Value);

        logger.LogInformation("Pokemon {PokemonName} retornado com sucesso", name);

        return Result<PokemonModel>.Success(model);
    }
}
