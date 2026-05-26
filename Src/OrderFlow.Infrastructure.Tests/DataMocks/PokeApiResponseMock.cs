using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OrderFlow.Domain.Integrations.Apis.PokeApi;

namespace OrderFlow.Infrastructure.Tests.DataMocks;

public static class PokeApiResponseMock
{
    public static PokeApiResponse Valid() => new()
    {
        Id = 25,
        Name = "pikachu",
        BaseExperience = 112
    };

    public static HttpResponseMessage HttpSuccess() =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Valid())
        };

    public static HttpResponseMessage HttpNotFound() =>
        new(HttpStatusCode.NotFound);

    public static HttpResponseMessage HttpInternalServerError() =>
        new(HttpStatusCode.InternalServerError);

    public static HttpResponseMessage HttpServiceUnavailable() =>
        new(HttpStatusCode.ServiceUnavailable);
}
