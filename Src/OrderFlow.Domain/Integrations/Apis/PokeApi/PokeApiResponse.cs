using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace OrderFlow.Domain.Integrations.Apis.PokeApi;

[ExcludeFromCodeCoverage]
public class PokeApiResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("base_experience")]
    public int BaseExperience { get; init; }
}
