using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Api.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class PokemonResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BaseExperience { get; init; }
}
