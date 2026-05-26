using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using OrderFlow.Domain.Integrations.Apis.PokeApi;
using OrderFlow.Domain.Models.Pokemon;

namespace OrderFlow.Domain.Mappings;

[ExcludeFromCodeCoverage]
public class PokemonMappingProfile : Profile
{
    public PokemonMappingProfile()
    {
        CreateMap<PokeApiResponse, PokemonModel>();
    }
}
