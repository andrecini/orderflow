using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Application.Services.Pokemon;
using OrderFlow.Domain.Integrations.Apis.PokeApi;
using OrderFlow.Domain.Mappings;
using OrderFlow.Domain.Result;
using OrderFlow.Infrastructure.Tests.DataMocks;
using OrderFlow.Infrastructure.Tests.Mocks;
using Shouldly;

namespace OrderFlow.Infrastructure.Tests.Tests.Services;

public class PokemonServiceTests
{
    private readonly IMapper _mapper;

    public PokemonServiceTests()
    {
        _mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PokemonMappingProfile>();
        }).CreateMapper();
    }

    [Fact]
    public async Task GetByNameAsync_WhenClientReturnsSuccess_ReturnsMappedModelAsync()
    {
        // Arrange
        var apiResponse = PokeApiResponseMock.Valid();
        var clientResult = Result<PokeApiResponse>.Success(apiResponse);

        var clientMock = new PokeApiClientMock()
            .SetupGetPokemonByNameAsync("pikachu", clientResult)
            .Build();

        var service = new PokemonService(clientMock, _mapper, NullLogger<PokemonService>.Instance);

        // Act
        var result = await service.GetByNameAsync("pikachu", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Name.ShouldBe("pikachu");
        result.Value!.Id.ShouldBe(25);
        result.Value!.BaseExperience.ShouldBe(112);
    }

    [Fact]
    public async Task GetByNameAsync_WhenClientReturnsFailure_PropagatesFailureAsync()
    {
        // Arrange
        var clientResult = Result<PokeApiResponse>.Failure(
            ResultCode.InternalError,
            "Erro ao comunicar com a PokeAPI.",
            503);

        var clientMock = new PokeApiClientMock()
            .SetupGetPokemonByNameAsync("pikachu", clientResult)
            .Build();

        var service = new PokemonService(clientMock, _mapper, NullLogger<PokemonService>.Instance);

        // Act
        var result = await service.GetByNameAsync("pikachu", CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.InternalError);
        result.StatusCode.ShouldBe(503);
        result.Message.ShouldBe("Erro ao comunicar com a PokeAPI.");
    }

    [Fact]
    public async Task GetByNameAsync_WhenClientReturnsNullValue_ReturnsNotFoundAsync()
    {
        // Arrange
        var clientResult = Result<PokeApiResponse>.Success(null!);

        var clientMock = new PokeApiClientMock()
            .SetupGetPokemonByNameAsync("unknown", clientResult)
            .Build();

        var service = new PokemonService(clientMock, _mapper, NullLogger<PokemonService>.Instance);

        // Act
        var result = await service.GetByNameAsync("unknown", CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.NotFound);
        result.StatusCode.ShouldBe(404);
    }

    [Fact]
    public async Task GetByNameAsync_WhenClientReturnsNotFound_ReturnsNotFoundResultAsync()
    {
        // Arrange
        var clientResult = Result<PokeApiResponse>.Failure(
            ResultCode.NotFound,
            "Pokemon não encontrado.",
            404);

        var clientMock = new PokeApiClientMock()
            .SetupGetPokemonByNameAsync("notexist", clientResult)
            .Build();

        var service = new PokemonService(clientMock, _mapper, NullLogger<PokemonService>.Instance);

        // Act
        var result = await service.GetByNameAsync("notexist", CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.NotFound);
        result.StatusCode.ShouldBe(404);
    }

    [Fact]
    public async Task GetByNameAsync_WhenCancellationTokenPropagated_PassesTokenToClientAsync()
    {
        // Arrange
        var apiResponse = PokeApiResponseMock.Valid();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        var clientResult = Result<PokeApiResponse>.Success(apiResponse);

        var clientMock = new PokeApiClientMock()
            .SetupGetPokemonByNameAsync("pikachu", clientResult)
            .Build();

        var service = new PokemonService(clientMock, _mapper, NullLogger<PokemonService>.Instance);

        // Act
        var result = await service.GetByNameAsync("pikachu", token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}
