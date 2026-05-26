using System.Diagnostics.CodeAnalysis;

namespace OrderFlow.Domain.Models.Pokemon;

[ExcludeFromCodeCoverage]
public class PokemonModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BaseExperience { get; init; }
}
