# Solution Architecture

## Visão Geral

A solução segue os princípios da **Clean Architecture**, organizada em camadas com dependências unidirecionais. Cada camada possui responsabilidades bem definidas e se comunica com as camadas adjacentes exclusivamente via abstrações. A stack principal é **.NET 8** com **ASP.NET Core**.

---

## Estrutura da Solution

```
[componente]/
├── 0 - Presentation/
│   └── [componente].Api/
│       ├── AppServices/
│       │   └── Interfaces/
│       ├── DTOs/
│       │   ├── Requests/
│       │   └── Responses/
│       ├── Endpoints/
│       │   └── [Resource]/
│       ├── Filters/
│       ├── Mappings/
│       ├── Middlewares/
│       ├── Validators/
│       │   └── [Resource]/
│       └── ApiDependency.cs
│
├── 1 - Application/
│   └── [componente].Application/
│       ├── Services/
│       │   └── [Resource]/
│       └── ApplicationDependency.cs
│
├── 2 - Domain/
│   └── [componente].Domain/
│       ├── Entities/
│       ├── Enums/
│       ├── Exceptions/
│       ├── Extensions/
│       ├── Helpers/
│       ├── Integrations/
│       │   ├── Apis/
│       │   │   └── [ApiName]/
│       │   │       ├── Interfaces/
│       │   │       ├── [ApiName]Request.cs
│       │   │       └── [ApiName]Response.cs
│       │   ├── Aws/
│       │   │   └── [ServiceName]/
│       │   │       ├── Interfaces/
│       │   │       ├── [ServiceName]Request.cs
│       │   │       └── [ServiceName]Response.cs
│       │   ├── Kafka/
│       │   │   └── [TopicName]/
│       │   │       ├── Interfaces/
│       │   │       └── [TopicName]Message.cs
│       │   ├── RabbitMq/
│       │   │   └── [QueueOrExchangeName]/
│       │   │       ├── Interfaces/
│       │   │       └── [QueueOrExchangeName]Message.cs
│       │   └── Sql/
│       │       └── [Resource]/
│       │           └── [Resource]Queries.cs
│       ├── Interfaces/
│       │   ├── Repositories/
│       │   └── Services/
│       ├── Mappings/
│       ├── Models/
│       │   └── [Resource]/
│       ├── Result/
│       │   ├── Result.cs
│       │   ├── Result{T}.cs
│       │   └── ResultCode.cs
│       ├── ValueObjects/
│       └── DomainDependency.cs
│
├── 3 - Infrastructure/
│   └── [componente].Infrastructure/
│       ├── Data/
│       │   ├── Configurations/
│       │   │   └── [Resource]Configuration.cs
│       │   └── AppDbContext.cs
│       ├── Integrations/
│       │   ├── Apis/
│       │   │   └── [ApiName]/
│       │   │       └── [ApiName]Client.cs
│       │   ├── Aws/
│       │   │   └── [ServiceName]/
│       │   │       └── [ServiceName]Client.cs
│       │   ├── Kafka/
│       │   │   └── [TopicName]/
│       │   │       ├── [TopicName]Producer.cs
│       │   │       └── [TopicName]Consumer.cs
│       │   └── RabbitMq/
│       │       └── [QueueOrExchangeName]/
│       │           ├── [QueueOrExchangeName]Producer.cs
│       │           └── [QueueOrExchangeName]Consumer.cs
│       ├── Messaging/
│       │   └── ResilientConsumerBase.cs
│       ├── Policies/
│       │   └── CircuitBreakerPolicy.cs
│       ├── Repositories/
│       │   └── [Resource]/
│       └── InfrastructureDependency.cs
│
└── Tests/
    ├── 0 - Presentation/
    │   └── [componente].Api.Tests/
    │       ├── DataMocks/
    │       ├── Mocks/
    │       └── Tests/
    ├── 1 - Application/
    │   └── [componente].Application.Tests/
    │       ├── DataMocks/
    │       ├── Mocks/
    │       └── Tests/
    ├── 2 - Domain/
    │   └── [componente].Domain.Tests/
    │       ├── DataMocks/
    │       ├── Mocks/
    │       └── Tests/
    └── 3 - Infrastructure/
        └── [componente].Infrastructure.Tests/
            ├── DataMocks/
            ├── Mocks/
            └── Tests/
```

---

## Responsabilidade por Camada

| Camada | Projeto | Responsabilidade |
|--------|---------|-----------------|
| Presentation | `[componente].Api` | Endpoints, validação de request, AppServices, DTOs, mapeamento Presentation↔Domain, autenticação, filtros e middlewares de pipeline |
| Application | `[componente].Application` | Implementação das services de aplicação, orquestração de casos de uso |
| Domain | `[componente].Domain` | Entidades, models, value objects, enums, exceções, helpers, extensions, contratos de repositórios, interfaces de services, contratos de integração, queries Dapper, Result Pattern, mapeamento Domain↔Entity e Domain↔Integration |
| Infrastructure | `[componente].Infrastructure` | Implementação de repositórios, DbContext, configurações EF Core, clientes de integração, mensageria, políticas de resiliência |

---

## Objetos por Camada

| Camada | Objetos utilizados |
|--------|--------------------|
| Presentation | `Request`, `Response` (DTOs) |
| Application | `Model`, `CreateModel` (Domain) |
| Infrastructure | `Entity` (Domain), objetos de integração (Domain) |

Consulte `layer-objects.md` e `automapper-profiles.md`.

---

## Fluxo de Dados

```
Request (DTO)
  → Endpoint ([componente].Api)
    → ValidationFilter ([componente].Api)
      → AppService ([componente].Api)
        → [AutoMapper] Request → Model
        → Service ([componente].Application)
          → [AutoMapper] Model → Entity
          → Repository / Integration Client ([componente].Infrastructure)
          ← Result<Entity>
          → [AutoMapper] Entity → Model
        ← Result<Model>
      ← Result<Model>
      → [AutoMapper] Model → Response
    ← Response (DTO)
  ← Response (DTO)
```

---

## Dependências entre Camadas

```
Api            → Application, Domain
Application    → Domain
Domain         → (nenhuma dependência interna)
Infrastructure → Domain
```

- `Infrastructure` implementa os contratos definidos no `Domain`
- `Application` implementa as interfaces de service definidas no `Domain`
- `Api` consome as interfaces de service do `Domain` via AppServices
- Nenhuma camada referencia diretamente a implementação de outra

---

## Padrões e Contextos Relacionados

| Padrão / Contexto | Arquivo |
|-------------------|---------|
| Arquitetura Geral | `solution-architecture.md` |
| Camada Presentation | `layer-presentation.md` |
| Camada Application | `layer-application.md` |
| Camada Domain | `layer-domain.md` |
| Camada Infrastructure | `layer-infrastructure.md` |
| Objetos por Camada | `layer-objects.md` |
| AutoMapper Profiles | `automapper-profiles.md` |
| Minimal APIs | `minimal-apis.md` |
| App Services | `app-services.md` |
| Validators | `validators.md` |
| Result Pattern | `result-pattern.md` |
| Generic Repository | `generic-repository.md` |
| Unit of Work | `unit-of-work.md` |
| Builder Pattern | `builder.md` |
| SOLID | `solid.md` |
| Autenticação e Autorização | `auth.md` |
| Logging | `logging-standards.md` |
| Exception Handling | `exception-handling.md` |
| Dependency Injection | `dependency-injection.md` |
| API Documentation | `api-documentation.md` |
| API Integrations | `apis-integrations.md` |
| AWS Integrations | `aws-integrations.md` |
| Kafka Integrations | `kafka-integrations.md` |
| RabbitMQ Integrations | `rabbit-mq-integrations.md` |
| Messaging Resilience | `messaging-resilience.md` |
| Test Architecture | `test-architecture.md` |
| Unit Tests | `unit-tests.md` |
| Mock Classes | `mock-classes.md` |
| Data Mocks | `data-mocks.md` |