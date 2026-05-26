# Mock Classes

## Visão Geral

As Mock Classes abstraem a configuração de dependências nos testes unitários utilizando **Moq**. Elas herdam de uma classe base genérica `BaseMock<T>` e expõem métodos de setup encadeáveis seguindo o pattern Builder, tornando a configuração de cenários reutilizável e legível entre diferentes suítes de teste.

---

## Localização

```
[componente]/[componente].X.Tests/Mocks/
[componente]/[componente].X.Tests/Mocks/BaseMock.cs
[componente]/[componente].X.Tests/Mocks/AppServices/
[componente]/[componente].X.Tests/Mocks/AppServices/OrderAppServiceMock.cs
[componente]/[componente].X.Tests/Mocks/Services/
[componente]/[componente].X.Tests/Mocks/Services/OrderServiceMock.cs
[componente]/[componente].X.Tests/Mocks/Integrations/
[componente]/[componente].X.Tests/Mocks/Integrations/PaymentGatewayClientMock.cs
[componente]/[componente].X.Tests/Mocks/Repositories/
[componente]/[componente].X.Tests/Mocks/Repositories/OrderRepositoryMock.cs
```

---

## BaseMock

Classe base genérica que encapsula o `Mock<T>` do Moq e expõe o método `Build()` como ponto terminal da construção.

```csharp
public class BaseMock<T> where T : class
{
    protected Mock<T> _mock = new();

    public T Build() => _mock.Object;
}
```

---

## Estrutura de uma Mock Class

Cada mock concreto herda de `BaseMock<T>` e expõe métodos de setup nomeados pelo método que configuram. Cada método retorna a própria instância para permitir encadeamento fluente.

```csharp
public class OrderServiceMock : BaseMock<IOrderService>
{
    public OrderServiceMock SetupCreateAsync(CreateOrderModel model, Result<OrderModel> returnValue)
    {
        _mock.Setup(x => x.CreateAsync(model, It.IsAny<CancellationToken>()))
             .ReturnsAsync(returnValue);

        return this;
    }

    public OrderServiceMock SetupGetByIdAsync(Guid orderId, Result<OrderModel> returnValue)
    {
        _mock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
             .ReturnsAsync(returnValue);

        return this;
    }
}
```

---

## Uso nos Testes

```csharp
public class OrderAppServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenOrderIsValid()
    {
        var model = CreateOrderModelMock.Valid();
        var expectedResult = Result<OrderModel>.Success(OrderModelMock.Valid());

        var orderService = new OrderServiceMock()
            .SetupCreateAsync(model, expectedResult)
            .Build();

        var appService = new OrderAppService(orderService, mapper);

        var result = await appService.CreateAsync(CreateOrderRequestMock.Valid(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
```

---

## Convenções

- Um arquivo de mock por interface ou classe mockada
- Nome do arquivo e da classe seguem o padrão `[NomeDoTipo]Mock` — ex: `OrderServiceMock`, `OrderRepositoryMock`
- Métodos de setup são nomeados pelo método que configuram — ex: `SetupCreateAsync`, `SetupGetByIdAsync`
- Cada método de setup retorna `this` para permitir encadeamento fluente
- `Build()` é sempre o método terminal — nunca acessar `_mock.Object` diretamente fora da classe base
- `It.IsAny<CancellationToken>()` é sempre utilizado para `CancellationToken` nos setups — nunca um valor fixo
- Mocks são organizados em subpastas por contexto: `AppServices/`, `Services/`, `Repositories/`, `Integrations/`, `Messaging/`
- Verificações de chamada (`_mock.Verify`) quando necessárias devem ser expostas via métodos `Verify*` na própria mock class, mantendo o encapsulamento do `_mock`

```csharp
public OrderServiceMock VerifyCreateAsyncCalled(Times times)
{
    _mock.Verify(x => x.CreateAsync(It.IsAny<CreateOrderModel>(), It.IsAny<CancellationToken>()), times);
    return this;
}
```