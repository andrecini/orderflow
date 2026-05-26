using OrderFlow.Domain.Models.Pokemon;
using OrderFlow.Domain.Result;

namespace OrderFlow.Domain.Interfaces.Services;

public interface IPokemonService
{
    Task<Result<PokemonModel>> GetByNameAsync(string name, CancellationToken cancellationToken);
}
