# Messaging Resilience

## Visão Geral

Todos os consumers de mensageria (Kafka e RabbitMQ) adotam um padrão de resiliência baseado em três filas/tópicos e circuit breaker via Polly. A lógica de roteamento e resiliência é encapsulada em uma classe base abstrata `ResilientConsumerBase`, evitando duplicação e garantindo consistência entre os consumers.

---

## Localização

```
[componente]/[componente].Infrastructure/Messaging/
[componente]/[componente].Infrastructure/Messaging/ResilientConsumerBase.cs
[componente]/[componente].Infrastructure/Policies/
[componente]/[componente].Infrastructure/Policies/CircuitBreakerPolicy.cs
```

---

## Padrão de Três Filas/Tópicos

Cada mensagem possui três filas ou tópicos associados:

| Sufixo | Propósito |
|--------|-----------|
| `[nome]-main` | Recebe as mensagens originais |
| `[nome]-retry` | Recebe mensagens que falharam no processamento para nova tentativa |
| `[nome]-dead-letter` | Recebe mensagens que esgotaram as tentativas de retry |

O roteamento entre as filas/tópicos é responsabilidade da classe base e não deve ser reimplementado nos consumers concretos.

---

## Classe Base

```csharp
public abstract class ResilientConsumerBase<TMessage>(IAsyncPolicy resiliencePolicy)
{
    protected abstract Task ProcessAsync(TMessage message, CancellationToken cancellationToken);
    protected abstract Task SendToRetryAsync(TMessage message, CancellationToken cancellationToken);
    protected abstract Task SendToDeadLetterAsync(TMessage message, CancellationToken cancellationToken);

    protected async Task HandleAsync(TMessage message, int attemptCount, CancellationToken cancellationToken)
    {
        try
        {
            await resiliencePolicy.ExecuteAsync(() => ProcessAsync(message, cancellationToken));
        }
        catch (BrokenCircuitException)
        {
            await SendToRetryAsync(message, cancellationToken);
        }
        catch (Exception)
        {
            if (attemptCount >= MaxAttempts)
                await SendToDeadLetterAsync(message, cancellationToken);
            else
                await SendToRetryAsync(message, cancellationToken);
        }
    }

    protected virtual int MaxAttempts => 3;
}
```

---

## Circuit Breaker

```csharp
public static class CircuitBreakerPolicy
{
    public static IAsyncPolicy Create() =>
        Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
}
```

---

## Convenções

- Todo consumer concreto herda de `ResilientConsumerBase<TMessage>`
- O consumer concreto implementa apenas `ProcessAsync`, `SendToRetryAsync` e `SendToDeadLetterAsync`
- O consumer da fila/tópico `[nome]-retry` reutiliza o mesmo consumer concreto do `[nome]-main`, incrementando o `attemptCount`
- O consumer da fila/tópico `[nome]-dead-letter` apenas persiste ou notifica — não reprocessa
- `MaxAttempts` pode ser sobrescrito por consumer quando necessário
- Nenhuma lógica de roteamento entre filas/tópicos deve existir fora da classe base