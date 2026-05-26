# Filters

## Visão Geral

Os filtros são implementados via `IEndpointFilter` do ASP.NET Core e interceptam o pipeline de execução dos endpoints antes e/ou depois do handler. Eles são usados para comportamentos transversais como validação, logging e tratamento de contexto, evitando duplicação de código entre endpoints.

-----

## Localização

```
[componente]/[componente].Api/Filters/
[componente]/[componente].Api/Filters/ValidationFilter.cs
[componente]/[componente].Api/Filters/LoggingFilter.cs
```

-----

## Estrutura de um Filtro

```csharp
public class ExampleFilter(ILogger<ExampleFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // lógica antes do handler

        var result = await next(context);

        // lógica depois do handler

        return result;
    }
}
```

-----

## Filtros Disponíveis

### ValidationFilter

Intercepta todas as chamadas e valida o objeto de request via FluentValidation antes do handler ser invocado. Em caso de falha, retorna `400 Bad Request` no formato `ProblemDetails`. Consulte `validators.md`.

```csharp
public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();

        if (request is null)
            return Results.BadRequest();

        var result = await validator.ValidateAsync(request);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
```

### LoggingFilter

Registra o início e o fim da execução de cada endpoint, incluindo o `CorrelationId`, método HTTP, rota e tempo de execução. Recomendado para endpoints críticos ou com requisitos de auditoria.

```csharp
public class LoggingFilter(ILogger<LoggingFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;
        var correlationId = context.HttpContext.Items["X-Correlation-Id"];
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "Request started: {Method} {Path} | CorrelationId: {CorrelationId}",
            method, path, correlationId);

        var result = await next(context);

        stopwatch.Stop();

        logger.LogInformation(
            "Request finished: {Method} {Path} | Elapsed: {Elapsed}ms | CorrelationId: {CorrelationId}",
            method, path, stopwatch.ElapsedMilliseconds, correlationId);

        return result;
    }
}
```

-----

## Registro dos Filtros

### Global — todos os endpoints

Filtros globais são registrados na `ApiDependency.cs` e aplicados automaticamente a todos os endpoints.

```csharp
app.MapPost("/api/v1/orders", CreateOrder)
   .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>()
   .WithName("CreateOrder")
   .WithTags("Orders")
   .WithOpenApi();
```

### Por endpoint — aplicação individual

Filtros específicos são encadeados diretamente no endpoint quando aplicáveis apenas a contextos específicos.

```csharp
app.MapGet("/api/v1/orders/{id}", GetOrderById)
   .AddEndpointFilter<LoggingFilter>()
   .WithName("GetOrderById")
   .WithTags("Orders")
   .WithOpenApi();
```

### Encadeamento de filtros

Múltiplos filtros podem ser encadeados — são executados na ordem em que são registrados.

```csharp
app.MapPost("/api/v1/orders", CreateOrder)
   .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>()
   .AddEndpointFilter<LoggingFilter>()
   .WithName("CreateOrder")
   .WithTags("Orders")
   .RequireAuthorization("orders:write")
   .WithOpenApi();
```

-----

## Convenções

- Um filtro por responsabilidade — nunca misturar múltiplas responsabilidades em um único filtro
- Nome do arquivo e da classe seguem o padrão `[Responsabilidade]Filter` — ex: `ValidationFilter`, `LoggingFilter`
- Filtros são registrados via `AddEndpointFilter<T>()` — nunca via middleware para lógica específica de endpoint
- A ordem de registro dos filtros define a ordem de execução — validação sempre antes de logging
- Filtros não contêm regras de negócio — apenas comportamentos transversais de infraestrutura
- O registro dos filtros globais está centralizado em `[componente]/[componente].Api/ApiDependency.cs`