using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using OrderFlow.Domain.Integrations.Apis.PokeApi;
using OrderFlow.Domain.Integrations.Apis.PokeApi.Interfaces;
using OrderFlow.Domain.Result;

namespace OrderFlow.Infrastructure.Integrations.Apis.PokeApi;

public class PokeApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<PokeApiClient> logger) : IPokeApiClient
{
    private const string ClientName = nameof(PokeApiClient);

    public async Task<Result<PokeApiResponse>> GetPokemonByNameAsync(string name, CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ClientName);

            var response = await client
                .GetAsync($"/api/v2/pokemon/{name.ToLowerInvariant()}", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "PokeAPI retornou status {StatusCode} para pokemon {PokemonName}",
                    (int)response.StatusCode, name);

                return Result<PokeApiResponse>.Failure(
                    ResultCode.InternalError,
                    $"PokeAPI retornou status {(int)response.StatusCode}.",
                    (int)response.StatusCode);
            }

            var pokemonResponse = await response.Content
                .ReadFromJsonAsync<PokeApiResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (pokemonResponse is null)
                return Result<PokeApiResponse>.Failure(
                    ResultCode.InternalError,
                    "Resposta da PokeAPI não pôde ser deserializada.",
                    500);

            return Result<PokeApiResponse>.Success(pokemonResponse);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao chamar PokeAPI para pokemon {PokemonName}", name);

            return Result<PokeApiResponse>.Failure(
                ResultCode.InternalError,
                "Erro ao comunicar com a PokeAPI. Verifique a disponibilidade do serviço.",
                503);
        }
    }
}
