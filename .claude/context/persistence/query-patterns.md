# Query Patterns

## Visão Geral

O projeto utiliza dois mecanismos de acesso a dados relacionais — **Entity Framework Core** e **Dapper** — cada um com seu papel bem definido. Este arquivo orienta quando e como usar cada um, evitando uso inadequado que comprometa performance ou manutenibilidade.

---

## Quando usar EF Core vs Dapper

| Situação | EF Core | Dapper |
|----------|---------|--------|
| CRUD simples em entidades mapeadas | ✅ | ❌ |
| Queries com filtros simples via LINQ | ✅ | ❌ |
| Operações dentro de transações (Unit of Work) | ✅ | ✅ via mesma conexão |
| Leituras em massa com alta performance | ❌ | ✅ |
| Queries com múltiplos JOINs complexos | ❌ | ✅ |
| Projeções para modelos de leitura específicos | ❌ | ✅ |
| Controle total sobre o SQL gerado | ❌ | ✅ |
| Bancos legados ou desnormalizados | ❌ | ✅ |

---

## Padrões EF Core

### Busca por ID
```csharp
var order = await dbContext.Orders
    .FindAsync(id, cancellationToken);
```

### Busca com filtro simples
```csharp
var orders = await dbContext.Orders
    .Where(x => x.CustomerId == customerId)
    .ToListAsync(cancellationToken);
```

### Busca com projeção
```csharp
var summaries = await dbContext.Orders
    .Where(x => x.Status == OrderStatus.Pending)
    .Select(x => new OrderModel
    {
        Id = x.Id,
        Status = x.Status,
        CreatedAt = x.CreatedAt
    })
    .ToListAsync(cancellationToken);
```

### Paginação
```csharp
var orders = await dbContext.Orders
    .OrderByDescending(x => x.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);
```

### Verificação de existência
```csharp
var exists = await dbContext.Orders
    .AnyAsync(x => x.CustomerId == customerId, cancellationToken);
```

---

## Padrões Dapper

### Leitura simples
```csharp
var result = await Connection.QueryAsync<OrderSummaryModel>(
    OrderQueries.GetSummariesByCustomer,
    new { CustomerId = customerId });
```

### Leitura paginada
```csharp
var result = await Connection.QueryAsync<OrderSummaryModel>(
    OrderQueries.GetPagedByStatus,
    new { Status = status, Offset = (page - 1) * pageSize, PageSize = pageSize });
```

### Leitura de valor único
```csharp
var total = await Connection.QuerySingleOrDefaultAsync<decimal>(
    OrderQueries.GetTotalByCustomer,
    new { CustomerId = customerId });
```

### Múltiplos resultados com QueryMultiple
```csharp
using var multi = await Connection.QueryMultipleAsync(
    OrderQueries.GetOrderWithItems,
    new { OrderId = orderId });

var order = await multi.ReadSingleOrDefaultAsync<OrderModel>();
var items = (await multi.ReadAsync<OrderItemModel>()).ToList();
```

---

## Decisão por Tipo de Operação

```
Operação de escrita?
  → Sempre EF Core

Operação de leitura simples (por ID, filtro direto)?
  → EF Core

Operação de leitura com JOIN, agregação ou projeção específica?
  → Dapper

Leitura em massa ou com requisito de alta performance?
  → Dapper
```

---

## Convenções

- Nunca misturar EF Core e Dapper para a mesma operação — escolher um e seguir até o fim
- Queries Dapper nunca são escritas inline nos repositórios — sempre referenciar constantes de `[componente].Domain/Integrations/Sql/`
- O soft delete (`deleted_at IS NULL`) é aplicado automaticamente pelo EF Core via query filter global — no Dapper deve ser aplicado manualmente em todas as queries
- Projeções EF Core via `.Select()` são preferidas a carregar entidades completas quando apenas alguns campos são necessários
- `AsNoTracking()` é utilizado em todas as queries EF Core de leitura que não resultarão em atualização posterior
- Paginação é sempre baseada em `page` (índice iniciado em 1) e `pageSize` — tanto no EF Core quanto no Dapper
- Consulte `ef-standards.md` e `dapper-standards.md` para detalhes de implementação de cada mecanismo