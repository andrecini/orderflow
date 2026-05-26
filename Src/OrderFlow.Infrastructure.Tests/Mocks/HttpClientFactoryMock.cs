using Moq;
using OrderFlow.Infrastructure.Tests.Fakes;

namespace OrderFlow.Infrastructure.Tests.Mocks;

public class HttpClientFactoryMock
{
    private readonly Mock<IHttpClientFactory> _mock = new();
    private readonly FakeHttpMessageHandler _handler = new();

    public FakeHttpMessageHandler Handler => _handler;

    public HttpClientFactoryMock SetupCreateClient(string clientName, string baseUrl = "http://fake-api.test")
    {
        var client = new HttpClient(_handler)
        {
            BaseAddress = new Uri(baseUrl)
        };

        _mock.Setup(f => f.CreateClient(clientName)).Returns(client);
        return this;
    }

    public IHttpClientFactory Build() => _mock.Object;
}
