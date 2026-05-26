using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using OrderFlow.Api.DTOs.Responses;
using OrderFlow.Domain.Models.Pokemon;

namespace OrderFlow.Api.Mappings;

[ExcludeFromCodeCoverage]
public class PokemonApiMappingProfile : Profile
{
    public PokemonApiMappingProfile()
    {
        CreateMap<PokemonModel, PokemonResponse>();
    }
}
