# Presentation Layer — [componente].Api

## Visão Geral

A camada de apresentação é o ponto de entrada da aplicação. Ela é responsável por receber as requisições HTTP, validar os dados de entrada, delegar o processamento para a camada de Application via AppServices e retornar as respostas ao cliente. Não contém regras de negócio.

---

## Localização

```
[componente]/[componente].Api/
```

---

## Componentes

### Endpoints
Implementados com Minimal APIs, organizados por recurso. Responsáveis por mapear rotas, declarar autorização e invocar a AppService correspondente. Consulte `minimal-apis.md`.

```
[componente]/[componente].Api/Endpoints/
```

### App Services
Camada intermediária entre os endpoints e os serviços de aplicação. Recebe o objeto de request, mapeia para o modelo da service, invoca a service e mapeia o resultado para o response. Consulte `app-services.md`.

```
[componente]/[componente].Api/AppServices/
[componente]/[componente].Api/AppServices/Interfaces/
```

### Validators
Validação de requests via FluentValidation, executada por um filtro global antes do handler do endpoint. Consulte `validators.md`.

```
[componente]/[componente].Api/Validators/
```

### Filters
Filtros globais aplicados ao pipeline dos endpoints. Incluem o filtro de validação de request. Consulte `validators.md`.

```
[componente]/[componente].Api/Filters/
```

### Middlewares
Middlewares do pipeline HTTP da aplicação. Incluem o tratamento global de exceções e o CorrelationId. Consulte `exception-handling.md` e `logging-standards.md`.

```
[componente]/[componente].Api/Middlewares/
```

### DTOs
Objetos de request e response exclusivos da camada de apresentação. Não são compartilhados com outras camadas.

```
[componente]/[componente].Api/DTOs/
[componente]/[componente].Api/DTOs/Requests/
[componente]/[componente].Api/DTOs/Requests/CreateOrderRequest.cs
[componente]/[componente].Api/DTOs/Responses/
[componente]/[componente].Api/DTOs/Responses/CreateOrderResponse.cs
```

### AutoMapper Profiles
Profiles responsáveis pelo mapeamento entre os DTOs da camada de apresentação e os modelos da camada de Application. Mapeamentos internos à própria camada também são permitidos quando necessário.

```
[componente]/[componente].Api/Mappings/
[componente]/[componente].Api/Mappings/OrderProfile.cs
```

Exemplo de profile:

```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<CreateOrderRequest, CreateOrderModel>();
        CreateMap<OrderModel, CreateOrderResponse>();
    }
}
```

### Dependency Injection
Registro de todos os componentes da camada via método de extensão. Consulte `dependency-injection.md`.

```
[componente]/[componente].Api/ApiDependency.cs
```

---

## Estrutura Completa

```
[componente]/[componente].Api/
├── AppServices/
│   ├── Interfaces/
│   └── OrderAppService.cs
├── DTOs/
│   ├── Requests/
│   └── Responses/
├── Endpoints/
│   └── [Resource]/
├── Filters/
├── Mappings/
├── Middlewares/
├── Validators/
│   └── [Resource]/
├── ApiDependency.cs
├── appsettings.json
│   └── appsettings.[Environment].json
└── Program.cs
```

---

## Convenções

- Nenhuma regra de negócio deve residir nessa camada
- DTOs de request e response são exclusivos da camada de apresentação — nunca reutilizados em outras camadas
- Todo mapeamento entre DTOs e modelos é feito via AutoMapper — nunca manualmente nas AppServices ou endpoints
- A camada de apresentação depende da camada de Application via abstrações — nunca referencia implementações diretamente