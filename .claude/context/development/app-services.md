# App Services

## Visão Geral

App Services é uma camada localizada na API, responsável exclusivamente por orquestrar o fluxo de dados entre a camada de apresentação e a camada de serviços. Ela recebe o objeto de request, mapeia para o modelo esperado pela service, invoca a service e mapeia o resultado de volta para o objeto de response.

Essa camada não contém regras de negócio. Qualquer lógica de negócio pertence à camada de serviços.

---

## Localização

```
[componente]/[componente].Api/AppServices/
[componente]/[componente].Api/AppServices/Interfaces/
```

---

## Responsabilidades

- Receber o objeto de request vindo do controller
- Mapear o request para o modelo de entrada da service
- Invocar a service correspondente
- Mapear o resultado da service para o objeto de response
- Retornar o response ao controller

---

## O que não pertence a essa camada

- Validações de negócio
- Regras condicionais baseadas em estado do domínio
- Acesso direto a repositórios ou banco de dados
- Chamadas a múltiplas services com lógica de composição ou decisão

---

## Estrutura de uma App Service

```csharp
public class OrderAppService(IOrderService orderService, IMapper mapper) : IOrderAppService
{
    public async Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request)
    {
        var result = await orderService.ProcessAsync(mapper.Map<CreateOrderModel>(request), cancellationToken);

        if (result.IsFailure)
            return Results.Problem(result.Message, statusCode: result.StatusCode ?? 500);

        return Results.Ok(mapper.Map<CreateOrderResponse>(result.Value));
    }
}
```

---

## Interfaces

As interfaces das App Services são declaradas dentro da própria API, em `[componente]/[componente].Api/AppServices/Interfaces/`. Elas não são compartilhadas com outras camadas.

```csharp
public interface IOrderAppService
{
    Task<CreateOrderResponse> CreateAsync(CreateOrderRequest request);
}
```

---

## Injeção de Dependência

O registro das App Services está centralizado na classe `[componente]/[componente].Api/ApiDependency.cs`.