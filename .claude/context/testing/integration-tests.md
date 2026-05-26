# Integration Tests

## Visão Geral

Os testes de integração validam o comportamento do sistema com suas dependências reais — banco de dados, repositórios e pipeline HTTP. Eles complementam os testes unitários cobrindo cenários que envolvem múltiplas camadas. O isolamento entre testes é garantido via **rollback de transação** ao final de cada teste.

-----

## Localização

```
[componente]/Tests/4 - Integration/
[componente]/[componente].Integration.Tests/
[componente]/[componente].Integration.Tests/Endpoints/
[componente]/[componente].Integration.Tests/Endpoints/Orders/
[componente]/[componente].Integration.Tests/Endpoints/Orders/CreateOrderEndpointTests.cs
[componente]/[componente].Integration.Tests/Fixtures/
[componente]/[componente].Integration.Tests/Fixtures/IntegrationTestFixture.cs
[componente]/[componente].Integration.Tests/Fixtures/DatabaseFixture.cs
```

-----

## Ferramentas

|Ferramenta                |Propósito                                           |
|--------------------------|----------------------------------------------------|
|xUnit                     |Framework de testes                                 |
|`WebApplicationFactory<T>`|Hospedagem da API em memória para testes de endpoint|
|Shouldly                  |Asserções                                           |
|EF Core                   |Acesso ao banco real com rollback de transação      |

-----

## Fixtures

### IntegrationTestFixture

Configura a `WebApplicationFactory` com o ambiente de testes, substituindo configurações de produção quando necessário.

```csharp
public class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public HttpClient Client { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // substituir configurações específicas para testes quando necessário
        });
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

### DatabaseFixture

Gerencia o ciclo de vida das transações, garantindo rollback ao final de cada teste.

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

-----

## Estrutura de um Teste de Endpoint

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

    public async Task DisposeAsync()
    {
        await _databaseFixture.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedAsync()
    {
        // Arrange
        var request = CreateOrderRequestMock.Valid();
        var token = await AuthHelper.GetTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
```

-----

## Helpers

Helpers de suporte para operações recorrentes nos testes de integração.

```
[componente]/[componente].Integration.Tests/Helpers/
[componente]/[componente].Integration.Tests/Helpers/AuthHelper.cs
```

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

-----

## Convenções

- Um arquivo de teste por endpoint ou cenário de integração
- Nome do arquivo segue o padrão `[Recurso]EndpointTests` — ex: `CreateOrderEndpointTests`
- Todos os testes seguem o padrão AAA com comentários `// Arrange`, `// Act`, `// Assert`
- Testes assíncronos retornam `Task` e incluem o sufixo `_Async`
- Asserções são feitas via **Shouldly** — nunca `Assert` nativo do xUnit
- O rollback de transação é sempre garantido via `DatabaseFixture` — nunca limpar tabelas manualmente
- Data Mocks são reutilizados dos projetos de testes unitários quando aplicável
- Testes de integração não substituem testes unitários — cobrem apenas cenários que exigem dependências reais
- O ambiente de testes é configurado via `UseEnvironment("Testing")` — nunca usar configurações de produção