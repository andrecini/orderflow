# Folder Structure

## Visão Geral

O repositório segue uma estrutura organizada com o código-fonte isolado dentro de `src/`, mantendo a raiz do projeto limpa com apenas arquivos de documentação e configuração global.

---

## Estrutura do Repositório

```
[nome-do-projeto]/
├── src/
│   ├── 0 - Presentation/
│   │   └── [componente].Api/
│   │       ├── AppServices/
│   │       │   └── Interfaces/
│   │       ├── DTOs/
│   │       │   ├── Requests/
│   │       │   └── Responses/
│   │       ├── Endpoints/
│   │       │   └── [Resource]/
│   │       ├── Filters/
│   │       ├── Mappings/
│   │       ├── Middlewares/
│   │       ├── Validators/
│   │       │   └── [Resource]/
│   │       └── ApiDependency.cs
│   │
│   ├── 1 - Application/
│   │   └── [componente].Application/
│   │       ├── Services/
│   │       │   └── [Resource]/
│   │       └── ApplicationDependency.cs
│   │
│   ├── 2 - Domain/
│   │   └── [componente].Domain/
│   │       ├── Entities/
│   │       ├── Enums/
│   │       ├── Exceptions/
│   │       ├── Extensions/
│   │       ├── Helpers/
│   │       ├── Integrations/
│   │       │   ├── Apis/
│   │       │   ├── Aws/
│   │       │   ├── Kafka/
│   │       │   ├── RabbitMq/
│   │       │   └── Sql/
│   │       ├── Interfaces/
│   │       │   ├── Repositories/
│   │       │   └── Services/
│   │       ├── Mappings/
│   │       ├── Models/
│   │       │   └── [Resource]/
│   │       ├── Result/
│   │       ├── ValueObjects/
│   │       └── DomainDependency.cs
│   │
│   ├── 3 - Infrastructure/
│   │   └── [componente].Infrastructure/
│   │       ├── Data/
│   │       │   ├── Configurations/
│   │       │   └── AppDbContext.cs
│   │       ├── Integrations/
│   │       │   ├── Apis/
│   │       │   ├── Aws/
│   │       │   ├── Kafka/
│   │       │   └── RabbitMq/
│   │       ├── Messaging/
│   │       ├── Policies/
│   │       ├── Repositories/
│   │       │   └── [Resource]/
│   │       └── InfrastructureDependency.cs
│   │
│   ├── Tests/
│   │   ├── 0 - Presentation/
│   │   │   └── [componente].Api.Tests/
│   │   │       ├── DataMocks/
│   │   │       ├── Mocks/
│   │   │       └── Tests/
│   │   ├── 1 - Application/
│   │   │   └── [componente].Application.Tests/
│   │   │       ├── DataMocks/
│   │   │       ├── Mocks/
│   │   │       └── Tests/
│   │   ├── 2 - Domain/
│   │   │   └── [componente].Domain.Tests/
│   │   │       ├── DataMocks/
│   │   │       ├── Mocks/
│   │   │       └── Tests/
│   │   └── 3 - Infrastructure/
│   │       └── [componente].Infrastructure.Tests/
│   │           ├── DataMocks/
│   │           ├── Mocks/
│   │           └── Tests/
│   │
│   └── [componente].sln
│
├── README.md
└── CHANGELOG.md
```

---

## Convenções

- Todo o código-fonte reside dentro de `src/` — nada de código na raiz do repositório
- A solution file `[componente].sln` fica dentro de `src/`, na raiz do código-fonte
- As pastas de camada são prefixadas com números (`0 - Presentation`, `1 - Application`, etc.) para definir a ordem de dependência visualmente no explorador de arquivos
- `README.md` e `CHANGELOG.md` ficam na raiz do repositório — fora de `src/`
- O `[nome-do-projeto]` é o nome do repositório e pode diferir do `[componente]` usado nos namespaces e projetos