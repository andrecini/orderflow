# Dapper Standards

## Visão Geral

O **Dapper** é utilizado como complemento ao Entity Framework Core para cenários que exigem performance máxima, queries SQL complexas ou controle total sobre a execução. Ele acessa o banco via a mesma conexão do `AppDbContext`, garantindo compatibilidade com transações gerenciadas pelo Unit of Work.

---

## Quando usar Dapper

| Cenário | Usar Dapper |
|---------|-------------|
| Leituras em massa com alta performance | ✅ |
| Queries SQL complexas com múltiplos JOINs | ✅ |
| Controle total sobre o SQL gerado | ✅ |
| Bancos legados ou desnormalizados | ✅ |
| CRUD simples em entidades mapeadas | ❌ Usar EF Core |
| Operações dentro de transações gerenciadas | ✅ via mesma conexão |

---

## Acesso à Conexão do AppDbContext

O Dapper utiliza a `DbConnection` extraída do `AppDbContext`, garantindo que queries Dapper e operações EF Core compartilhem a mesma transação quando necessário.

```csharp
public class OrderRepository(AppDbContext dbContext) : IOrderRepository
{
    private IDbConnection Connection => dbContext.Database.GetDbConnection();

    public async Task<IEnumerable<OrderSummaryModel>> GetSummariesAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var parameters = new { CustomerId = customerId };

        return await Connection.QueryAsync<OrderSummaryModel>(
            OrderQueries.GetSummariesByCustomer,
            parameters);
    }
}
```

---

## Queries

As queries Dapper são definidas como constantes estáticas no Domain, organizadas por recurso em `[componente]/[componente].Domain/Integrations/Sql/[Resource]/[Resource]Queries.cs`. Consulte `sql.md` para convenções de nomenclatura.

```csharp
public static class OrderQueries
{
    public const string GetSummariesByCustomer = """
        SELECT
            o.id AS Id,
            o.status AS Status,
            o.total_amount AS TotalAmount,
            o.created_at AS CreatedAt,
            c.name AS CustomerName
        FROM orders o
        INNER JOIN customers c ON c.id = o.customer_id
        WHERE o.customer_id = @CustomerId
          AND o.deleted_at IS NULL
        ORDER BY o.created_at DESC
        """;

    public const string GetPagedByStatus = """
        SELECT
            o.id AS Id,
            o.status AS Status,
            o.total_amount AS TotalAmount,
            o.created_at AS CreatedAt
        FROM orders o
        WHERE o.status = @Status
          AND o.deleted_at IS NULL
        ORDER BY o.created_at DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
        """;
}
```

---

## Parâmetros

Os parâmetros são passados via **objetos anônimos** para queries simples e via **classes de parâmetro** para queries com muitos parâmetros ou reutilizáveis.

```csharp
// Objeto anônimo — preferido para queries simples
var result = await Connection.QueryAsync<OrderSummaryModel>(
    OrderQueries.GetSummariesByCustomer,
    new { CustomerId = customerId });

// Classe de parâmetro — preferida para queries complexas ou reutilizáveis
var parameters = new GetPagedByStatusParams
{
    Status = status,
    Offset = (page - 1) * pageSize,
    PageSize = pageSize
};

var result = await Connection.QueryAsync<OrderSummaryModel>(
    OrderQueries.GetPagedByStatus,
    parameters);
```

---

## Mapeamento de Resultados

O Dapper mapeia os resultados automaticamente por nome de coluna. Os aliases das colunas no SQL devem corresponder exatamente às propriedades do modelo de destino.

```csharp
// Modelo de destino
public class OrderSummaryModel
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CustomerName { get; init; } = string.Empty;
}
```

---

## Convenções

- Dapper é sempre usado para leitura — operações de escrita usam EF Core, exceto em cenários de banco legado ou desnormalizado
- Nenhuma query SQL é escrita inline nos repositórios — todas as queries ficam em constantes no Domain em `Integrations/Sql/`
- O filtro de soft delete (`deleted_at IS NULL`) é sempre aplicado manualmente nas queries Dapper — o query filter global do EF Core não se aplica
- Aliases de colunas no SQL devem corresponder exatamente às propriedades do modelo de destino — `snake_case` no SQL, `PascalCase` no C#
- Parâmetros são sempre nomeados — nunca usar concatenação de string para montar queries
- A conexão é sempre obtida via `AppDbContext.Database.GetDbConnection()` — nunca instanciar `NpgsqlConnection` diretamente nos repositórios