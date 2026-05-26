# AutoMapper Profiles

## Visão Geral

O mapeamento entre objetos de camadas diferentes é feito exclusivamente via **AutoMapper**. Cada recurso possui um único profile (`[Resource]Profile`) que centraliza todos os mapeamentos relacionados a ele. Os profiles são distribuídos entre as camadas de **Presentation** e **Domain**, conforme o tipo de objeto mapeado.

---

## Distribuição dos Profiles por Camada

| Camada | Localização | Mapeamentos |
|--------|-------------|-------------|
| Presentation | `[componente]/[componente].Api/Mappings/` | Request → Model, Model → Response |
| Domain | `[componente]/[componente].Domain/Mappings/` | Model → Entity, Entity → Model, Message → Model, IntegrationRequest/Response → Model |

---

## Profiles na Camada de Presentation

Responsáveis pelo mapeamento entre DTOs de Request/Response e Models do Domain.

```
[componente]/[componente].Api/Mappings/
[componente]/[componente].Api/Mappings/OrderProfile.cs
```

```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // Request → Model
        CreateMap<CreateOrderRequest, CreateOrderModel>();
        CreateMap<OrderItemRequest, OrderItemModel>();

        // Model → Response
        CreateMap<OrderModel, CreateOrderResponse>();
    }
}
```

---

## Profiles na Camada de Domain

Responsáveis pelo mapeamento entre Models, Entities e objetos de integração.

```
[componente]/[componente].Domain/Mappings/
[componente]/[componente].Domain/Mappings/OrderProfile.cs
```

```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // Model → Entity
        CreateMap<CreateOrderModel, Order>();

        // Entity → Model
        CreateMap<Order, OrderModel>();

        // Integration Request → Model
        CreateMap<PaymentGatewayResponse, PaymentModel>();

        // Message → Model
        CreateMap<OrderCreatedMessage, OrderCreatedModel>();
    }
}
```

---

## Fluxo Completo de Mapeamento

```
CreateOrderRequest         (Presentation)
  → [Presentation Profile] → CreateOrderModel  (Domain)
    → [Domain Profile]     → Order             (Infrastructure/Entity)
    ← [Domain Profile]     ← Order             (Infrastructure/Entity)
  ← [Domain Profile]       ← OrderModel        (Domain)
← [Presentation Profile]   ← CreateOrderResponse (Presentation)
```

---

## Convenções

- Um profile por recurso em cada camada — ex: `OrderProfile` na Presentation e `OrderProfile` no Domain são arquivos distintos com responsabilidades distintas
- Nenhum mapeamento é feito manualmente fora dos profiles — nunca usar `new DestType { Prop = src.Prop }` entre objetos de camadas diferentes
- Profiles são registrados via `AddAutoMapper(typeof(XDependency).Assembly)` na `XDependency.cs` de cada camada
- Mapeamentos com transformações complexas utilizam `.ForMember()` dentro do profile — nunca lógica de transformação nas classes de negócio
- Mapeamentos entre objetos da mesma camada são permitidos dentro do mesmo profile quando necessário
- Nunca criar um profile em Infrastructure — todos os mapeamentos envolvendo Entities e objetos de integração são responsabilidade do Domain