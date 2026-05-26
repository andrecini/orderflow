using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Domain.Result;
using OrderFlow.Infrastructure.Integrations.Apis.PokeApi;
using OrderFlow.Infrastructure.Tests.DataMocks;
using OrderFlow.Infrastructure.Tests.Mocks;
using Shouldly;

namespace OrderFlow.Infrastructure.Tests.Tests.Clients;

public class PokeApiClientTests
{
    private static PokeApiClient BuildClient(HttpClientFactoryMock factoryMock)
    {
        factoryMock.SetupCreateClient(nameof(PokeApiClient));
        return new PokeApiClient(
            factoryMock.Build(),
            NullLogger<PokeApiClient>.Instance);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenApiReturns200_ReturnsSuccessWithDataAsync()
    {
        // Arrange
        var factoryMock = new HttpClientFactoryMock();
        factoryMock.Handler.EnqueueResponse(PokeApiResponseMock.HttpSuccess());
        var client = BuildClient(factoryMock);

        // Act
        var result = await client.GetPokemonByNameAsync("pikachu", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Name.ShouldBe("pikachu");
        result.Value!.Id.ShouldBe(25);
        result.Value!.BaseExperience.ShouldBe(112);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenApiReturns404_ReturnsFailureAsync()
    {
        // Arrange
        var factoryMock = new HttpClientFactoryMock();
        factoryMock.Handler.EnqueueResponse(PokeApiResponseMock.HttpNotFound());
        var client = BuildClient(factoryMock);

        // Act
        var result = await client.GetPokemonByNameAsync("nonexistent", CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.StatusCode.ShouldBe(404);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenApiReturns500_ReturnsFailureWithInternalErrorAsync()
    {
        // Arrange
        var factoryMock = new HttpClientFactoryMock();
        factoryMock.Handler.EnqueueResponse(PokeApiResponseMock.HttpInternalServerError());
        var client = BuildClient(factoryMock);

        // Act
        var result = await client.GetPokemonByNameAsync("pikachu", CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.InternalError);
        result.StatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenApiReturns503_ReturnsFailureAsync()
    {
        // Arrange
        var factoryMock = new HttpClientFactoryMock();
        factoryMock.Handler.EnqueueResponse(PokeApiResponseMock.HttpServiceUnavailable());
        var client = BuildClient(factoryMock);

        // Act
        var result = await client.GetPokemonByNameAsync("pikachu", CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.InternalError);
        result.StatusCode.ShouldBe(503);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenHttpClientThrowsException_ReturnsFailureAsync()
    {
        // Arrange
        var factoryMock = new HttpClientFactoryMock();
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var client = new PokeApiClient(
            new FakeHttpClientFactory(handler, nameof(PokeApiClient)),
            NullLogger<PokeApiClient>.Instance);

        // Act
        var result = await client.GetPokemonByNameAsync("pikachu", CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.InternalError);
        result.StatusCode.ShouldBe(503);
    }

    [Fact]
    public async Task GetPokemonByNameAsync_WhenCancellationRequested_ThrowsOperationCanceledExceptionAsync()
    {
        // Arrange
        var factoryMock = new HttpClientFactoryMock();
        factoryMock.Handler.EnqueueResponse(PokeApiResponseMock.HttpSuccess());
        var client = BuildClient(factoryMock);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(
            () => client.GetPokemonByNameAsync("pikachu", cts.Token));
    }

    [Fact]
    public async Task GetPokemonByNameAsync_NameIsUpperCase_NormalizesToLowerCaseAsync()
    {
        // Arrange
        var factoryMock = new HttpClientFactoryMock();
        factoryMock.Handler.EnqueueResponse(PokeApiResponseMock.HttpSuccess());
        var client = BuildClient(factoryMock);

        // Act
        var result = await client.GetPokemonByNameAsync("PIKACHU", CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }
}

// Helpers internos para testes
internal class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        Task.FromException<HttpResponseMessage>(exception);
}

internal class FakeHttpClientFactory(HttpMessageHandler handler, string clientName) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        if (name != clientName)
            throw new InvalidOperationException($"Cliente '{name}' não esperado.");

        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://fake-api.test")
        };
    }
}
