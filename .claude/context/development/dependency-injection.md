# Dependency Injection

## Visão Geral

A injeção de dependência é configurada via `IServiceCollection` com métodos de extensão. Cada módulo da solução possui sua própria classe `XDependency.cs` responsável por registrar seus próprios serviços. O `Program.cs` chama cada `XDependency` diretamente, sem classe orquestradora intermediária.

---

## Localização

```
[componente]/[componente].Api/ApiDependency.cs
[componente]/[componente].Application/ApplicationDependency.cs
[componente]/[componente].Infrastructure/InfrastructureDependency.cs
```

---

## Estrutura de um XDependency

Cada `XDependency` expõe um método de extensão estático sobre `IServiceCollection`.

```csharp
public static class ApiDependency
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        //Exemplo
        // Adiciona deps de AppServices
        // Adiciona deps de Filters
        // Adiciona deps de Validators
        // Adiciona deps de mappings, etc

        return services;
    }
}
```

---

## Registro no Program.cs

```csharp
builder.Services
    .AddDomain(builder.Configuration)
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);
```

---

## Lifetimes

O lifetime de cada serviço é decidido conforme a natureza do componente. A tabela abaixo descreve as regras gerais adotadas:

| Componente | Lifetime | Justificativa |
|------------|----------|---------------|
| App Services | `Scoped` | Ciclo de vida por request |
| Services (Application) | `Scoped` | Ciclo de vida por request |
| Repositories | `Scoped` | Compartilham o contexto de banco por request |
| Unit of Work | `Scoped` | Deve compartilhar o mesmo contexto dos repositórios |
| Validators (FluentValidation) | `Scoped` | Ciclo de vida por request |
| HttpClient (IHttpClientFactory) | `Singleton` | Gerenciado pelo factory — evita socket exhaustion |
| Clientes AWS (AWSSDK) | `Singleton` | Clientes thread-safe e custosos para instanciar |
| Producers Kafka/RabbitMQ | `Singleton` | Conexões de longa duração reutilizadas entre requests |
| Consumers Kafka/RabbitMQ | `Singleton` | Registrados como BackgroundService |
| IMemoryCache | `Singleton` | Cache compartilhado entre requests |
| Políticas Polly | `Singleton` | Stateful — circuit breaker mantém estado entre chamadas |

> O agente deve avaliar o contexto de cada serviço antes de definir o lifetime. A tabela acima é uma referência — casos específicos podem exigir lifetimes diferentes.

---

## Convenções

- Cada `XDependency` registra apenas os serviços do seu próprio módulo — nunca de outros módulos
- `IConfiguration` é sempre recebido como parâmetro quando o registro depende de configurações externas (`appsettings`)
- Serviços com lifetime `Scoped` nunca são injetados em serviços com lifetime `Singleton` — isso causaria captive dependency
- O registro de `HttpClient` é sempre feito via `AddHttpClient<TClient>()` com as políticas de resiliência encadeadas
- Validators do FluentValidation são registrados via `AddValidatorsFromAssemblyContaining<T>()` para evitar registro manual unitário
- Consumers de mensageria são registrados via `AddHostedService<T>()`