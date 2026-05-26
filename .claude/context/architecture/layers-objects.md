# Layer Objects

## Visão Geral

Cada camada da aplicação possui seus próprios tipos de objetos para trafegar dados. Nenhum objeto é compartilhado entre camadas de forma direta — o mapeamento via AutoMapper garante o isolamento entre elas. Usar o tipo errado de objeto em uma camada é uma violação arquitetural.

---

## Objetos por Camada

### Presentation — `[componente].Api`

Utiliza **DTOs de Request e Response**, definidos e exclusivos da camada de apresentação.

| Tipo | Descrição | Localização |
|------|-----------|-------------|
| `[Resource]Request` | Dados recebidos pelo endpoint | `[componente].Api/DTOs/Requests/` |
| `[Resource]Response` | Dados retornados pelo endpoint | `[componente].Api/DTOs/Responses/` |

```csharp
// Request — entrada do endpoint
public class CreateOrderRequest
{
    public Guid CustomerId { get; init; }
    public List<OrderItemRequest> Items { get; init; } = [];
}

// Response — saída do endpoint
public class CreateOrderResponse
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
```

---

### Application — `[componente].Application`

Utiliza **Models**, definidos no Domain e compartilhados entre Application e Domain.

| Tipo | Descrição | Localização |
|------|-----------|-------------|
| `Create[Resource]Model` | Dados de entrada para criação | `[componente].Domain/Models/[Resource]/` |
| `[Resource]Model` | Dados de saída das services | `[componente].Domain/Models/[Resource]/` |

```csharp
// Model de entrada da service
public class CreateOrderModel
{
    public Guid CustomerId { get; init; }
    public List<OrderItemModel> Items { get; init; } = [];
}

// Model de saída da service
public class OrderModel
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
```

---

### Infrastructure — `[componente].Infrastructure`

Utiliza **Entities** para persistência relacional e NoSQL, e **objetos específicos de integração** para comunicação com serviços externos — ambos definidos no Domain.

| Tipo | Descrição | Localização |
|------|-----------|-------------|
| `[Resource]` (Entity) | Objeto persistido no banco de dados | `[componente].Domain/Entities/` |
| `[ApiName]Request/Response` | Contrato de integração com API externa | `[componente].Domain/Integrations/Apis/[ApiName]/` |
| `[ServiceName]Request/Response` | Contrato de integração com serviço AWS | `[componente].Domain/Integrations/Aws/[ServiceName]/` |
| `[TopicName]Message` | Mensagem Kafka | `[componente].Domain/Integrations/Kafka/[TopicName]/` |
| `[QueueName]Message` | Mensagem RabbitMQ | `[componente].Domain/Integrations/RabbitMq/[QueueName]/` |

```csharp
// Entity — persistida via EF Core ou MongoDB Driver
public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}
```

---

## Fluxo de Objetos entre Camadas

```
Request (Presentation)
  → [AutoMapper] → Model (Application/Domain)
    → [AutoMapper] → Entity (Infrastructure)
    ← [AutoMapper] ← Entity (Infrastructure)
  ← [AutoMapper] ← Model (Application/Domain)
← Response (Presentation)
```

---

## Convenções

- DTOs de Request e Response nunca ultrapassam a camada de Presentation
- Models nunca ultrapassam a camada de Application — não são retornados para a Presentation diretamente
- Entities nunca são expostas para camadas superiores à Infrastructure
- Objetos de integração (requests, responses, messages) são exclusivos de cada integração — nunca reutilizados como DTOs internos
- Todo mapeamento entre objetos de camadas diferentes é feito via AutoMapper — nunca manualmente nas classes de negócio
- A ausência de um objeto adequado para uma camada indica que um novo tipo deve ser criado — nunca reutilizar o tipo de outra camada como atalho