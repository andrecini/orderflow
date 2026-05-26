---
name: create-endpoint
description: 'Use this skill when the user asks to create a single endpoint with its validator, AppService and Swagger documentation. Trigger for prompts like "create an endpoint", "add a route", "implement a POST endpoint for X". Do not trigger for full feature creation — use create-feature instead.'
allowed-tools: Read, Edit, Write, Glob, Grep, Bash(dotnet *)
---

## Guardrails

- **Escopo restrito à camada de Presentation** — nunca criar arquivos fora de `[componente].Api/`
- **Sem criação de Services ou Repositories** — responsabilidade das skills `create-service` e `create-repository`
- **Sem criação de migrations** — responsabilidade exclusiva da skill `create-migration`
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler ou alterar `appsettings.Production.json`
- **Sem alteração de `XDependency.cs` de outras camadas** — apenas `ApiDependency.cs`
- **Perguntar antes de sobrescrever** — se um arquivo já existir, nunca sobrescrever sem confirmação do usuário

# Skill: Create Endpoint

## MCP

### 1. Verificar arquivos existentes via Filesystem MCP

```
list_directory → src/[componente].Api/Endpoints/[Recurso]/
list_directory → src/[componente].Api/AppServices/
list_directory → src/[componente].Api/Validators/[Recurso]/
```
Para cada arquivo já existente, informar o usuário antes de atualizar.

### 2. Escrever arquivos via Filesystem MCP

```
write_file → src/[componente].Api/Endpoints/...
write_file → src/[componente].Api/AppServices/...
write_file → src/[componente].Api/DTOs/...
write_file → src/[componente].Api/Mappings/...
write_file → src/[componente].Api/Validators/...
```
---

## Objetivo

Guia a criação isolada de um endpoint com Minimal API, Validator, Filter, AppService e documentação Swagger, seguindo todos os padrões da camada de Presentation. Diferente do `create-feature`, não gera Service, Repository ou testes — foca exclusivamente na camada de API.

---

## Contextos Necessários

- [minimal-apis.md](../../context/development/minimal-apis.md)
- [validators.md](../../context/development/validators.md)
- [filters.md](../../context/development/filters.md)
- [app-services.md](../../context/development/app-services.md)
- [api-documentation.md](../../context/development/api-documentation.md)
- [auth.md](../../context/development/auth.md)
- [automapper-profiles.md](../../context/architecture/automapper-profiles.md)
- [layer-objects.md](../../context/architecture/layer-objects.md)
- [dependency-injection.md](../../context/development/dependency-injection.md)

---

## Entrada

O usuário deve fornecer as informações abaixo. Para cada item não informado, perguntar antes de gerar:

```
1. Recurso — ex: Order, Payment, Customer
2. Operação — ex: Create, Get, Update, Delete, List
3. Método HTTP — GET, POST, PUT, PATCH, DELETE
4. Rota — ex: /api/v1/orders (ou gerar automaticamente seguindo o padrão)
5. Requer autenticação? (sim/não) — se sim, qual policy? ex: orders:write
6. Quais campos o Request terá?
7. Quais campos o Response terá?
8. A Service correspondente já existe? (sim/não)
```

---

## Passos

### 1. Confirmar entradas
Perguntar sobre qualquer informação não fornecida antes de prosseguir.

### 2. Gerar DTOs
Seguindo [layer-objects.md](../../context/architecture/layer-objects.md):

- Criar `[Operação][Recurso]Request` em `[componente].Api/DTOs/Requests/`
- Criar `[Operação][Recurso]Response` em `[componente].Api/DTOs/Responses/`

```csharp
public class CreateOrderRequest
{
    public Guid CustomerId { get; init; }
    public List<OrderItemRequest> Items { get; init; } = [];
}

public class CreateOrderResponse
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
```

### 3. Gerar Validator
Seguindo [validators.md](../../context/development/validators.md):

- Criar `[Operação][Recurso]RequestValidator` em `[componente].Api/Validators/[Recurso]/`
- Cobrir todos os campos obrigatórios e regras de formato informadas

```csharp
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
    }
}
```

### 4. Gerar AppService
Seguindo [app-services.md](../../context/development/app-services.md):

- Criar interface `I[Recurso]AppService` em `[componente].Api/AppServices/Interfaces/`
  - Se a interface já existir, adicionar apenas o novo método
- Criar ou atualizar `[Recurso]AppService` em `[componente].Api/AppServices/`
- Mapear request → model via AutoMapper, invocar service, mapear result → response

```csharp
public class OrderAppService(I[Recurso]Service service, IMapper mapper) : IOrderAppService
{
    public async Task<CreateOrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var model = mapper.Map<CreateOrderModel>(request);
        var result = await service.CreateAsync(model, cancellationToken);
        return mapper.Map<CreateOrderResponse>(result.Value);
    }
}
```

### 5. Gerar profile AutoMapper
Seguindo [automapper-profiles.md](../../context/architecture/automapper-profiles.md):

- Adicionar mapeamentos em `[componente].Api/Mappings/[Recurso]Profile.cs`:
  - `[Operação][Recurso]Request` → `[Operação][Recurso]Model`
  - `[Recurso]Model` → `[Operação][Recurso]Response`
  - Se o profile já existir, adicionar apenas os novos mapeamentos

### 6. Gerar Endpoint
Seguindo [minimal-apis.md](../../context/development/minimal-apis.md):

- Criar `[Operação][Recurso]Endpoint` em `[componente].Api/Endpoints/[Recurso]/`
- Declarar `.WithName()`, `.WithSummary()`, `.WithTags()`, `.WithOpenApi()`
- Adicionar `.RequireAuthorization("policy-name")` se autenticado
- Adicionar `.AddEndpointFilter<ValidationFilter<[Operação][Recurso]Request>>()`
- Usar `TypedResults` para retorno

```csharp
public static class CreateOrderEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/orders", async (
            CreateOrderRequest request,
            IOrderAppService appService,
            CancellationToken cancellationToken) =>
        {
            var response = await appService.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/v1/orders/{response.OrderId}", response);
        })
        .WithName("CreateOrder")
        .WithSummary("Cria um novo pedido")
        .WithTags("Orders")
        .AddEndpointFilter<ValidationFilter<CreateOrderRequest>>()
        .RequireAuthorization("orders:write")
        .WithOpenApi();
    }
}
```

### 7. Registrar DI
Seguindo [dependency-injection.md](../../context/development/dependency-injection.md):

- Registrar `[Recurso]AppService` em `[componente].Api/ApiDependency.cs`
- Registrar profile AutoMapper em `AddAutoMapper()` na `ApiDependency.cs`
- Registrar validator em `AddValidatorsFromAssemblyContaining<T>()` se ainda não estiver registrado
- Registrar endpoint no método de mapeamento de rotas da `ApiDependency.cs`

---

## Output Esperado

```
[componente].Api/
├── AppServices/[Recurso]AppService.cs           — criado ou atualizado
├── AppServices/Interfaces/I[Recurso]AppService.cs — criado ou atualizado
├── DTOs/Requests/[Operação][Recurso]Request.cs  — criado
├── DTOs/Responses/[Operação][Recurso]Response.cs — criado
├── Endpoints/[Recurso]/[Operação][Recurso]Endpoint.cs — criado
├── Mappings/[Recurso]Profile.cs                 — criado ou atualizado
└── Validators/[Recurso]/[Operação][Recurso]RequestValidator.cs — criado
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] DTOs exclusivos da camada de Presentation — nunca reutilizados de outras camadas
- [ ] Validator cobre todos os campos obrigatórios informados
- [ ] AppService apenas mapeia e delega — sem regras de negócio
- [ ] AutoMapper usado em todos os mapeamentos entre Request/Response e Models
- [ ] `.WithName()`, `.WithSummary()`, `.WithTags()` e `.WithOpenApi()` declarados
- [ ] `.RequireAuthorization()` declarado se o endpoint for autenticado
- [ ] `TypedResults` usado no retorno — nunca `Results` diretamente
- [ ] `ValidationFilter` adicionado ao endpoint
- [ ] `CancellationToken` propagado na AppService e no endpoint
- [ ] Construtores primários em todas as classes com DI
- [ ] AppService, profile e validator registrados na `ApiDependency.cs`

---

## Prompt Examples

- "cria um endpoint POST de criação de pedido"
- "adiciona a rota GET /api/v1/products/{id}"
- "implementa o endpoint de cancelamento de pagamento"
- "quero um DELETE para remover clientes"
- "adiciona um endpoint autenticado de listagem de faturas"

---

## Related Skills

- `create-service` — criar a service correspondente se ainda não existir
- `create-unit-test` — gerar testes unitários para o endpoint e AppService criados
- `create-migration` — criar migration se uma nova entidade for necessária

---

## Error Handling

- **Service não encontrada** — se a interface da service correspondente não existir, alertar e sugerir execução da skill `create-service` antes de prosseguir
- **Rota duplicada** — se a rota informada já existir no projeto, alertar e perguntar se deseja criar uma nova versão ou substituir
- **Policy de autorização não cadastrada** — se a policy informada não estiver registrada na `ApiDependency.cs`, alertar e incluir o registro automaticamente
- **Campos de request ou response não informados** — perguntar antes de gerar DTOs genéricos