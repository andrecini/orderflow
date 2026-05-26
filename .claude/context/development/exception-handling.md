# Exception Handling

## Visão Geral

O tratamento de exceções inesperadas é centralizado em um middleware global localizado na camada de apresentação. Ele intercepta qualquer exceção não tratada no pipeline, mapeia para o status code correspondente e retorna uma resposta padronizada no formato `ProblemDetails` (RFC 7807). Exceções de negócio devem ser tratadas via Result Pattern — este middleware é exclusivo para erros inesperados de infraestrutura e sistema.

---

## Localização

```
[componente]/[componente].Api/Middlewares/
[componente]/[componente].Api/Middlewares/ExceptionHandlingMiddleware.cs
```

---

## Mapeamento de Exceções

| Exceção | Status Code | Title |
|---------|-------------|-------|
| `UnauthorizedAccessException` | 401 | Unauthorized |
| `InvalidOperationException` | 422 | Unprocessable Operation |
| `ArgumentException` | 400 | Bad Request |
| `KeyNotFoundException` | 404 | Not Found |
| `TimeoutException` | 408 | Request Timeout |
| `NotImplementedException` | 501 | Not Implemented |
| Qualquer outra | 500 | Internal Server Error |

---

## Implementação

```csharp
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            UnauthorizedAccessException => (401, "Unauthorized"),
            ArgumentException           => (400, "Bad Request"),
            KeyNotFoundException        => (404, "Not Found"),
            InvalidOperationException   => (422, "Unprocessable Operation"),
            TimeoutException            => (408, "Request Timeout"),
            NotImplementedException     => (501, "Not Implemented"),
            _                          => (500, "Internal Server Error")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}
```

---

## Registro no Pipeline

O middleware deve ser registrado no início do pipeline, antes dos demais middlewares, para garantir que todas as exceções sejam interceptadas.

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

O registro está centralizado em `[componente]/[componente].Api/ApiDependency.cs`.

---

## Exemplo de Response

```json
{
  "status": 500,
  "title": "Internal Server Error",
  "detail": "A connection attempt failed because the connected party did not properly respond.",
  "instance": "/api/v1/orders",
  "traceId": "0HN2K1J5QV2PT:00000001",
  "timestamp": "2024-03-15T10:30:00Z"
}
```

---

## Convenções

- Este middleware trata apenas exceções **não tratadas** — erros de negócio são tratados via Result Pattern
- O `detail` expõe a mensagem da exceção — em produção, considerar suprimir detalhes sensíveis via configuração de ambiente
- O `traceId` é derivado do `HttpContext.TraceIdentifier` para correlação com logs
- Toda exceção interceptada é registrada via `ILogger` antes de gerar a resposta
- Novas exceções com mapeamento específico devem ser adicionadas ao `switch` do middleware — nunca tratadas em outros pontos do pipeline