# Logging Standards

## Visão Geral

O logging é feito via `ILogger<T>` nativo do ASP.NET Core, sem sinks externos. Os logs seguem o padrão de **structured logging**, utilizando sempre a sintaxe de template do `ILogger` para que as propriedades sejam indexáveis e consultáveis. Um middleware de `CorrelationId` garante a rastreabilidade de cada request de ponta a ponta.

---

## Localização

```
[componente]/[componente].Api/Middlewares/CorrelationIdMiddleware.cs
```

---

## Níveis de Log

| Nível | Quando usar |
|-------|-------------|
| `Trace` | Detalhes internos de execução — apenas em desenvolvimento |
| `Debug` | Informações de diagnóstico — apenas em desenvolvimento |
| `Information` | Início e fim de operações relevantes, autenticações bem-sucedidas, eventos de negócio importantes |
| `Warning` | Situações inesperadas mas recuperáveis — ex: retry acionado, cache miss, recurso não encontrado |
| `Error` | Exceções não tratadas, falhas em integrações externas, erros que impactam o usuário |
| `Critical` | Falhas que comprometem a disponibilidade da aplicação |

---

## Structured Logging

Sempre utilizar a sintaxe de template do `ILogger` — nunca interpolação de string. Isso garante que as propriedades sejam estruturadas e consultáveis.

```csharp
// Correto
logger.LogInformation("Order {OrderId} created for customer {CustomerId}", order.Id, order.CustomerId);

// Incorreto
logger.LogInformation($"Order {order.Id} created for customer {order.CustomerId}");
```

---

## CorrelationId

Um middleware injeta um `correlationId` único no início de cada request. Ele é incluído no header de resposta e adicionado ao escopo do logger para que todos os logs do request carreguem essa propriedade automaticamente.

```csharp
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Response.Headers[CorrelationIdHeader] = correlationId;
        context.Items[CorrelationIdHeader] = correlationId;

        var logger = context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>();

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}
```

---

## Campos Recomendados por Nível

### Information
```csharp
logger.LogInformation("Request started: {Method} {Path}", context.Request.Method, context.Request.Path);
logger.LogInformation("User {Username} authenticated successfully", username);
logger.LogInformation("Order {OrderId} created successfully", orderId);
```

### Warning
```csharp
logger.LogWarning("Order {OrderId} not found", orderId);
logger.LogWarning("Retry attempt {Attempt} for {Service}", attempt, serviceName);
logger.LogWarning("Cache miss for user {Username}", username);
```

### Error
```csharp
logger.LogError(ex, "Unhandled exception processing request {Method} {Path}", method, path);
logger.LogError(ex, "Failed to call external service {Service}: {Message}", serviceName, ex.Message);
```

### Critical
```csharp
logger.LogCritical(ex, "Database connection lost: {Message}", ex.Message);
```

---

## Convenções

- `ILogger<T>` é sempre injetado via construtor primário — nunca instanciado manualmente
- Nunca logar dados sensíveis — senhas, tokens, dados pessoais (PII) ou informações financeiras
- Exceções são sempre passadas como primeiro argumento do `LogError` e `LogCritical` para preservar o stack trace
- O `CorrelationId` é propagado para chamadas a serviços externos via header `X-Correlation-Id`
- Logs de `Trace` e `Debug` são desabilitados em produção via configuração do `appsettings.Production.json`
- O middleware de `CorrelationId` deve ser registrado antes do middleware de exception handling no pipeline
- O registro do middleware está centralizado em `[componente]/[componente].Api/ApiDependency.cs`