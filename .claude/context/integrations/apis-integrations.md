# API Integrations

## Visão Geral

As integrações com APIs externas seguem os princípios da Clean Architecture. Cada integração possui seus próprios contratos, cliente HTTP e políticas de resiliência, organizados em camadas bem definidas dentro da solução.

---

## Localização

### Contratos (Domain)
```
[componente]/[componente].Domain/Integrations/Apis/[ApiName]/
[componente]/[componente].Domain/Integrations/Apis/[ApiName]/Interfaces/
[componente]/[componente].Domain/Integrations/Apis/[ApiName]/Interfaces/I[ApiName]Client.cs
[componente]/[componente].Domain/Integrations/Apis/[ApiName]/[ApiName]Request.cs
[componente]/[componente].Domain/Integrations/Apis/[ApiName]/[ApiName]Response.cs
```

### Implementação (Infrastructure)
```
[componente]/[componente].Infrastructure/Integrations/Apis/[ApiName]/
[componente]/[componente].Infrastructure/Integrations/Apis/[ApiName]/[ApiName]Client.cs
[componente]/[componente].Infrastructure/Policies/
[componente]/[componente].Infrastructure/Policies/CircuitBreakerPolicy.cs
```

---

## Contrato da Integração

A interface é declarada no `[componente].Domain` e implementada no `[componente].Infrastructure`, seguindo a regra de inversão de dependência da Clean Architecture.

```csharp
public interface IPaymentGatewayClient
{
    Task<PaymentGatewayResponse> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken);
}
```

---

## Implementação do Cliente HTTP

```csharp
public class PaymentGatewayClient(IHttpClientFactory httpClientFactory, IMapper mapper) : IPaymentGatewayClient
{
    public async Task<PaymentGatewayResponse> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(nameof(PaymentGatewayClient));

        var response = await client.PostAsJsonAsync("/payments", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PaymentGatewayResponse>(cancellationToken: cancellationToken);
    }
}
```

---

## Políticas de Resiliência

As políticas de resiliência são definidas em classes centralizadas dentro de `[componente]/[componente].Infrastructure/Policies/` e aplicadas no registro do `HttpClient`. O padrão adotado é **circuit breaker** via Polly.

```csharp
public static class CircuitBreakerPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> Create() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
}
```

---

## Convenções

- Um cliente por API externa
- Nome da classe e interface seguem o padrão `[ApiName]Client` e `I[ApiName]Client`
- Requests e responses da integração nunca são reutilizados como DTOs internos da aplicação — são exclusivos da camada de integração
- O mapeamento entre os modelos internos e os contratos da integração é feito via AutoMapper
- `EnsureSuccessStatusCode` é usado como primeira barreira — o tratamento de erros HTTP é complementado pelo circuit breaker e pelo contexto de tratamento de erros global

---

## Injeção de Dependência

O registro dos clientes HTTP, políticas de resiliência e validators de integração está centralizado em `[componente]/[componente].Infrastructure/InfrastructureDependency.cs`.