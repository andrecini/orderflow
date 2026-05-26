# Domain Layer — [componente].Domain

## Visão Geral

A camada de domínio é o núcleo da aplicação. Ela define os contratos, modelos, entidades, enums e regras estruturais que guiam todas as outras camadas. Não possui dependências de outras camadas internas — todas as demais camadas dependem do Domain, nunca o contrário.

---

## Localização

```
[componente]/[componente].Domain/
```

---

## Componentes

### Entities
Representam os objetos de domínio persistidos. Possuem identidade própria e encapsulam estado e comportamento relacionados ao domínio.

```
[componente]/[componente].Domain/Entities/
[componente]/[componente].Domain/Entities/Order.cs
```

### Models
Modelos de transferência de dados utilizados entre a camada de Application e o Domain. Não são entidades persistidas — representam dados de entrada e saída das services.

```
[componente]/[componente].Domain/Models/
[componente]/[componente].Domain/Models/Orders/
[componente]/[componente].Domain/Models/Orders/CreateOrderModel.cs
[componente]/[componente].Domain/Models/Orders/OrderModel.cs
```

### Enums
Enumerações de domínio compartilhadas entre as camadas.

```
[componente]/[componente].Domain/Enums/
[componente]/[componente].Domain/Enums/OrderStatus.cs
```

### Exceptions
Exceções específicas de domínio. Utilizadas para representar violações estruturais que não se enquadram no Result Pattern — ex: estados inválidos que nunca deveriam ocorrer em condições normais.

```
[componente]/[componente].Domain/Exceptions/
[componente]/[componente].Domain/Exceptions/DomainException.cs
```

```csharp
public class DomainException(string message) : Exception(message);
```

### Helpers
Classes utilitárias de suporte a regras e operações recorrentes no domínio.

```
[componente]/[componente].Domain/Helpers/
[componente]/[componente].Domain/Helpers/DateHelper.cs
[componente]/[componente].Domain/Helpers/CurrencyHelper.cs
```

### Extensions
Métodos de extensão que enriquecem tipos existentes com comportamentos reutilizáveis no contexto do domínio.

```
[componente]/[componente].Domain/Extensions/
[componente]/[componente].Domain/Extensions/StringExtensions.cs
[componente]/[componente].Domain/Extensions/DateTimeExtensions.cs
```

```csharp
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
}
```

### Value Objects
Objetos imutáveis que representam conceitos específicos do domínio sem identidade própria — ex: `Money`, `Address`, `DocumentNumber`. Encapsulam validações e comportamentos relacionados ao valor que representam.

```
[componente]/[componente].Domain/ValueObjects/
[componente]/[componente].Domain/ValueObjects/Money.cs
[componente]/[componente].Domain/ValueObjects/Address.cs
```

```csharp
public record Money(decimal Amount, string Currency)
{
    public Money(decimal amount, string currency) : this(amount, currency)
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrEmpty(currency)) throw new DomainException("Currency is required.");
    }
}
```

### Interfaces de Services
Contratos das services de aplicação, implementados na camada de Application e consumidos via injeção de dependência.

```
[componente]/[componente].Domain/Interfaces/
[componente]/[componente].Domain/Interfaces/Services/
[componente]/[componente].Domain/Interfaces/Services/IOrderService.cs
```

### Interfaces de Repositories
Contratos dos repositórios, implementados na camada de Infrastructure. Consulte `generic-repository.md` e `unit-of-work.md`.

```
[componente]/[componente].Domain/Interfaces/Repositories/
[componente]/[componente].Domain/Interfaces/Repositories/IOrderRepository.cs
```

### Result Pattern
Estrutura de retorno padronizada utilizada em todas as camadas. Consulte `result-pattern.md`.

```
[componente]/[componente].Domain/Result/
[componente]/[componente].Domain/Result/Result.cs
[componente]/[componente].Domain/Result/Result{T}.cs
[componente]/[componente].Domain/Result/ResultCode.cs
```

### Integrations
Contratos e DTOs das integrações externas — APIs, AWS, Kafka, RabbitMQ e queries Dapper. Consulte `apis-integrations.md`, `aws-integrations.md`, `kafka-integrations.md` e `rabbit-mq-integrations.md`.

```
[componente]/[componente].Domain/Integrations/
[componente]/[componente].Domain/Integrations/Apis/
│   └── [ApiName]/
│       ├── Interfaces/
│       ├── [ApiName]Request.cs
│       └── [ApiName]Response.cs
[componente]/[componente].Domain/Integrations/Aws/
│   └── [ServiceName]/
│       ├── Interfaces/
│       ├── [ServiceName]Request.cs
│       └── [ServiceName]Response.cs
[componente]/[componente].Domain/Integrations/Kafka/
│   └── [TopicName]/
│       ├── Interfaces/
│       └── [TopicName]Message.cs
[componente]/[componente].Domain/Integrations/RabbitMq/
│   └── [QueueOrExchangeName]/
│       ├── Interfaces/
│       └── [QueueOrExchangeName]Message.cs
[componente]/[componente].Domain/Integrations/Sql/
│   └── [Resource]/
│       └── [Resource]Queries.cs
```

Exemplo de classe de queries Dapper:

```csharp
public static class OrderQueries
{
    public const string GetById = """
        SELECT * FROM orders WHERE id = @Id
        """;

    public const string GetAllPaged = """
        SELECT * FROM orders ORDER BY created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        """;
}
```

### AutoMapper Profiles
Profiles responsáveis pelo mapeamento entre Models e Entities. Ficam no Domain por serem o ponto de encontro natural entre esses dois tipos.

```
[componente]/[componente].Domain/Mappings/
[componente]/[componente].Domain/Mappings/OrderProfile.cs
```

```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<CreateOrderModel, Order>();
        CreateMap<Order, OrderModel>();
    }
}
```

### Dependency Injection
Registro dos componentes da camada. Consulte `dependency-injection.md`.

```
[componente]/[componente].Domain/DomainDependency.cs
```

---

## Estrutura Completa

```
[componente]/[componente].Domain/
├── Entities/
├── Enums/
├── Exceptions/
├── Extensions/
├── Helpers/
├── Integrations/
│   ├── Apis/
│   ├── Aws/
│   ├── Kafka/
│   ├── RabbitMq/
│   └── Sql/
│       └── [Resource]/
├── Interfaces/
│   ├── Repositories/
│   └── Services/
├── Mappings/
├── Models/
│   └── [Resource]/
├── Result/
│   ├── Result.cs
│   ├── Result{T}.cs
│   └── ResultCode.cs
├── ValueObjects/
└── DomainDependency.cs
```

---

## Convenções

- O Domain não referencia nenhuma outra camada interna da solução
- Regras de negócio estruturais pertencem às entidades e value objects — regras de orquestração pertencem às services na camada de Application
- Models são usados para trafegar dados entre Application e Domain — nunca expor entidades diretamente para camadas superiores
- Exceções de domínio são reservadas para estados inválidos estruturais — erros de negócio esperados utilizam o Result Pattern
- Queries Dapper são declaradas como constantes estáticas em classes organizadas por recurso dentro de `Integrations/Sql/`
- Helpers e Extensions são utilitários sem estado — sempre implementados como classes e métodos estáticos
- Value Objects são sempre imutáveis — preferir `record` para sua implementação
- Enums são sempre definidos no Domain e compartilhados com as demais camadas via referência ao projeto
- Os profiles do AutoMapper que mapeiam entre Models e Entities ficam no Domain — profiles que mapeiam entre DTOs e Models ficam na camada de Presentation