---
name: create-service
description: 'Use this skill when the user asks to create a service for a specific resource or operation. Trigger for prompts like "create a service for X", "implement the business logic for Y", "add a service method for Z". Do not trigger for full feature creation — use create-feature instead.'
allowed-tools: Read, Edit, Write, Glob, Grep
---

## Guardrails

- **Escopo restrito às camadas de Domain e Application** — interface em `[componente].Domain/Interfaces/Services/` e implementação em `[componente].Application/Services/`
- **Sem criação de endpoints ou AppServices** — responsabilidade das skills `create-feature` ou `create-endpoint`
- **Sem acesso direto a repositórios fora da injeção de dependência** — sempre via interface injetada
- **Sem lançamento de exceções de negócio** — sempre retornar `Result<T>` ou `Result`
- **Sem alteração de `XDependency.cs` de outras camadas** — apenas `ApplicationDependency.cs`
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler ou alterar `appsettings.Production.json`
- **Perguntar antes de sobrescrever** — nunca sobrescrever métodos existentes sem confirmação do usuário

# Skill: Create Service

## MCP

### 1. Verificar arquivos existentes via Filesystem MCP

```
read_file → src/[componente].Domain/Interfaces/Services/I[Recurso]Service.cs
read_file → src/[componente].Application/Services/[Recurso]/[Recurso]Service.cs
list_directory → src/[componente].Domain/Interfaces/Repositories/
```

### 2. Escrever arquivos via Filesystem MCP

```
write_file → src/[componente].Domain/Interfaces/Services/...
write_file → src/[componente].Domain/Models/...
write_file → src/[componente].Domain/Mappings/...
write_file → src/[componente].Application/Services/...
write_file → src/Tests/.../Services/...
```

---

## Objetivo

Guia a criação isolada de uma service — interface no Domain e implementação no Application — seguindo o Result Pattern. Agrega novos métodos a interfaces existentes sem recriar o que já existe, verifica dependências necessárias e gera os testes unitários correspondentes.

---

## Contextos Necessários

- [layer-application.md](../../context/architecture/layer-application.md)
- [layer-domain.md](../../context/architecture/layer-domain.md)
- [layer-objects.md](../../context/architecture/layer-objects.md)
- [result-pattern.md](../../context/patterns/result-pattern.md)
- [automapper-profiles.md](../../context/architecture/automapper-profiles.md)
- [dependency-injection.md](../../context/development/dependency-injection.md)
- [unit-tests.md](../../context/testing/unit-tests.md)
- [mock-classes.md](../../context/testing/mock-classes.md)
- [data-mocks.md](../../context/testing/data-mocks.md)

---

## Entrada

O usuário deve fornecer:

- **Recurso** — ex: `Order`, `Payment`, `Customer`
- **Operação** — ex: `Create`, `GetById`, `Update`, `Delete`, `List`. Se não informada, perguntar:

```
Qual operação a service deve implementar?
1. Create
2. GetById
3. Update
4. Delete
5. List / GetPaged
6. Customizada — descrever a operação
```

- **Regras de negócio** — descrição das validações e lógica que a service deve aplicar. Se não informadas, perguntar:

```
Quais regras de negócio essa operação deve aplicar?
Ex: validar estoque, verificar duplicidade, calcular total
(Informe ou deixe em branco para gerar apenas o fluxo base)
```

---

## Passos

### 1. Confirmar entradas
Se recurso ou operação não foram informados, perguntar antes de prosseguir.

### 2. Verificar dependências

Verificar se os seguintes artefatos já existem:

- **Interface do repositório** — `I[Recurso]Repository` em `[componente].Domain/Interfaces/Repositories/`
- **Implementação do repositório** — `[Recurso]Repository` em `[componente].Infrastructure/Repositories/`
- **Models** — `[Operação][Recurso]Model` e `[Recurso]Model` em `[componente].Domain/Models/[Recurso]/`

Se o repositório não existir, alertar o usuário:

```
⚠️ O repositório I[Recurso]Repository não foi encontrado.
A service depende do repositório para persistência.
Deseja criá-lo antes de prosseguir?
1. Sim — executar skill create-repository primeiro
2. Não — gerar a service com a dependência referenciada (repositório será criado posteriormente)
```

### 3. Gerar ou atualizar Models no Domain

Verificar se os models necessários existem em `[componente].Domain/Models/[Recurso]/`:

- Se não existirem, criar `[Operação][Recurso]Model` e `[Recurso]Model`
- Se existirem, verificar se precisam de novos campos para a operação

```csharp
public class CreateOrderModel
{
    public Guid CustomerId { get; init; }
    public List<OrderItemModel> Items { get; init; } = [];
}

public class OrderModel
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
```

### 4. Gerar ou atualizar interface no Domain

Verificar se `I[Recurso]Service` existe em `[componente].Domain/Interfaces/Services/`:

- Se **não existir** → criar a interface com o novo método
- Se **já existir** → adicionar apenas o novo método, preservando os existentes

```csharp
public interface IOrderService
{
    Task<Result<OrderModel>> CreateAsync(CreateOrderModel model, CancellationToken cancellationToken);
}
```

### 5. Gerar ou atualizar implementação no Application

Verificar se `[Recurso]Service` existe em `[componente].Application/Services/[Recurso]/`:

- Se **não existir** → criar a classe implementando a interface
- Se **já existir** → adicionar apenas o novo método, preservando os existentes

```csharp
public class OrderService(
    IOrderRepository orderRepository,
    IMapper mapper) : IOrderService
{
    public async Task<Result<OrderModel>> CreateAsync(
        CreateOrderModel model,
        CancellationToken cancellationToken)
    {
        var existingOrder = await orderRepository.FindAsync(
            x => x.CustomerId == model.CustomerId, cancellationToken);

        if (existingOrder.Any())
            return Result<OrderModel>.Failure(
                ResultCode.Conflict,
                "Já existe um pedido em aberto para este cliente.",
                statusCode: 409);

        var order = mapper.Map<Order>(model);

        await orderRepository.AddAsync(order, cancellationToken);

        return Result<OrderModel>.Success(mapper.Map<OrderModel>(order));
    }
}
```

### 6. Verificar profile AutoMapper no Domain

Verificar se os mapeamentos necessários existem em `[componente].Domain/Mappings/[Recurso]Profile.cs`:

- `[Operação][Recurso]Model` → `[Recurso]` (Entity)
- `[Recurso]` (Entity) → `[Recurso]Model`

Se não existirem, adicionar ao profile existente ou criar um novo — consulte [automapper-profiles.md](../../context/architecture/automapper-profiles.md).

### 7. Registrar DI

Verificar se a service já está registrada em `[componente].Application/ApplicationDependency.cs`:

- Se não estiver, adicionar:

```csharp
services.AddScoped<IOrderService, OrderService>();
```

### 8. Gerar testes unitários

Seguindo [unit-tests.md](../../context/testing/unit-tests.md), [mock-classes.md](../../context/testing/mock-classes.md) e [data-mocks.md](../../context/testing/data-mocks.md):

- **Data Mocks** em `[componente].Application.Tests/DataMocks/`
  - `[Operação][Recurso]ModelMock` com `Valid()` e cenários de falha
  - `[Recurso]ModelMock` com `Valid()`
- **Mock Classes** em `[componente].Application.Tests/Mocks/Repositories/`
  - `[Recurso]RepositoryMock` com setup dos métodos utilizados — criado ou atualizado
- **Testes** em `[componente].Application.Tests/Tests/Services/`

```csharp
public class OrderServiceTests
{
    private readonly IMapper _mapper;

    public OrderServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>())
            .CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_ValidModel_ReturnsSuccessAsync()
    {
        // Arrange
        var model = CreateOrderModelMock.Valid();
        var order = OrderMock.Valid();

        var repository = new OrderRepositoryMock()
            .SetupFindAsync([], result: Enumerable.Empty<Order>())
            .SetupAddAsync()
            .Build();

        var service = new OrderService(repository, _mapper);

        // Act
        var result = await service.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_DuplicateOrder_ReturnsConflictAsync()
    {
        // Arrange
        var model = CreateOrderModelMock.Valid();

        var repository = new OrderRepositoryMock()
            .SetupFindAsync(model.CustomerId, result: new[] { OrderMock.Valid() })
            .Build();

        var service = new OrderService(repository, _mapper);

        // Act
        var result = await service.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Code.ShouldBe(ResultCode.Conflict);
        result.StatusCode.ShouldBe(409);
    }
}
```

---

## Output Esperado

```
[componente].Domain/
├── Interfaces/Services/I[Recurso]Service.cs     — criado ou atualizado
├── Mappings/[Recurso]Profile.cs                 — criado ou atualizado
└── Models/[Recurso]/                            — criado ou atualizado

[componente].Application/
└── Services/[Recurso]/[Recurso]Service.cs       — criado ou atualizado

[componente].Application.Tests/
├── DataMocks/Models/[Operação][Recurso]ModelMock.cs — criado ou atualizado
├── DataMocks/Models/[Recurso]ModelMock.cs           — criado ou atualizado
├── Mocks/Repositories/[Recurso]RepositoryMock.cs    — criado ou atualizado
└── Tests/Services/[Recurso]ServiceTests.cs          — criado ou atualizado
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] Interface criada ou atualizada no Domain — métodos existentes preservados
- [ ] Implementação retorna sempre `Result<T>` ou `Result` — nunca lança exceções de negócio
- [ ] Regras de negócio aplicadas antes da persistência
- [ ] AutoMapper usado para todos os mapeamentos entre Models e Entities
- [ ] `CancellationToken` propagado em todas as operações assíncronas
- [ ] Construtor primário utilizado
- [ ] Repositório verificado — usuário alertado se não existir
- [ ] Service registrada na `ApplicationDependency.cs`
- [ ] Profile AutoMapper verificado e atualizado se necessário
- [ ] Data Mocks possuem método `Valid()` obrigatório
- [ ] Testes cobrem ao menos 85% dos cenários testáveis
- [ ] Mock Classes existentes atualizadas — nunca duplicadas

---

## Prompt Examples

- "cria a service de pedidos"
- "implementa a lógica de negócio para criação de pagamento"
- "adiciona o método de cancelamento na OrderService"
- "quero uma service para validar estoque antes de criar o pedido"
- "cria o caso de uso de aprovação de crédito"

---

## Related Skills

- `create-repository` — criar o repositório necessário se não existir
- `create-endpoint` — criar o endpoint que consumirá a service
- `create-unit-test` — gerar testes unitários para a service criada

---

## Error Handling

- **Repository não encontrado** — alertar o usuário e perguntar se deseja executar `create-repository` antes de prosseguir ou gerar a service com a dependência referenciada
- **Interface já existente** — nunca recriar; apenas adicionar o novo método preservando os existentes
- **Regras de negócio não informadas** — gerar apenas o fluxo base e alertar que as regras de negócio devem ser implementadas manualmente ou informadas para uma nova iteração
- **Conflito de método** — se o método já existir na interface ou implementação, alertar e perguntar se deseja substituir ou criar uma variação