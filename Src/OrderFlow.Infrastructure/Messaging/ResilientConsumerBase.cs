namespace OrderFlow.Infrastructure.Messaging;

/// <summary>
/// Base class for resilient message consumers with retry and circuit-breaker support.
/// Concrete consumer implementations inherit from this class.
/// </summary>
public abstract class ResilientConsumerBase
{
    protected abstract Task ConsumeAsync(CancellationToken cancellationToken);
}
