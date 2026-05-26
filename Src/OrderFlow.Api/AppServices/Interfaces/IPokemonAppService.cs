namespace OrderFlow.Api.AppServices.Interfaces;

public interface IPokemonAppService
{
    Task<IResult> GetByNameAsync(string name, CancellationToken cancellationToken);
}
