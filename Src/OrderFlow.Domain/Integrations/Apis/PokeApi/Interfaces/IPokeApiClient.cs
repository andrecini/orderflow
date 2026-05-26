using OrderFlow.Domain.Result;

namespace OrderFlow.Domain.Integrations.Apis.PokeApi.Interfaces;

public interface IPokeApiClient
{
    Task<Result<PokeApiResponse>> GetPokemonByNameAsync(string name, CancellationToken cancellationToken);
}
