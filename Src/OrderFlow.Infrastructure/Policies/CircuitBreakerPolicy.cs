using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;

namespace OrderFlow.Infrastructure.Policies;

/// <summary>
/// Central place to define Polly circuit-breaker and retry pipeline configurations
/// shared across infrastructure clients (HTTP, messaging, etc.).
/// </summary>
public static class CircuitBreakerPolicy
{
    /// <summary>
    /// Configura o pipeline de resiliência padrão para chamadas HTTP externas do OrderFlow.
    /// Ordem de execução (de fora para dentro): Fallback → Timeout → Retry → CircuitBreaker.
    /// </summary>
    public static void ConfigureOrderFlowResilienceHandler(
        ResiliencePipelineBuilder<HttpResponseMessage> builder,
        ILogger logger)
    {
        builder
            .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<Exception>()
                    .HandleResult(r => !r.IsSuccessStatusCode),
                FallbackAction = _ =>
                {
                    logger.LogWarning(
                        "Fallback ativado para chamada HTTP. Retornando resposta de fallback (503).");

                    var fallback = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
                    return ValueTask.FromResult(Outcome.FromResult(fallback));
                },
                OnFallback = args =>
                {
                    logger.LogWarning(
                        "Pipeline de resiliência ativou Fallback. Outcome: {Outcome}",
                        args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString());
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(3))
            .AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry tentativa {Attempt} após {Delay}ms. Motivo: {Outcome}",
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString());
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                FailureRatio = 0.5,
                BreakDuration = TimeSpan.FromSeconds(15),
                OnOpened = args =>
                {
                    logger.LogWarning(
                        "Circuit Breaker ABERTO. Break duration: {BreakDuration}s. Motivo: {Outcome}",
                        args.BreakDuration.TotalSeconds,
                        args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString());
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation("Circuit Breaker FECHADO. Serviço recuperado.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = _ =>
                {
                    logger.LogInformation("Circuit Breaker HALF-OPEN. Testando recuperação do serviço.");
                    return ValueTask.CompletedTask;
                }
            });
    }
}
