# Unit Tests

## Visão Geral

Os testes unitários são escritos com **xUnit** e **Shouldly** para asserções. Eles cobrem todas as classes com regras de negócio, utilizando Data Mocks para construção de cenários e Mock Classes para substituição de dependências. A cobertura mínima exigida é de **85% dos cenários testáveis** por classe.

---

## Localização

```
[componente]/[componente].X.Tests/Tests/
[componente]/[componente].X.Tests/Tests/AppServices/
[componente]/[componente].X.Tests/Tests/AppServices/OrderAppServiceTests.cs
[componente]/[componente].X.Tests/Tests/Services/
[componente]/[componente].X.Tests/Tests/Services/OrderServiceTests.cs
[componente]/[componente].X.Tests/Tests/Validators/
[componente]/[componente].X.Tests/Tests/Validators/CreateOrderRequestValidatorTests.cs
```

---

## Padrão AAA

Todos os testes seguem o padrão **Arrange, Act, Assert**, com as seções separadas por comentários.

```csharp
[Fact]
public async Task CreateAsync_ValidRequest_ReturnsSuccessAsync()
{
    // Arrange
    var request = CreateOrderRequestMock.Valid();
    var expectedResult = Result<OrderModel>.Success(OrderModelMock.Valid());

    var orderService = new OrderServiceMock()
        .SetupCreateAsync(It.IsAny<CreateOrderModel>(), expectedResult)
        .Build();

    var appService = new OrderAppService(orderService, mapper);

    // Act
    var result = await appService.CreateAsync(request, CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldNotBeNull();
}
```

---

## Nomenclatura

Os testes seguem o padrão `MétodoASerTestado_Cenário_ComportamentoEsperado`. Testes assíncronos incluem o sufixo `_Async`.

```
CreateAsync_ValidRequest_ReturnsSuccessAsync
CreateAsync_EmptyCustomerId_ReturnsValidationErrorAsync
GetById_OrderNotFound_ReturnsNotFoundResultAsync
Validate_ValidRequest_PassesValidation
Validate_EmptyCustomerId_FailsValidation
```

---

## Estrutura de uma Classe de Testes

```csharp
public class OrderAppServiceTests
{
    private readonly IMapper _mapper;

    public OrderAppServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>())
            .CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccessAsync()
    {
        // Arrange
        var request = CreateOrderRequestMock.Valid();
        var expectedResult = Result<OrderModel>.Success(OrderModelMock.Valid());

        var orderService = new OrderServiceMock()
            .SetupCreateAsync(It.IsAny<CreateOrderModel>(), expectedResult)
            .Build();

        var appService = new OrderAppService(orderService, _mapper);

        // Act
        var result = await appService.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.OrderId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_ServiceReturnsFailure_ReturnsFailureResultAsync()
    {
        // Arrange
        var request = CreateOrderRequestMock.Valid();
        var expectedResult = Result<OrderModel>.Failure(ResultCode.BusinessError, "Estoque insuficiente.", 422);

        var orderService = new OrderServiceMock()
            .SetupCreateAsync(It.IsAny<CreateOrderModel>(), expectedResult)
            .Build();

        var appService = new OrderAppService(orderService, _mapper);

        // Act
        var result = await appService.CreateAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.BusinessError);
        result.Message.ShouldBe("Estoque insuficiente.");
        result.StatusCode.ShouldBe(422);
    }
}
```

---

## Testes de Validator

```csharp
public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = CreateOrderRequestMock.Valid();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmptyCustomerId_FailsValidation()
    {
        // Arrange
        var request = CreateOrderRequestMock.WithEmptyCustomerId();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateOrderRequest.CustomerId));
    }
}
```

---

## Exclusão de Cobertura

Use `[ExcludeFromCodeCoverage]` para remover da análise classes que não têm lógica testável. O atributo pertence ao namespace `System.Diagnostics.CodeAnalysis`.

### Onde aplicar

| Tipo de artefato | Aplicar? |
|------------------|----------|
| `XDependency.cs` (classes de DI/registro) | ✅ Sim |
| Entidades (`BaseEntity` e subclasses) | ✅ Sim |
| Modelos, DTOs, Requests, Responses | ✅ Sim |
| Perfis AutoMapper (`Profile`) | ✅ Sim |
| Configurações EF Core (`IEntityTypeConfiguration<T>`) | ✅ Sim |
| `AppDbContext` | ✅ Sim |
| Endpoints estáticos de configuração de rota | ✅ Sim |
| `Program.cs` | ✅ Sim — via `partial class Program { }` no final do arquivo |
| Services, repositories, validators, app services, filtros | ❌ Não — têm lógica testável |

### Tipos C# onde o atributo é INVÁLIDO

- **`enum`** — o compilador rejeita com `CS0592`. Enums não precisam do atributo: o Coverlet não os instrumenta por padrão
- **Interfaces** — não são instrumentadas pelo Coverlet; o atributo é desnecessário

### Padrão para Program.cs

Top-level statements não aceitam atributos diretamente. Use a declaração de partial class ao final do arquivo:

```csharp
// final de Program.cs
namespace NomeDoProjeto.Api;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Program { }
```

---

## Convenções

- Um arquivo de testes por classe testada
- Nome do arquivo segue o padrão `[NomeDaClasse]Tests` — ex: `OrderAppServiceTests`, `CreateOrderRequestValidatorTests`
- Testes são organizados em subpastas por contexto: `AppServices/`, `Services/`, `Validators/`, `Repositories/`
- O padrão AAA é obrigatório em todos os testes, com comentários `// Arrange`, `// Act` e `// Assert`
- Testes assíncronos retornam `Task` e incluem o sufixo `_Async` no nome
- Asserções são feitas exclusivamente via **Shouldly** — nunca via `Assert` nativo do xUnit
- `CancellationToken.None` é sempre utilizado nos testes — nunca um token com cancelamento real
- Cada cenário é testado em um método isolado — nunca múltiplos cenários em um único teste
- A cobertura mínima exigida é de **85% dos cenários testáveis** por classe