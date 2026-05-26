using Moq;
using OrderFlow.Domain.Integrations.Apis.PokeApi;
using OrderFlow.Domain.Integrations.Apis.PokeApi.Interfaces;
using OrderFlow.Domain.Result;

namespace OrderFlow.Infrastructure.Tests.Mocks;

public class PokeApiClientMock
{
    private readonly Mock<IPokeApiClient> _mock = new();

    public PokeApiClientMock SetupGetPokemonByNameAsync(
        string name,
        Result<PokeApiResponse> result)
    {
        _mock
            .Setup(c => c.GetPokemonByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return this;
    }

    public IPokeApiClient Build() => _mock.Object;
}
