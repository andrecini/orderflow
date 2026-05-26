# Validators

## Visão Geral

A validação de requests é feita com **FluentValidation**. Cada request possui seu próprio validator, que é executado automaticamente por um filtro global antes de o handler do endpoint ser invocado. Os endpoints não contêm lógica de validação diretamente.

---

## Localização

```
[componente]/[componente].Api/Validators/
[componente]/[componente].Api/Validators/Orders/
[componente]/[componente].Api/Validators/Orders/CreateOrderRequestValidator.cs
```

---

## Estrutura de um Validator

```csharp
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("O pedido deve conter ao menos um item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
```

---

## Convenções

- Um validator por request
- Nome do arquivo e da classe seguem o padrão `[NomeDoRequest]Validator` — ex: `CreateOrderRequestValidator`
- Validators são organizados em subpastas por recurso dentro de `Validators/`
- Nenhuma regra de negócio deve ser inserida no validator — apenas validações de formato, obrigatoriedade e integridade dos dados de entrada

---

## Execução via Filtro

A validação é interceptada por um filtro global localizado em `[componente]/[componente].Api/Filters/`. O filtro resolve o validator correspondente ao tipo do request via injeção de dependência e executa a validação antes do handler. Em caso de falha, retorna `400 Bad Request` no formato `ProblemDetails`.

```csharp
public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();

        if (request is null)
            return Results.BadRequest();

        var result = await validator.ValidateAsync(request);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
```

---

## Injeção de Dependência

O registro dos validators e do filtro está centralizado em `[componente]/[componente].Api/ApiDependency.cs`.