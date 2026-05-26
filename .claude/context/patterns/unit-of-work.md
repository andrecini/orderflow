# Pattern: Unit of Work

## Visão Geral

O Unit of Work é um padrão que agrupa um conjunto de operações de persistência em uma única transação, garantindo que todas sejam confirmadas ou revertidas juntas. Ele também atua como ponto central de acesso aos repositórios, evitando que as camadas superiores instanciem ou gerenciem repositórios diretamente.

O projeto adota duas variações — uma para bancos relacionais (SQL) e outra para bancos de documentos (NoSQL). As implementações serão especificadas em arquivos de contexto dedicados.

---

## Localização

```
[componente]/[componente].Domain/Result/
```

> As interfaces do Unit of Work residem no `[componente].Domain`. As implementações residem no `[componente].Infrastructure`.

---

## Interface Base — SQL

```csharp
public interface ISqlUnitOfWork : IAsyncDisposable
{
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
```

---

## Interface Base — NoSQL

```csharp
public interface INoSqlUnitOfWork : IAsyncDisposable
{
    IOrderDocumentRepository Orders { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
```

---

## Uso

```csharp
public class CreateOrderService(ISqlUnitOfWork unitOfWork, IMapper mapper)
{
    public async Task<Result> ExecuteAsync(CreateOrderModel model, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await unitOfWork.Customers.GetByIdAsync(model.CustomerId, cancellationToken);

            if (customer is null)
                return Result.Failure(ResultCode.NotFound, "Cliente não encontrado.", statusCode: 404);

            var order = mapper.Map<Order>(model);

            await unitOfWork.Orders.AddAsync(order, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return Result.Failure(ResultCode.InternalError, "Erro ao processar o pedido.", statusCode: 500);
        }
    }
}
```

---

## Convenções

- O Unit of Work é injetado nas services — nunca nos repositórios ou nas camadas de apresentação
- Repositórios não gerenciam transações diretamente — essa responsabilidade pertence exclusivamente ao Unit of Work
- `BeginTransactionAsync` deve ser chamado explicitamente antes de operações que exijam atomicidade
- `CommitAsync` e `RollbackAsync` são sempre chamados dentro de um bloco `try/catch`
- O Unit of Work implementa `IAsyncDisposable` — o ciclo de vida é gerenciado via injeção de dependência
- Operações de leitura simples que não fazem parte de uma transação podem usar os repositórios diretamente, sem necessidade do Unit of Work
- SQL e NoSQL possuem interfaces e implementações separadas — nunca devem ser combinados em um único Unit of Work