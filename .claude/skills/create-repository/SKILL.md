---
name: create-repository
description: 'Use this skill when the user asks to create a repository for a specific resource. Trigger for prompts like "create a repository for X", "implement the repository layer for Y", "add data access for Z". Do not trigger for full feature creation — use create-feature instead.'
allowed-tools: Read, Edit, Write, Glob, Grep
---

## Guardrails

- **Escopo restrito às camadas de Domain e Infrastructure** — interface em `[componente].Domain/Interfaces/Repositories/` e implementação em `[componente].Infrastructure/Repositories/`
- **Sem criação de endpoints ou AppServices** — responsabilidade das skills `create-feature` ou `create-endpoint`
- **Sem criação de migrations** — responsabilidade exclusiva da skill `create-migration`
- **Sem acesso a `appsettings.Production.json`** — nunca ler ou alterar arquivos com credenciais
- **Sem alteração de `XDependency.cs` de outras camadas** — apenas `InfrastructureDependency.cs`
- **Sem queries inline** — sempre referenciar constantes do Domain em `Integrations/Sql/`
- **Soft delete obrigatório** — nunca usar `Remove()` do EF Core; sempre `DeletedAt`
- **Perguntar antes de sobrescrever** — se um arquivo já existir, nunca sobrescrever sem confirmação

# Skill: Create Repository

## MCP

### 1. Inspecionar schema via PostgreSQL ou MongoDB MCP

**Para repositórios SQL — PostgreSQL MCP:**

```
list_tables → confirmar se a tabela correspondente existe
describe_table → inspecionar colunas para validar mapeamento EF/Dapper
```

**Para repositórios NoSQL — MongoDB MCP:**

```
list_collections → confirmar se a collection existe
describe_collection → inspecionar campos para validar mapeamento
```

### 2. Verificar arquivos existentes via Filesystem MCP

```
read_file → src/[componente].Domain/Interfaces/Repositories/I[Recurso]Repository.cs
read_file → src/[componente].Infrastructure/Repositories/[Recurso]/[Recurso]Repository.cs
```

### 3. Escrever arquivos via Filesystem MCP

```
write_file → src/[componente].Domain/Interfaces/Repositories/...
write_file → src/[componente].Domain/Integrations/Sql/...  (se Dapper)
write_file → src/[componente].Infrastructure/Repositories/...
write_file → src/Tests/.../Repositories/...
```

---

## Objetivo

Guia a criação isolada de um repositório — interface no Domain e implementação na Infrastructure — decidindo automaticamente entre EF Core e Dapper com justificativa. Gera também os testes unitários correspondentes.

---

## Contextos Necessários

- [generic-repository.md](../../context/patterns/generic-repository.md)
- [unit-of-work.md](../../context/patterns/unit-of-work.md)
- [ef-standards.md](../../context/persistence/ef-standards.md)
- [dapper-standards.md](../../context/persistence/dapper-standards.md)
- [query-patterns.md](../../context/persistence/query-patterns.md)
- [layer-domain.md](../../context/architecture/layer-domain.md)
- [layer-infrastructure.md](../../context/architecture/layer-infrastructure.md)
- [layer-objects.md](../../context/architecture/layer-objects.md)
- [result-pattern.md](../../context/patterns/result-pattern.md)
- [unit-tests.md](../../context/testing/unit-tests.md)
- [mock-classes.md](../../context/testing/mock-classes.md)
- [data-mocks.md](../../context/testing/data-mocks.md)

---

## Entrada

O usuário deve fornecer:

- **Recurso** — ex: `Order`, `Product`, `Customer`
- **Operações** — ex: `GetById`, `GetPaged`, `Create`, `Update`, `Delete`. Se não informadas, perguntar:

```
Quais operações o repositório deve suportar?
1. CRUD completo (GetById, GetAll, GetPaged, Add, Update, Delete)
2. Apenas leitura (GetById, GetAll, GetPaged, Find)
3. Customizado — informar quais operações
```

---

## Passos

### 1. Confirmar entradas
Se recurso ou operações não foram informados, perguntar antes de prosseguir.

### 2. Decidir EF Core vs Dapper

Analisar as operações solicitadas e decidir automaticamente conforme [query-patterns.md](../../context/persistence/query-patterns.md). Apresentar a decisão ao usuário antes de prosseguir:

```
📋 Decisão de implementação:

✅ EF Core — para as operações: [lista]
Motivo: [ex: operações de CRUD simples sem necessidade de controle total do SQL]

✅ Dapper — para as operações: [lista]
Motivo: [ex: leitura paginada com JOIN complexo que exige performance máxima]

Deseja prosseguir com essa decisão ou ajustar?
1. Prosseguir
2. Ajustar — informar preferência
```

### 3. Gerar interface no Domain

Seguindo [generic-repository.md](../../context/patterns/generic-repository.md), criar a interface em `[componente].Domain/Interfaces/Repositories/`:

```csharp
public interface IOrderRepository : ISqlRepository<Order>
{
    Task<IEnumerable<OrderSummaryModel>> GetSummariesByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken);
}
```

### 4. Gerar queries Dapper (se aplicável)

Se Dapper for utilizado, criar as queries em `[componente].Domain/Integrations/Sql/[Recurso]/[Recurso]Queries.cs` — consulte [dapper-standards.md](../../context/persistence/dapper-standards.md):

```csharp
public static class OrderQueries
{
    public const string GetSummariesByCustomer = """
        SELECT
            o.id AS Id,
            o.status AS Status,
            o.total_amount AS TotalAmount,
            o.created_at AS CreatedAt
        FROM orders o
        WHERE o.customer_id = @CustomerId
          AND o.deleted_at IS NULL
        ORDER BY o.created_at DESC
        """;
}
```

### 5. Gerar implementação na Infrastructure

Criar `[Recurso]Repository` em `[componente].Infrastructure/Repositories/[Recurso]/` implementando a interface do Domain:

#### Operações EF Core
```csharp
public class OrderRepository(AppDbContext dbContext, IMapper mapper) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Orders.FindAsync(id, cancellationToken);

    public async Task AddAsync(Order entity, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken)
    {
        dbContext.Orders.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Orders.FindAsync(id, cancellationToken);
        if (entity is null) return;
        entity.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

#### Operações Dapper
```csharp
    private IDbConnection Connection => dbContext.Database.GetDbConnection();

    public async Task<IEnumerable<OrderSummaryModel>> GetSummariesByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken)
        => await Connection.QueryAsync<OrderSummaryModel>(
            OrderQueries.GetSummariesByCustomer,
            new { CustomerId = customerId });
```

### 6. Registrar no Unit of Work

Verificar se o repositório está exposto na interface do Unit of Work em `[componente].Domain/Interfaces/`:

```csharp
public interface ISqlUnitOfWork : IAsyncDisposable
{
    IOrderRepository Orders { get; }
    // ...
}
```

Se não estiver, adicionar a propriedade.

### 7. Registrar DI

Adicionar registro em `[componente].Infrastructure/InfrastructureDependency.cs`:

```csharp
services.AddScoped<IOrderRepository, OrderRepository>();
```

### 8. Gerar testes unitários

Seguindo [unit-tests.md](../../context/testing/unit-tests.md), [mock-classes.md](../../context/testing/mock-classes.md) e [data-mocks.md](../../context/testing/data-mocks.md):

- **Data Mocks** em `[componente].Infrastructure.Tests/DataMocks/`
- **Mock Classes** em `[componente].Infrastructure.Tests/Mocks/Repositories/`
- **Testes** em `[componente].Infrastructure.Tests/Tests/Repositories/`

```csharp
public class OrderRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ExistingOrder_ReturnsOrderAsync()
    {
        // Arrange
        var order = OrderMock.Valid();
        var dbContext = DbContextMock.WithOrders(order);
        var repository = new OrderRepository(dbContext, mapper);

        // Act
        var result = await repository.GetByIdAsync(order.Id, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(order.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingOrder_ReturnsNullAsync()
    {
        // Arrange
        var dbContext = DbContextMock.Empty();
        var repository = new OrderRepository(dbContext, mapper);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}
```

---

## Output Esperado

```
[componente].Domain/
├── Interfaces/Repositories/I[Recurso]Repository.cs  — criado
└── Integrations/Sql/[Recurso]/[Recurso]Queries.cs   — criado se Dapper

[componente].Infrastructure/
└── Repositories/[Recurso]/[Recurso]Repository.cs    — criado

[componente].Infrastructure.Tests/
├── DataMocks/[Tipo]/[Recurso]Mock.cs                — criado ou atualizado
├── Mocks/Repositories/[Recurso]RepositoryMock.cs   — criado
└── Tests/Repositories/[Recurso]RepositoryTests.cs  — criado
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] Interface criada no Domain — nunca na Infrastructure
- [ ] Implementação depende apenas de abstrações do Domain
- [ ] Soft delete via `DeletedAt` — nunca `Remove()` do EF Core
- [ ] Queries Dapper em constantes do Domain — nunca inline
- [ ] Filtro `deleted_at IS NULL` aplicado manualmente nas queries Dapper
- [ ] `AsNoTracking()` em queries EF Core de leitura sem atualização posterior
- [ ] `CancellationToken` propagado em todas as operações assíncronas
- [ ] Construtor primário utilizado
- [ ] Repositório exposto no Unit of Work
- [ ] Registro adicionado na `InfrastructureDependency.cs`
- [ ] Data Mock possui método `Valid()` obrigatório
- [ ] Testes cobrem ao menos 85% dos cenários testáveis

---

## Prompt Examples

- "cria o repository de pedidos"
- "implementa o acesso a dados para a entidade Product"
- "adiciona o repository de clientes com paginação"
- "quero um repository só de leitura para relatórios"
- "cria o repositório do Payment com GetById e Create"

---

## Related Skills

- `create-service` — criar a service que consumirá o repositório
- `create-migration` — criar a migration se a entidade ainda não existir no banco
- `create-unit-test` — gerar testes unitários para o repositório criado
- `create-dapper-query` — adicionar queries Dapper customizadas ao repositório

---

## Error Handling

- **Entidade não encontrada** — se a entidade correspondente não existir no Domain, alertar e perguntar se deseja criá-la antes de prosseguir
- **Interface já existente** — se `I[Recurso]Repository` já existir, apenas adicionar os novos métodos sem recriar a interface
- **Configuração EF ausente** — se a entidade não tiver `IEntityTypeConfiguration<T>`, alertar e sugerir execução da skill `create-migration` para criá-la
- **Unit of Work não atualizado** — sempre verificar e alertar se o repositório não estiver exposto no Unit of Work após a criação