# Application Layer — [componente].Application

## Visão Geral

A camada de Application é responsável pela orquestração dos casos de uso da aplicação. Ela implementa os contratos de serviço definidos no Domain, coordenando o fluxo entre as regras de domínio e a infraestrutura. Não contém regras de negócio estruturais — apenas orquestração.

---

## Localização

```
[componente]/[componente].Application/
```

---

## Componentes

### Services
Implementações das interfaces de serviço definidas em `[componente].Domain/Interfaces/Services/`. São responsáveis por orquestrar o fluxo entre repositórios, integrações e regras de domínio, retornando sempre um `Result` ou `Result<T>`. Consulte `result-pattern.md`.

```
[componente]/[componente].Application/Services/
[componente]/[componente].Application/Services/Orders/
[componente]/[componente].Application/Services/Orders/OrderService.cs
```

```csharp
public class OrderService(
    IOrderRepository orderRepository,
    IMapper mapper) : IOrderService
{
    public async Task<Result<OrderModel>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
            return Result<OrderModel>.Failure(ResultCode.NotFound, $"Pedido {id} não encontrado.", statusCode: 404);

        return Result<OrderModel>.Success(mapper.Map<OrderModel>(order));
    }

    public async Task<Result<OrderModel>> CreateAsync(CreateOrderModel model, CancellationToken cancellationToken)
    {
        var order = mapper.Map<Order>(model);

        await orderRepository.AddAsync(order, cancellationToken);

        return Result<OrderModel>.Success(mapper.Map<OrderModel>(order));
    }
}
```

### Dependency Injection
Registro de todos os componentes da camada. Consulte `dependency-injection.md`.

```
[componente]/[componente].Application/ApplicationDependency.cs
```

---

## Estrutura Completa

```
[componente]/[componente].Application/
├── Services/
│   └── [Resource]/
│       └── [Resource]Service.cs
└── ApplicationDependency.cs
```

---

## Convenções

- As services implementam sempre as interfaces definidas em `[componente].Domain/Interfaces/Services/`
- Todas as operações das services retornam `Result` ou `Result<T>` — nunca lançam exceções de negócio
- As services não acessam DTOs da camada de Presentation — consomem e retornam apenas Models do Domain
- O mapeamento entre Models e Entities é feito via AutoMapper com profiles definidos no Domain
- As services não se comunicam entre si diretamente — dependências entre casos de uso devem ser resolvidas via repositórios ou integrações
- A camada de Application referencia apenas o Domain — nunca a Infrastructure ou a Presentation diretamente