---
name: create-feature
description: 'Use this skill when the user asks to create a new feature, endpoint, service, repository, or any combination of layers for a new functionality. Trigger for prompts like "create a feature", "implement an endpoint", "add a new service", "create a repository for X". Do not trigger for bug fixes, refactoring, test creation only, or integration creation — use the specific skill for each case.'
allowed-tools: Read, Edit, Write, Glob, Grep, Bash(dotnet *)
---

## Guardrails

- **Escopo restrito ao projeto atual** — nunca criar arquivos fora da estrutura definida em `project-structure.md`
- **Sem acesso a branches externas** — apenas leitura e escrita na branch atual
- **Sem alteração de arquivos existentes sem aviso** — ao atualizar interfaces ou profiles já existentes, informar o usuário antes de aplicar
- **Sem geração de código fora da stack** — apenas C# e .NET 8; nunca gerar scripts, arquivos de configuração ou código em outras linguagens
- **Sem criação de migrations** — responsabilidade exclusiva da skill `create-migration`
- **Sem alteração de `XDependency.cs` de outras camadas** — apenas registrar na `XDependency.cs` da camada correspondente ao artefato gerado
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler ou alterar `appsettings.Production.json` ou arquivos com credenciais
- **Sem geração de testes de integração** — responsabilidade exclusiva da skill `create-integration-test`
- **Perguntar antes de sobrescrever** — se um arquivo já existir, nunca sobrescrever sem confirmação do usuário

# Skill: Create Feature

## MCP

### 1. Verificar arquivos existentes via Filesystem MCP

Antes de criar qualquer artefato, usar o Filesystem MCP para verificar quais arquivos já existem:
 
```
read_multiple_files / list_directory → src/[componente].Api/AppServices/
read_multiple_files / list_directory → src/[componente].Domain/Interfaces/
read_multiple_files / list_directory → src/[componente].Infrastructure/Repositories/
```

### 2. Escrever arquivos via Filesystem MCP

Após gerar todos os artefatos, usar o Filesystem MCP para criar os arquivos confirmados:
 
```
write_file → src/[componente].Api/...
write_file → src/[componente].Application/...
write_file → src/[componente].Domain/...
write_file → src/[componente].Infrastructure/...
write_file → src/Tests/...
```

---

## Objetivo

Guia a criação completa de uma feature seguindo os padrões arquiteturais do projeto. O escopo de geração é definido pelo usuário — se não informado, deve ser perguntado antes de iniciar.

---

## Contextos Necessários

Consulte os seguintes arquivos antes de executar, conforme o escopo solicitado:

| Escopo | Contextos |
|--------|-----------|
| Endpoint | [minimal-apis.md](../../context/development/minimal-apis.md) · [validators.md](../../context/development/validators.md) · [filters.md](../../context/development/filters.md) · [api-documentation.md](../../context/development/api-documentation.md) · [auth.md](../../context/development/auth.md) |
| App Service | [app-services.md](../../context/development/app-services.md) · [automapper-profiles.md](../../context/architecture/automapper-profiles.md) |
| Service | [layer-application.md](../../context/architecture/layer-application.md) · [result-pattern.md](../../context/patterns/result-pattern.md) |
| Repository | [generic-repository.md](../../context/patterns/generic-repository.md) · [unit-of-work.md](../../context/patterns/unit-of-work.md) · [ef-standards.md](../../context/persistence/ef-standards.md) · [dapper-standards.md](../../context/persistence/dapper-standards.md) · [query-patterns.md](../../context/persistence/query-patterns.md) |
| Testes | [unit-tests.md](../../context/testing/unit-tests.md) · [mock-classes.md](../../context/testing/mock-classes.md) · [data-mocks.md](../../context/testing/data-mocks.md) |
| Objetos e Mapeamento | [layer-objects.md](../../context/architecture/layer-objects.md) · [automapper-profiles.md](../../context/architecture/automapper-profiles.md) |
| DI | [dependency-injection.md](../../context/development/dependency-injection.md) |

---

## Entrada

O usuário deve fornecer:

- **Nome do recurso** — ex: `Order`, `Payment`, `Customer`
- **Operação** — ex: `Create`, `Get`, `Update`, `Delete`, `List`
- **Escopo de geração** — quais camadas devem ser geradas. Se não informado, perguntar antes de iniciar:

```
Qual o escopo de geração da feature?
1. Completo (Endpoint → Validator → AppService → Service → Repository → Testes)
2. Apenas API (Endpoint + Validator + AppService)
3. Apenas domínio (Service + Repository)
4. Apenas testes
5. Customizado — informar quais camadas
```

---

## Passos

### 1. Confirmar escopo
Se o escopo não foi informado, apresentar as opções acima e aguardar resposta antes de prosseguir.

### 2. Gerar objetos por camada
Seguindo [layer-objects.md](../../context/architecture/layer-objects.md):

- **Presentation:** `[Operação][Recurso]Request` e `[Operação][Recurso]Response` em `[componente].Api/DTOs/`
- **Domain:** `[Operação][Recurso]Model` e `[Recurso]Model` em `[componente].Domain/Models/[Recurso]/`
- **Infrastructure:** Entity `[Recurso]` em `[componente].Domain/Entities/` se ainda não existir

### 3. Gerar profiles AutoMapper
Seguindo [automapper-profiles.md](../../context/architecture/automapper-profiles.md):

- **Presentation profile** em `[componente].Api/Mappings/[Recurso]Profile.cs`:
  - `[Operação][Recurso]Request` → `[Operação][Recurso]Model`
  - `[Recurso]Model` → `[Operação][Recurso]Response`
- **Domain profile** em `[componente].Domain/Mappings/[Recurso]Profile.cs`:
  - `[Operação][Recurso]Model` → `[Recurso]` (Entity)
  - `[Recurso]` (Entity) → `[Recurso]Model`

### 4. Gerar validator
Seguindo [validators.md](../../context/development/validators.md):

- Criar `[Operação][Recurso]RequestValidator` em `[componente].Api/Validators/[Recurso]/`
- Cobrir todos os campos obrigatórios e regras de formato

### 5. Gerar endpoint
Seguindo [minimal-apis.md](../../context/development/minimal-apis.md):

- Criar `[Operação][Recurso]Endpoint` em `[componente].Api/Endpoints/[Recurso]/`
- Declarar `.WithName()`, `.WithSummary()`, `.WithTags()`, `.WithOpenApi()`
- Declarar `.RequireAuthorization("policy-name")` se autenticado
- Usar `TypedResults` para retorno

### 6. Gerar App Service
Seguindo [app-services.md](../../context/development/app-services.md):

- Criar interface `I[Recurso]AppService` em `[componente].Api/AppServices/Interfaces/`
- Criar `[Recurso]AppService` em `[componente].Api/AppServices/`
- Mapear request → model via AutoMapper, invocar service, mapear result → response

### 7. Gerar Service
Seguindo [layer-application.md](../../context/architecture/layer-application.md) e [result-pattern.md](../../context/patterns/result-pattern.md):

- Adicionar método na interface `I[Recurso]Service` em `[componente].Domain/Interfaces/Services/`
- Implementar em `[componente].Application/Services/[Recurso]/[Recurso]Service.cs`
- Retornar sempre `Result<T>` ou `Result`

### 8. Gerar Repository
Seguindo [generic-repository.md](../../context/patterns/generic-repository.md) e [query-patterns.md](../../context/persistence/query-patterns.md):

- Adicionar método na interface `I[Recurso]Repository` em `[componente].Domain/Interfaces/Repositories/`
- Implementar em `[componente].Infrastructure/Repositories/[Recurso]/[Recurso]Repository.cs`
- Decidir EF Core vs Dapper conforme [query-patterns.md](../../context/persistence/query-patterns.md)
- Queries Dapper em `[componente].Domain/Integrations/Sql/[Recurso]/[Recurso]Queries.cs`

### 9. Registrar DI
Seguindo [dependency-injection.md](../../context/development/dependency-injection.md):

- `[Recurso]AppService` → `[componente].Api/ApiDependency.cs`
- `[Recurso]Service` → `[componente].Application/ApplicationDependency.cs`
- `[Recurso]Repository` → `[componente].Infrastructure/InfrastructureDependency.cs`
- Profiles AutoMapper → `AddAutoMapper()` nas respectivas `XDependency.cs`

### 10. Gerar testes
Seguindo [unit-tests.md](../../context/testing/unit-tests.md), [mock-classes.md](../../context/testing/mock-classes.md) e [data-mocks.md](../../context/testing/data-mocks.md):

- **Data Mocks** por tipo de objeto em `[componente].X.Tests/DataMocks/`
  - `[Operação][Recurso]RequestMock` com `Valid()` e cenários de falha
  - `[Operação][Recurso]ModelMock` com `Valid()`
  - `[Recurso]ModelMock` com `Valid()`
- **Mock Classes** em `[componente].X.Tests/Mocks/`
  - `[Recurso]ServiceMock` com setup dos métodos utilizados
  - `[Recurso]RepositoryMock` com setup dos métodos utilizados
- **Testes** em `[componente].X.Tests/Tests/`
  - `[Recurso]AppServiceTests` cobrindo cenários de sucesso e falha
  - `[Recurso]ServiceTests` cobrindo regras de negócio
  - `[Operação][Recurso]RequestValidatorTests` cobrindo todos os campos

---

## Output Esperado

Lista de artefatos gerados conforme o escopo definido:

```
[componente].Api/
├── AppServices/[Recurso]AppService.cs
├── AppServices/Interfaces/I[Recurso]AppService.cs
├── DTOs/Requests/[Operação][Recurso]Request.cs
├── DTOs/Responses/[Operação][Recurso]Response.cs
├── Endpoints/[Recurso]/[Operação][Recurso]Endpoint.cs
├── Mappings/[Recurso]Profile.cs
└── Validators/[Recurso]/[Operação][Recurso]RequestValidator.cs

[componente].Application/
└── Services/[Recurso]/[Recurso]Service.cs

[componente].Domain/
├── Entities/[Recurso].cs
├── Integrations/Sql/[Recurso]/[Recurso]Queries.cs (se Dapper)
├── Interfaces/Repositories/I[Recurso]Repository.cs
├── Interfaces/Services/I[Recurso]Service.cs
├── Mappings/[Recurso]Profile.cs
└── Models/[Recurso]/

[componente].Infrastructure/
└── Repositories/[Recurso]/[Recurso]Repository.cs

[componente].X.Tests/
├── DataMocks/Requests/[Operação][Recurso]RequestMock.cs
├── DataMocks/Models/[Operação][Recurso]ModelMock.cs
├── DataMocks/Models/[Recurso]ModelMock.cs
├── Mocks/Services/[Recurso]ServiceMock.cs
├── Mocks/Repositories/[Recurso]RepositoryMock.cs
├── Tests/AppServices/[Recurso]AppServiceTests.cs
├── Tests/Services/[Recurso]ServiceTests.cs
└── Tests/Validators/[Operação][Recurso]RequestValidatorTests.cs
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] Objetos corretos em cada camada — [layer-objects.md](../../context/architecture/layer-objects.md)
- [ ] AutoMapper usado em todos os mapeamentos entre camadas
- [ ] `Result<T>` retornado em todas as operações de service e repository
- [ ] `CancellationToken` propagado em todas as operações assíncronas
- [ ] Construtores primários em todas as classes com DI
- [ ] `TypedResults` usado no endpoint
- [ ] `.RequireAuthorization()` declarado se o endpoint for autenticado
- [ ] Todos os novos serviços registrados nas `XDependency.cs` corretas
- [ ] Data Mock possui método `Valid()` obrigatório
- [ ] Testes cobrem ao menos 85% dos cenários testáveis
- [ ] Nomenclatura de arquivos e classes seguindo os padrões de cada camada

---

## Prompt Examples

- "cria uma feature de pedidos com endpoint, service e repository"
- "implementa o fluxo completo de criação de pagamento"
- "adiciona a funcionalidade de listagem de clientes"
- "quero um CRUD de produtos"
- "cria o endpoint e a service de cancelamento de pedido"

---

## Related Skills

- `create-unit-test` — gerar testes unitários para os artefatos criados
- `create-integration-test` — gerar testes de integração para o endpoint criado
- `create-migration` — criar migration se uma nova entidade foi gerada
- `code-review` — revisar os artefatos gerados antes do commit

---

## Error Handling

- **Recurso já existente** — se a entidade, interface ou implementação já existir, alertar o usuário e perguntar se deseja adicionar uma nova operação ao artefato existente ou criar um novo
- **Estrutura de pastas divergente** — se a estrutura do projeto não seguir o padrão de `project-structure.md`, alertar e aguardar confirmação antes de prosseguir
- **Service não encontrada** — se o escopo incluir AppService mas a interface da Service correspondente não existir, sugerir execução da skill `create-service` antes de prosseguir
- **Repository não encontrado** — se o escopo incluir Service mas o Repository correspondente não existir, sugerir execução da skill `create-repository` antes de prosseguir
- **Escopo não informado** — nunca assumir escopo; sempre perguntar antes de gerar qualquer artefato