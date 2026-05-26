# Pattern: Result

## Visão Geral

O Result Pattern é adotado para representar o desfecho de operações sem lançar exceções para regras de negócio. Ele torna o fluxo de sucesso e falha explícito no contrato dos métodos, forçando as camadas superiores a tratarem ambos os casos. É utilizado em todas as camadas da aplicação — da infraestrutura até a apresentação.

---

## Localização

```
[componente]/[componente].Domain/Result/
[componente]/[componente].Domain/Result/Result.cs
[componente]/[componente].Domain/Result/Result{T}.cs
[componente]/[componente].Domain/Result/ResultCode.cs
```

---

## ResultCode

Enum que representa os possíveis desfechos de uma operação, cobrindo tanto cenários de sucesso quanto de falha.

```csharp
public enum ResultCode
{
    Success,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    ValidationError,
    BusinessError,
    InternalError
}
```

---

## Result (sem valor de retorno)

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ResultCode Code { get; }
    public string? Message { get; }
    public int? StatusCode { get; }

    protected Result(bool isSuccess, ResultCode code, string? message, int? statusCode)
    {
        IsSuccess = isSuccess;
        Code = code;
        Message = message;
        StatusCode = statusCode;
    }

    public static Result Success() =>
        new(true, ResultCode.Success, null, null);

    public static Result Failure(ResultCode code, string message, int? statusCode = null) =>
        new(false, code, message, statusCode);
}
```

---

## Result\<T\> (com valor de retorno)

```csharp
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, ResultCode code, string? message, int? statusCode, T? value)
        : base(isSuccess, code, message, statusCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) =>
        new(true, ResultCode.Success, null, null, value);

    public static new Result<T> Failure(ResultCode code, string message, int? statusCode = null) =>
        new(false, code, message, statusCode, default);
}
```

---

## Uso nas Camadas

### Infraestrutura / Repositório
```csharp
public async Task<Result<OrderEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
{
    var order = await dbContext.Orders.FindAsync(id, cancellationToken);

    if (order is null)
        return Result<OrderEntity>.Failure(ResultCode.NotFound, $"Pedido {id} não encontrado.", statusCode: 404);

    return Result<OrderEntity>.Success(order);
}
```

### Service
```csharp
public async Task<Result<OrderModel>> ProcessAsync(CreateOrderModel model, CancellationToken cancellationToken)
{
    var result = await orderRepository.GetByIdAsync(model.OrderId, cancellationToken);

    if (result.IsFailure)
        return Result<OrderModel>.Failure(result.Code, result.Message!, result.StatusCode);

    // regras de negócio

    return Result<OrderModel>.Success(mapper.Map<OrderModel>(result.Value));
}
```

### AppService / Presentation
```csharp
public async Task<IResult> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
{
    var result = await orderService.ProcessAsync(mapper.Map<CreateOrderModel>(request), cancellationToken);

    if (result.IsFailure)
        return Results.Problem(result.Message, statusCode: result.StatusCode ?? 500);

    return Results.Ok(mapper.Map<CreateOrderResponse>(result.Value));
}
```

---

## Convenções

- Exceções são reservadas para erros inesperados — nunca para regras de negócio
- Falhas de negócio sempre retornam `Result.Failure` com um `ResultCode` e mensagem descritiva
- `StatusCode` é opcional — deve ser informado quando o resultado for consumido diretamente pela camada de apresentação
- O `Value` de um `Result<T>` só deve ser acessado após confirmar `IsSuccess`
- A propagação de falhas entre camadas é feita repassando `Code`, `Message` e `StatusCode` do resultado recebido