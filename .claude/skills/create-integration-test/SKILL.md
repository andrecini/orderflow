---
name: create-integration-test
description: 'Use this skill when the user asks to create integration tests for an endpoint or between internal layers. Trigger for prompts like "create integration tests for X", "add integration tests to this endpoint", "test the service with the real database". Do not trigger for unit test creation — use create-unit-test instead.'
allowed-tools: Read, Edit, Write, Glob, Grep, Bash(dotnet test *)
---

## Guardrails

- **Escopo restrito ao projeto de testes de integração** — nunca criar arquivos fora de `[componente].Integration.Tests/`
- **Sem alteração de código de produção** — apenas leitura para identificar cenários
- **Sem criação de testes unitários** — responsabilidade exclusiva da skill `create-unit-test`
- **Sem acesso a bancos de produção** — apenas ambientes de desenvolvimento e staging
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler ou alterar `appsettings.Production.json`
- **Rollback obrigatório** — sempre usar `DatabaseFixture` para garantir rollback; nunca limpar tabelas manualmente
- **Perguntar antes de sobrescrever** — se fixture ou classe de teste já existir, nunca sobrescrever sem confirmação

# Skill: Create Integration Test

## MCP

### 1. Verificar arquivos existentes via Filesystem MCP

Antes de criar qualquer artefato, verificar quais arquivos já existem:

```
list_directory → src/[componente].Domain/Integrations/[Tipo]/
list_directory → src/[componente].Infrastructure/Integrations/[Tipo]/
```

### 2. Escrever arquivos via Filesystem MCP

```
write_file → src/[componente].Domain/Integrations/...
write_file → src/[componente].Infrastructure/Integrations/...
write_file → src/Tests/...
```

---

## Objetivo

Guia a criação completa de testes de integração — cobrindo endpoints HTTP ou integração entre camadas internas (ex: Service → Repository). Gera fixtures, helpers, Data Mocks e classes de teste seguindo os padrões de `integration-tests.md`. Data Mocks e helpers são criados automaticamente se não existirem.

---

## Contextos Necessários

- [integration-tests.md](../../context/testing/integration-tests.md)
- [test-architecture.md](../../context/testing/test-architecture.md)
- [data-mocks.md](../../context/testing/data-mocks.md)
- [unit-tests.md](../../context/testing/unit-tests.md)
- [auth.md](../../context/development/auth.md)
- [layer-objects.md](../../context/architecture/layer-objects.md)
- [result-pattern.md](../../context/patterns/result-pattern.md)

---

## Entrada

O usuário deve fornecer:

- **Tipo de teste** — se não informado, perguntar:

```
Qual o tipo de teste de integração?
1. Endpoint HTTP — testa o pipeline completo via WebApplicationFactory
2. Camadas internas — testa a integração entre Service e Repository com banco real
```

- **Recurso e operação** — ex: `Order` / `Create`, ou `OrderService.CreateAsync`
- **Cenários** — se não informados, a skill identifica automaticamente a partir do artefato

---

## Passos

### 1. Confirmar entradas
Se tipo, recurso ou operação não foram informados, perguntar antes de prosseguir.

### 2. Verificar e gerar fixtures

Verificar se as fixtures já existem em `[componente].Integration.Tests/Fixtures/`:

#### IntegrationTestFixture
Se não existir, criar:

```csharp
public class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public HttpClient Client { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        Client = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        Client.Dispose();
    }
}
```

#### DatabaseFixture
Se não existir, criar:

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction _transaction = null!;

    public DatabaseFixture(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
    }
}
```

### 3. Verificar e gerar AuthHelper

Se o teste for de endpoint autenticado e `AuthHelper` não existir em `[componente].Integration.Tests/Helpers/`, criar:

```csharp
public static class AuthHelper
{
    public static async Task<string> GetTokenAsync(HttpClient client)
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("service-user:password"));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        var response = await client.PostAsync("/api/v1/authenticate", null);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        return body!.Token;
    }
}
```

### 4. Verificar e gerar Data Mocks

Para cada objeto de entrada e saída necessário nos testes:

- Verificar se existe em `[componente].Integration.Tests/DataMocks/`
- Se não existir, criar seguindo [data-mocks.md](../../context/testing/data-mocks.md)
- Garantir que o método `Valid()` existe em todo Data Mock

### 5. Mapear cenários testáveis

Para cada operação, mapear:

- **Endpoint HTTP:** sucesso (2xx), request inválido (400), não autenticado (401), não encontrado (404), erro interno (500)
- **Camadas internas:** sucesso, falha de validação, recurso não encontrado, erro de persistência

### 6. Gerar classe de testes

#### Teste de Endpoint HTTP

```csharp
public class CreateOrderEndpointTests(IntegrationTestFixture fixture)
    : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly HttpClient _client = fixture.Client;
    private DatabaseFixture _databaseFixture = null!;

    public async Task InitializeAsync()
    {
        var dbContext = fixture.Services.GetRequiredService<AppDbContext>();
        _databaseFixture = new DatabaseFixture(dbContext);
        await _databaseFixture.InitializeAsync();
    }

    public async Task DisposeAsync() => await _databaseFixture.DisposeAsync();

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAsync()
    {
        // Arrange
        var request = CreateOrderRequestMock.Valid();
        var token = await AuthHelper.GetTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
        body.ShouldNotBeNull();
        body.OrderId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ReturnsBadRequestAsync()
    {
        // Arrange
        var request = CreateOrderRequestMock.WithEmptyCustomerId();
        var token = await AuthHelper.GetTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
```

#### Teste de Camadas Internas

```csharp
public class OrderServiceIntegrationTests : IAsyncLifetime
{
    private readonly AppDbContext _dbContext;
    private readonly IOrderService _service;
    private DatabaseFixture _databaseFixture = null!;

    public OrderServiceIntegrationTests()
    {
        // configurar DbContext apontando para banco de testes
    }

    public async Task InitializeAsync()
    {
        _databaseFixture = new DatabaseFixture(_dbContext);
        await _databaseFixture.InitializeAsync();
    }

    public async Task DisposeAsync() => await _databaseFixture.DisposeAsync();

    [Fact]
    public async Task CreateAsync_ValidModel_PersistsOrderAsync()
    {
        // Arrange
        var model = CreateOrderModelMock.Valid();

        // Act
        var result = await _service.CreateAsync(model, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.OrderId.ShouldNotBe(Guid.Empty);
    }
}
```

---

## Output Esperado

```
[componente].Integration.Tests/
├── Fixtures/
│   ├── IntegrationTestFixture.cs   — criado ou existente
│   └── DatabaseFixture.cs          — criado ou existente
├── Helpers/
│   └── AuthHelper.cs               — criado ou existente
├── DataMocks/
│   └── [Tipo]/[NomeDoObjeto]Mock.cs — criado se necessário
└── Tests/
    └── [Tipo]/[NomeDoArtefato]Tests.cs — criado
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] Fixtures geradas ou verificadas antes dos testes
- [ ] `AuthHelper` gerado se endpoint autenticado e não existir
- [ ] Data Mocks criados no projeto de integração se não existirem
- [ ] Método `Valid()` presente em todos os Data Mocks
- [ ] Padrão AAA com comentários `// Arrange`, `// Act`, `// Assert`
- [ ] Nomenclatura `MétodoASerTestado_Cenário_ComportamentoEsperado`
- [ ] Sufixo `_Async` em testes assíncronos
- [ ] Asserções via **Shouldly** — nunca `Assert` nativo do xUnit
- [ ] `CancellationToken.None` em todas as operações assíncronas
- [ ] Rollback garantido via `DatabaseFixture` — nunca limpeza manual
- [ ] Ambiente de testes configurado via `UseEnvironment("Testing")`

---

## Prompt Examples

- "cria testes de integração para o endpoint de pedidos"
- "adiciona testes de integração para o OrderService com banco real"
- "quero testar o fluxo completo de criação de pagamento"
- "gera os testes de integração para o GET /api/v1/orders"
- "testa a integração entre OrderService e OrderRepository"

---

## Related Skills

- `create-unit-test` — gerar testes unitários complementares para as classes envolvidas
- `check-coverage` — verificar cobertura após geração dos testes

---

## Error Handling

- **`WebApplicationFactory` não configurada** — criar a fixture automaticamente se não existir, informando o usuário
- **Banco de dados inacessível** — alertar que o banco configurado no ambiente de testes não está acessível e orientar sobre a configuração necessária
- **Endpoint não encontrado** — se o endpoint informado não existir no projeto, alertar e sugerir execução da skill `create-endpoint` antes de prosseguir
- **`AuthHelper` ausente em teste autenticado** — criar automaticamente se não existir, informando o usuário