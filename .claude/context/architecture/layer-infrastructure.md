# Infrastructure Layer — [componente].Infrastructure

## Visão Geral

A camada de infraestrutura é responsável por implementar os contratos definidos no Domain. Ela gerencia o acesso a dados, integrações com serviços externos e mensageria. Não contém regras de negócio — apenas detalhes técnicos de persistência e comunicação.

---

## Localização

```
[componente]/[componente].Infrastructure/
```

---

## Componentes

### Repositories
Implementações dos contratos de repositório definidos no Domain. Utilizam Entity Framework Core para operações ORM e Dapper para queries customizadas. Consulte `generic-repository.md` e `unit-of-work.md`.

```
[componente]/[componente].Infrastructure/Repositories/
[componente]/[componente].Infrastructure/Repositories/Orders/
[componente]/[componente].Infrastructure/Repositories/Orders/OrderRepository.cs
```

### DbContext (Entity Framework Core)
Contexto do Entity Framework Core, responsável pelo mapeamento das entidades e gerenciamento das conexões com o banco relacional. As configurações de mapeamento das entidades são definidas em classes separadas por entidade.

```
[componente]/[componente].Infrastructure/Data/
[componente]/[componente].Infrastructure/Data/AppDbContext.cs
[componente]/[componente].Infrastructure/Data/Configurations/
[componente]/[componente].Infrastructure/Data/Configurations/OrderConfiguration.cs
```

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>();
    }
}
```

### Integrations — APIs Externas
Implementações dos clientes HTTP para integrações com APIs externas. Consulte `apis-integrations.md`.

```
[componente]/[componente].Infrastructure/Integrations/Apis/
[componente]/[componente].Infrastructure/Integrations/Apis/[ApiName]/
[componente]/[componente].Infrastructure/Integrations/Apis/[ApiName]/[ApiName]Client.cs
```

### Integrations — AWS
Implementações dos clientes AWS utilizando o AWSSDK oficial. Consulte `aws-integrations.md`.

```
[componente]/[componente].Infrastructure/Integrations/Aws/
[componente]/[componente].Infrastructure/Integrations/Aws/[ServiceName]/
[componente]/[componente].Infrastructure/Integrations/Aws/[ServiceName]/[ServiceName]Client.cs
```

### Integrations — Kafka
Implementações de producers e consumers Kafka via Confluent.Kafka. Consulte `kafka-integrations.md`.

```
[componente]/[componente].Infrastructure/Integrations/Kafka/
[componente]/[componente].Infrastructure/Integrations/Kafka/[TopicName]/
[componente]/[componente].Infrastructure/Integrations/Kafka/[TopicName]/[TopicName]Producer.cs
[componente]/[componente].Infrastructure/Integrations/Kafka/[TopicName]/[TopicName]Consumer.cs
```

### Integrations — RabbitMQ
Implementações de producers e consumers RabbitMQ via RabbitMQ.Client. Consulte `rabbit-mq-integrations.md`.

```
[componente]/[componente].Infrastructure/Integrations/RabbitMq/
[componente]/[componente].Infrastructure/Integrations/RabbitMq/[QueueOrExchangeName]/
[componente]/[componente].Infrastructure/Integrations/RabbitMq/[QueueOrExchangeName]/[QueueOrExchangeName]Producer.cs
[componente]/[componente].Infrastructure/Integrations/RabbitMq/[QueueOrExchangeName]/[QueueOrExchangeName]Consumer.cs
```

### Messaging
Classe base de resiliência para consumers de mensageria. Consulte `messaging-resilience.md`.

```
[componente]/[componente].Infrastructure/Messaging/
[componente]/[componente].Infrastructure/Messaging/ResilientConsumerBase.cs
```

### Policies
Políticas de resiliência via Polly, compartilhadas entre as integrações. Consulte `apis-integrations.md` e `messaging-resilience.md`.

```
[componente]/[componente].Infrastructure/Policies/
[componente]/[componente].Infrastructure/Policies/CircuitBreakerPolicy.cs
```

### Dependency Injection
Registro de todos os componentes da camada, incluindo configuração do DbContext, MongoDB, clientes AWS, Kafka, RabbitMQ e políticas de resiliência. Consulte `dependency-injection.md`.

```
[componente]/[componente].Infrastructure/InfrastructureDependency.cs
```

---

## Estrutura Completa

```
[componente]/[componente].Infrastructure/
├── Data/
│   ├── Configurations/
│   │   └── OrderConfiguration.cs
│   └── AppDbContext.cs
├── Integrations/
│   ├── Apis/
│   │   └── [ApiName]/
│   ├── Aws/
│   │   └── [ServiceName]/
│   ├── Kafka/
│   │   └── [TopicName]/
│   └── RabbitMq/
│       └── [QueueOrExchangeName]/
├── Messaging/
│   └── ResilientConsumerBase.cs
├── Policies/
│   └── CircuitBreakerPolicy.cs
├── Repositories/
│   └── [Resource]/
└── InfrastructureDependency.cs
```

---

## Convenções

- A Infrastructure implementa contratos do Domain — nunca define seus próprios contratos
- O DbContext é sempre configurado via `IEntityTypeConfiguration<T>` por entidade — nunca via Fluent API diretamente no `OnModelCreating`
- As configurações do MongoDB (connection string, database name, collection names) são gerenciadas via `appsettings` e registradas na `InfrastructureDependency.cs`
- Repositórios nunca expõem o DbContext ou a sessão MongoDB para camadas superiores
- Queries Dapper são sempre referenciadas a partir das constantes definidas em `[componente].Domain/Integrations/Sql/`
- Consumers de mensageria são registrados como `BackgroundService` na `InfrastructureDependency.cs`
- A camada de Infrastructure não referencia a camada de Application ou Presentation