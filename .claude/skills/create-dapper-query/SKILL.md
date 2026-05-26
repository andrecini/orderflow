---
name: create-dapper-query
description: 'Use this skill when the user asks to create a Dapper query for a specific resource. Trigger for prompts like "create a Dapper query for X", "write a SQL query for Y", "add a custom query for Z". Do not trigger for EF Core operations or full repository creation — use create-repository instead.'
allowed-tools: Read, Edit, Write, Glob, Grep
---

## Guardrails

- **Escopo restrito a queries Dapper** — nunca criar operações EF Core; use `create-repository` para isso
- **Sem queries inline** — sempre definir como constante em `[componente].Domain/Integrations/Sql/`
- **Sem acesso a bancos de produção** — apenas leitura de schema para referência
- **Sem acesso a `appsettings.Production.json`** — nunca ler arquivos com credenciais
- **Filtro de soft delete obrigatório** — sempre incluir `deleted_at IS NULL` nas queries
- **Sem alteração de queries existentes** — apenas adicionar novas constantes; nunca modificar queries já existentes sem confirmação
- **Confirmar SQL gerado antes de aplicar** — sempre apresentar o SQL ao usuário antes de criar os artefatos

# Skill: Create Dapper Query

## MCP 

### 1. Inspecionar schema via PostgreSQL MCP

Antes de gerar o SQL, consultar o schema real do banco:

```
list_tables → confirmar tabelas envolvidas na query
describe_table → inspecionar colunas disponíveis para SELECT e WHERE
```

Usar os dados retornados para validar nomes de tabelas, colunas e aliases.

### 2. Verificar arquivos existentes via Filesystem MCP

```
read_file → src/[componente].Domain/Integrations/Sql/[Recurso]/[Recurso]Queries.cs
read_file → src/[componente].Domain/Interfaces/Repositories/I[Recurso]Repository.cs
read_file → src/[componente].Infrastructure/Repositories/[Recurso]/[Recurso]Repository.cs
```

### 3. Escrever arquivos via Filesystem MCP

```
write_file → src/[componente].Domain/Integrations/Sql/...
write_file → src/[componente].Domain/Interfaces/Repositories/...
write_file → src/[componente].Infrastructure/Repositories/...
write_file → src/Tests/.../Repositories/...
```

---

## Objetivo

Guia a criação de uma query Dapper completa — do SQL à implementação no repositório. Aceita descrição em linguagem natural ou SQL direto. Gera a constante no Domain, o modelo de resultado, a implementação no repositório e os testes correspondentes.

---

## Contextos Necessários

- [dapper-standards.md](../../context/persistence/dapper-standards.md)
- [query-patterns.md](../../context/persistence/query-patterns.md)
- [sql.md](../../context/persistence/sql.md)
- [generic-repository.md](../../context/patterns/generic-repository.md)
- [layer-domain.md](../../context/architecture/layer-domain.md)
- [layer-infrastructure.md](../../context/architecture/layer-infrastructure.md)
- [unit-tests.md](../../context/testing/unit-tests.md)
- [mock-classes.md](../../context/testing/mock-classes.md)
- [data-mocks.md](../../context/testing/data-mocks.md)

---

## Entrada

O usuário deve fornecer:

- **Recurso** — ex: `Order`, `Product`, `Customer`
- **Descrição da query** — em linguagem natural ou SQL direto. Se não informado, perguntar:

```
Como deseja definir a query?
1. Linguagem natural — descrever o que a query deve retornar
   Ex: "listar todos os pedidos de um cliente com status e total, ordenados por data"
2. SQL direto — fornecer o SQL já escrito
```

- **Parâmetros** — se não informados, identificar automaticamente a partir da descrição ou SQL

---

## Passos

### 1. Confirmar entradas
Se recurso ou descrição não foram informados, perguntar antes de prosseguir.

### 2. Gerar ou validar o SQL

#### Se linguagem natural
Gerar o SQL com base na descrição, seguindo as convenções de [sql.md](../../context/persistence/sql.md):

- Nomes de tabelas e colunas em `snake_case`
- Filtro `deleted_at IS NULL` sempre aplicado
- Aliases das colunas correspondendo às propriedades do modelo de resultado
- Ordenação e paginação quando aplicável

```sql
SELECT
    o.id           AS Id,
    o.status       AS Status,
    o.total_amount AS TotalAmount,
    o.created_at   AS CreatedAt,
    c.name         AS CustomerName
FROM orders o
INNER JOIN customers c ON c.id = o.customer_id
WHERE o.customer_id = @CustomerId
  AND o.deleted_at IS NULL
ORDER BY o.created_at DESC
```

Apresentar o SQL gerado ao usuário antes de prosseguir:

```
SQL gerado:

[sql gerado]

Deseja usar esse SQL ou ajustar?
1. Usar o SQL gerado
2. Ajustar — informar as mudanças
```

#### Se SQL direto
Validar o SQL fornecido contra as convenções do projeto:

- Verificar se `deleted_at IS NULL` está aplicado
- Verificar se aliases estão em `PascalCase` para mapeamento correto
- Alertar sobre qualquer desvio encontrado antes de prosseguir

### 3. Gerar modelo de resultado

Criar o modelo de resultado em `[componente].Domain/Models/[Recurso]/` com propriedades correspondendo aos aliases do SQL:

```csharp
public class OrderSummaryModel
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CustomerName { get; init; } = string.Empty;
}
```

Se o modelo já existir e os campos forem compatíveis, reutilizá-lo. Se houver conflito de campos, alertar o usuário e sugerir a criação de um novo modelo.

### 4. Gerar constante no Domain

Criar ou atualizar a classe de queries em `[componente].Domain/Integrations/Sql/[Recurso]/[Recurso]Queries.cs`:

```csharp
public static class OrderQueries
{
    public const string GetSummariesByCustomer = """
        SELECT
            o.id           AS Id,
            o.status       AS Status,
            o.total_amount AS TotalAmount,
            o.created_at   AS CreatedAt,
            c.name         AS CustomerName
        FROM orders o
        INNER JOIN customers c ON c.id = o.customer_id
        WHERE o.customer_id = @CustomerId
          AND o.deleted_at IS NULL
        ORDER BY o.created_at DESC
        """;
}
```

Se a classe já existir, adicionar apenas a nova constante preservando as existentes.

### 5. Gerar parâmetros

Definir a classe de parâmetros se a query tiver mais de dois parâmetros — para queries simples, usar objeto anônimo:

```csharp
// Objeto anônimo — queries simples
new { CustomerId = customerId }

// Classe de parâmetro — queries com múltiplos parâmetros
public class GetOrderSummariesParams
{
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public int Offset { get; init; }
    public int PageSize { get; init; }
}
```

### 6. Implementar no repositório

Adicionar o método ao repositório em `[componente].Infrastructure/Repositories/[Recurso]/[Recurso]Repository.cs`:

```csharp
public async Task<IEnumerable<OrderSummaryModel>> GetSummariesByCustomerAsync(
    Guid customerId,
    CancellationToken cancellationToken)
    => await Connection.QueryAsync<OrderSummaryModel>(
        OrderQueries.GetSummariesByCustomer,
        new { CustomerId = customerId });
```

Adicionar o método à interface `I[Recurso]Repository` no Domain se ainda não estiver declarado.

### 7. Gerar testes unitários

Seguindo [unit-tests.md](../../context/testing/unit-tests.md), [mock-classes.md](../../context/testing/mock-classes.md) e [data-mocks.md](../../context/testing/data-mocks.md):

- **Data Mocks** em `[componente].Infrastructure.Tests/DataMocks/Models/`
  - `[NomeDoModelo]Mock` com `Valid()` e cenários relevantes
- **Testes** em `[componente].Infrastructure.Tests/Tests/Repositories/`

```csharp
public class OrderRepositoryTests
{
    [Fact]
    public async Task GetSummariesByCustomerAsync_ExistingCustomer_ReturnsSummariesAsync()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedSummaries = new List<OrderSummaryModel>
        {
            OrderSummaryModelMock.Valid()
        };

        var connection = ConnectionMock.WithQueryResult(expectedSummaries);
        var repository = new OrderRepository(dbContext, mapper);

        // Act
        var result = await repository.GetSummariesByCustomerAsync(
            customerId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetSummariesByCustomerAsync_NoOrders_ReturnsEmptyAsync()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var connection = ConnectionMock.WithQueryResult(
            Enumerable.Empty<OrderSummaryModel>());
        var repository = new OrderRepository(dbContext, mapper);

        // Act
        var result = await repository.GetSummariesByCustomerAsync(
            customerId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
```

---

## Output Esperado

```
[componente].Domain/
├── Integrations/Sql/[Recurso]/[Recurso]Queries.cs  — criado ou atualizado
├── Interfaces/Repositories/I[Recurso]Repository.cs — atualizado
└── Models/[Recurso]/[NomeDoModelo].cs               — criado ou reutilizado

[componente].Infrastructure/
└── Repositories/[Recurso]/[Recurso]Repository.cs   — atualizado

[componente].Infrastructure.Tests/
├── DataMocks/Models/[NomeDoModelo]Mock.cs           — criado ou atualizado
└── Tests/Repositories/[Recurso]RepositoryTests.cs  — criado ou atualizado
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] SQL segue as convenções de [sql.md](../../context/persistence/sql.md) — `snake_case`, aliases `PascalCase`
- [ ] Filtro `deleted_at IS NULL` aplicado em todas as queries
- [ ] Query definida como constante no Domain — nunca inline no repositório
- [ ] Aliases das colunas correspondem exatamente às propriedades do modelo
- [ ] Parâmetros nomeados — nunca concatenação de string
- [ ] Método adicionado à interface do repositório no Domain
- [ ] `CancellationToken` propagado
- [ ] Testes cobrem cenário de resultado vazio e resultado com dados
- [ ] Data Mock possui método `Valid()` obrigatório

---

## Prompt Examples

- "cria uma query Dapper para listar pedidos por cliente"
- "adiciona uma consulta SQL paginada para produtos"
- "quero uma query customizada para relatório de vendas"
- "implementa o GetSummariesByStatus com Dapper"
- "escreve a query de busca de clientes inativos"

---

## Related Skills

- `create-repository` — criar ou atualizar o repositório que executará a query
- `create-unit-test` — gerar testes para a query criada

---

## Error Handling

- **SQL inválido fornecido pelo usuário** — alertar sobre os erros encontrados e sugerir correções antes de criar os artefatos
- **Filtro `deleted_at IS NULL` ausente** — adicionar automaticamente e informar o usuário
- **Aliases incompatíveis com o modelo** — se os aliases do SQL não corresponderem às propriedades do modelo de destino, alertar e sugerir correções antes de prosseguir
- **Classe de queries já existente** — nunca recriar; apenas adicionar a nova constante preservando as existentes
- **Modelo de resultado já existente com campos conflitantes** — alertar e perguntar se deseja reutilizar, estender ou criar um novo modelo