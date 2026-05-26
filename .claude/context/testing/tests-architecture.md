# Test Architecture

## Visão Geral

Os projetos de teste espelham a estrutura de camadas da solução. Cada camada possui seu próprio projeto de testes, organizado dentro de uma pasta `Tests/` na raiz da solution. Isso garante isolamento entre as suítes de teste e clareza sobre qual camada cada teste cobre.

---

## Estrutura

```
[componente]/
├── 0 - Presentation/
│   └── [componente].Api/
├── 1 - Application/
│   └── [componente].Application/
├── 2 - Domain/
│   └── [componente].Domain/
├── 3 - Infrastructure/
│   └── [componente].Infrastructure/
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

## Responsabilidade por Projeto de Testes

| Projeto | O que testa |
|---------|-------------|
| `[componente].Api.Tests` | Endpoints, validators de request, app services, filtros e middlewares |
| `[componente].Application.Tests` | Services de aplicação, middlewares de aplicação |
| `[componente].Domain.Tests` | Regras de domínio, entidades, value objects, result pattern |
| `[componente].Infrastructure.Tests` | Repositórios, clientes de integração, consumers e producers de mensageria |

---

## Estrutura Interna de cada Projeto

Cada projeto de testes segue a mesma organização interna:

```
[componente].X.Tests/
├── DataMocks/          — objetos de dados reutilizáveis por cenário de teste
│   ├── Requests/
│   ├── Responses/
│   ├── Models/
│   └── Messages/
├── Mocks/              — mock classes de dependências via Moq
│   ├── AppServices/
│   ├── Services/
│   ├── Repositories/
│   └── Integrations/
└── Tests/              — classes de teste organizadas por contexto
    ├── AppServices/
    ├── Services/
    ├── Validators/
    └── Repositories/
```

---

## Configuração de Cobertura por Projeto

Cada projeto de testes usa **`coverlet.msbuild`** e **deve declarar `<Include>` no `.csproj`** para restringir o cálculo de cobertura ao assembly alvo. Sem esse filtro, o coverlet mede todos os assemblies transitivamente carregados — incluindo camadas que não são responsabilidade do projeto — derrubando o total para valores incorretos (ex: Api.Tests medindo Infrastructure com 0%).

```xml
<!-- Api.Tests.csproj -->
<PropertyGroup>
  <Include>[NomeDoProjeto.Api]*</Include>
</PropertyGroup>

<!-- Application.Tests.csproj -->
<PropertyGroup>
  <Include>[NomeDoProjeto.Application]*</Include>
</PropertyGroup>

<!-- Infrastructure.Tests.csproj -->
<PropertyGroup>
  <Include>[NomeDoProjeto.Infrastructure]*</Include>
  <ExcludeByFile>**/Migrations/**/*.cs</ExcludeByFile>
</PropertyGroup>
```

**Regras:**
- Todo projeto de testes declara `<Include>` com o assembly da sua camada
- `Infrastructure.Tests` sempre declara `<ExcludeByFile>**/Migrations/**/*.cs</ExcludeByFile>` — Migrations são geradas automaticamente pelo EF Core e não têm cobertura real
- O filtro `[NomeDoAssembly]*` cobre todos os tipos do assembly sem precisar especificar namespace

### Testes de Repositório (Infrastructure.Tests)

Repositórios que dependem de `DbContext` devem ser testados com **EF Core InMemory** — sem banco real, sem mocks do contexto.

```csharp
private static AppDbContext CreateDbContext() =>
    new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())  // nome único por teste garante isolamento
        .Options);
```

Package necessário no `Infrastructure.Tests.csproj`:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.x" />
```

---

## Convenções

- Cada projeto de testes referencia apenas o projeto da camada correspondente — nunca projetos de outras camadas diretamente
- O nome do projeto de testes segue o padrão `[componente].[Escopo].Tests`
- A organização interna de `DataMocks/`, `Mocks/` e `Tests/` segue os padrões definidos em `data-mocks.md`, `mock-classes.md` e `unit-tests.md`
- Testes de integração entre camadas, quando necessários, são tratados em projetos separados e não pertencem a nenhum dos projetos acima
- Todo novo projeto de testes deve ser registrado no `.slnx` da solution