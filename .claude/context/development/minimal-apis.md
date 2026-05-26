# Minimal APIs

## Visão Geral

Os endpoints da aplicação são implementados com Minimal APIs do ASP.NET Core 8, organizados em arquivos separados por recurso dentro do projeto `[componente]/[componente].Api`. Essa abordagem substitui os controllers MVC em todos os módulos novos da aplicação.

---

## Localização

```
[componente]/[componente].Api/Endpoints/
[componente]/[componente].Api/Endpoints/Orders/
[componente]/[componente].Api/Endpoints/Orders/CreateOrderEndpoint.cs
[componente]/[componente].Api/Endpoints/Orders/GetOrderByIdEndpoint.cs
```

---

## Padrão de Organização

Cada endpoint é representado por uma classe estática com um método de extensão responsável por mapear a rota. Os endpoints são agrupados em pastas por recurso.

```csharp
public static class CreateOrderEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/orders", async (
            CreateOrderRequest request,
            IOrderAppService appService,
            CancellationToken cancellationToken) =>
        {
            var response = await appService.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/v1/orders/{response.OrderId}", response);
        })
        .WithName("CreateOrder")
        .WithSummary("Cria um novo pedido")
        .WithTags("Orders")
        .RequireAuthorization("orders:write")
        .WithOpenApi();
    }
}
```

---

## Convenções

- Um arquivo por endpoint
- Nome do arquivo e da classe seguem o padrão `[Ação][Recurso]Endpoint` — ex: `CreateOrderEndpoint`, `GetOrderByIdEndpoint`
- Rotas em `kebab-case` com versionamento obrigatório: `/api/v1/`
- Sempre declarar `.WithName()`, `.WithSummary()` e `.WithTags()` para suporte ao Swagger
- Endpoints autenticados devem declarar `.RequireAuthorization("policy-name")` seguindo o padrão `[recurso]:[ação]` — ex: `orders:read`, `orders:write`
- Validação de request é tratada via FluentValidation — há um arquivo de contexto específico para isso

---

## Retornos Padrão

Os endpoints utilizam `TypedResults` para retorno, por tornar o tipo de retorno explícito e melhorar a inferência do Swagger.

```csharp
return TypedResults.Ok(response);
return TypedResults.Created($"/api/v1/orders/{response.OrderId}", response);
return TypedResults.NoContent();
return TypedResults.NotFound();
```

---

## Registro dos Endpoints

O registro de todos os endpoints no pipeline da aplicação está centralizado em `[componente]/[componente].Api/ApiDependency.cs`.