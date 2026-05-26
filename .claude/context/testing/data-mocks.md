# Data Mocks

## Visão Geral

Data Mocks são classes estáticas que centralizam a criação de objetos de teste reutilizáveis. Elas abstraem a construção de cenários de dados para testes unitários, evitando duplicação e garantindo consistência entre diferentes suítes de teste. Quando a construção de um objeto envolve múltiplas variações, o pattern Builder é aplicado — consulte `builder.md`.

---

## Localização

```
[componente]/[componente].X.Tests/DataMocks/
[componente]/[componente].X.Tests/DataMocks/Requests/
[componente]/[componente].X.Tests/DataMocks/Requests/CreateOrderRequestMock.cs
[componente]/[componente].X.Tests/DataMocks/Models/
[componente]/[componente].X.Tests/DataMocks/Models/CreateOrderModelMock.cs
[componente]/[componente].X.Tests/DataMocks/Responses/
[componente]/[componente].X.Tests/DataMocks/Responses/CreateOrderResponseMock.cs
```

---

## Estrutura de um Data Mock simples

Para objetos com poucos cenários, a classe expõe métodos estáticos nomeados pelo cenário que representam.

```csharp
public static class CreateOrderRequestDataMock
{
    public static CreateOrderRequest Valid() => new()
    {
        CustomerId = Guid.NewGuid(),
        Items =
        [
            new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 2 }
        ]
    };

    public static CreateOrderRequest WithEmptyCustomerId() => new()
    {
        CustomerId = Guid.Empty,
        Items =
        [
            new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 2 }
        ]
    };

    public static CreateOrderRequest WithNoItems() => new()
    {
        CustomerId = Guid.NewGuid(),
        Items = []
    };

    public static CreateOrderRequest WithInvalidItemQuantity() => new()
    {
        CustomerId = Guid.NewGuid(),
        Items =
        [
            new OrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 0 }
        ]
    };
}
```

---

## Estrutura com Builder

Quando o objeto possui muitas variações ou configurações opcionais, o Builder é preferido para evitar proliferação de métodos estáticos.

```csharp
public class CreateOrderRequestDataMockBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private List<OrderItemRequest> _items = [new() { ProductId = Guid.NewGuid(), Quantity = 1 }];

    public CreateOrderRequestDataMockBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public CreateOrderRequestDataMockBuilder WithItems(List<OrderItemRequest> items)
    {
        _items = items;
        return this;
    }

    public CreateOrderRequestDataMockBuilder WithNoItems()
    {
        _items = [];
        return this;
    }

    public CreateOrderRequest Build() => new()
    {
        CustomerId = _customerId,
        Items = _items
    };
}
```

### Uso no teste

```csharp
var request = new CreateOrderRequestDataMockBuilder()
    .WithCustomerId(Guid.Empty)
    .WithNoItems()
    .Build();
```

---

## Convenções

- Um arquivo de Data Mock por tipo de objeto
- Nome do arquivo e da classe seguem o padrão `[NomeDoObjeto]Mock` ou `[NomeDoObjeto]Builder`
- Métodos estáticos de cenário são nomeados de forma descritiva pelo cenário que representam — ex: `Valid()`, `WithEmptyCustomerId()`, `WithNoItems()`
- O método `Valid()` é obrigatório em todo Data Mock e deve representar o objeto em seu estado completamente válido
- Data Mocks não contêm lógica de asserção — apenas construção de objetos
- Data Mocks são organizados por tipo de objeto — Ex: `Requests/`, `Responses/`, `Models/`, `Messages/`, `Documents/`, `Entities`, etc
- Builders são preferidos quando o objeto possui mais de quatro variações de cenário