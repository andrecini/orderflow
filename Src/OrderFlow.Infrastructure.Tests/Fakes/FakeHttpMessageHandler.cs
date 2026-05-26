namespace OrderFlow.Infrastructure.Tests.Fakes;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();
    private int _callCount;

    public int CallCount => _callCount;

    public void EnqueueResponse(HttpResponseMessage response) =>
        _responses.Enqueue(response);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Interlocked.Increment(ref _callCount);

        if (_responses.Count == 0)
            throw new InvalidOperationException("Nenhuma resposta enfileirada no FakeHttpMessageHandler.");

        return Task.FromResult(_responses.Dequeue());
    }
}
