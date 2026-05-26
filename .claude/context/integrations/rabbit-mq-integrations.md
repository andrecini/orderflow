# RabbitMQ Integrations

## Visão Geral

As integrações com RabbitMQ seguem os mesmos princípios da Clean Architecture adotados nas demais integrações. Os contratos são definidos no `[componente].Domain` e as implementações residem no `[componente].Infrastructure`, utilizando **RabbitMQ.Client** como biblioteca de comunicação. Um componente pode atuar como producer, consumer, ou ambos, dependendo do contexto. O padrão de mensageria adotado — queue (ponto a ponto) ou exchange/fanout — varia conforme o cenário.

---

## Localização

### Contratos (Domain)
```
[componente]/[componente].Domain/Integrations/RabbitMq/[QueueOrExchangeName]/
[componente]/[componente].Domain/Integrations/RabbitMq/[QueueOrExchangeName]/Interfaces/
[componente]/[componente].Domain/Integrations/RabbitMq/[QueueOrExchangeName]/Interfaces/I[QueueOrExchangeName]Producer.cs
[componente]/[componente].Domain/Integrations/RabbitMq/[QueueOrExchangeName]/Interfaces/I[QueueOrExchangeName]Consumer.cs
[componente]/[componente].Domain/Integrations/RabbitMq/[QueueOrExchangeName]/[QueueOrExchangeName]Message.cs
```

### Implementação (Infrastructure)
```
[componente]/[componente].Infrastructure/Integrations/RabbitMq/[QueueOrExchangeName]/
[componente]/[componente].Infrastructure/Integrations/RabbitMq/[QueueOrExchangeName]/[QueueOrExchangeName]Producer.cs
[componente]/[componente].Infrastructure/Integrations/RabbitMq/[QueueOrExchangeName]/[QueueOrExchangeName]Consumer.cs
```

---

## Contratos da Integração

As interfaces são declaradas no `[componente].Domain` e implementadas no `[componente].Infrastructure`, seguindo a regra de inversão de dependência da Clean Architecture. Apenas as interfaces necessárias ao contexto do componente devem ser criadas.

```csharp
public interface IOrderCreatedProducer
{
    Task ProduceAsync(OrderCreatedMessage message, CancellationToken cancellationToken);
}

public interface IOrderCreatedConsumer
{
    Task ConsumeAsync(CancellationToken cancellationToken);
}
```

---

## Mensagens

Cada fila ou exchange possui sua própria classe de mensagem, tipada e exclusiva da integração RabbitMQ. Não são reutilizadas como DTOs internos da aplicação.

```csharp
public class OrderCreatedMessage
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

---

## Implementação do Producer

### Queue (ponto a ponto)
```csharp
public class OrderCreatedProducer(IConnection connection, IMapper mapper) : IOrderCreatedProducer
{
    private const string QueueName = "order-created-main";

    public async Task ProduceAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

        var payload = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: QueueName, body: payload, cancellationToken: cancellationToken);
    }
}
```

### Exchange (fanout)
```csharp
public class OrderCreatedProducer(IConnection connection, IMapper mapper) : IOrderCreatedProducer
{
    private const string ExchangeName = "order-created-main";

    public async Task ProduceAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(ExchangeName, type: ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken);

        var payload = JsonSerializer.SerializeToUtf8Bytes(message);

        await channel.BasicPublishAsync(exchange: ExchangeName, routingKey: string.Empty, body: payload, cancellationToken: cancellationToken);
    }
}
```

---

## Implementação do Consumer

Consumers herdam de `ResilientConsumerBase<TMessage>` e são implementados como `BackgroundService`. O padrão de três filas (`[nome]-main`, `[nome]-retry`, `[nome]-dead-letter`) e o roteamento entre elas é gerenciado pela classe base — consulte `messaging-resilience.md`.

```csharp
public class OrderCreatedConsumer(
    IConnection connection,
    IMapper mapper,
    IAsyncPolicy resiliencePolicy) : ResilientConsumerBase<OrderCreatedMessage>(resiliencePolicy), IOrderCreatedConsumer
{
    private const string MainQueue = "order-created-main";
    private const string RetryQueue = "order-created-retry";
    private const string DeadLetterQueue = "order-created-dead-letter";

    public async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(MainQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            var message = JsonSerializer.Deserialize<OrderCreatedMessage>(args.Body.Span);
            await HandleAsync(message, attemptCount: 0, cancellationToken);
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken);
        };

        await channel.BasicConsumeAsync(MainQueue, autoAck: false, consumer, cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    protected override async Task ProcessAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var model = mapper.Map<OrderCreatedModel>(message);
        // invocar a service correspondente
    }

    protected override async Task SendToRetryAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(RetryQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        var payload = JsonSerializer.SerializeToUtf8Bytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: RetryQueue, body: payload, cancellationToken: cancellationToken);
    }

    protected override async Task SendToDeadLetterAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(DeadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        var payload = JsonSerializer.SerializeToUtf8Bytes(message);
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: DeadLetterQueue, body: payload, cancellationToken: cancellationToken);
    }
}
```

---

## Validação de Mensagens

As mensagens produzidas são validadas com FluentValidation antes de serem publicadas.

```csharp
public class OrderCreatedMessageValidator : AbstractValidator<OrderCreatedMessage>
{
    public OrderCreatedMessageValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}
```

---

## Convenções

- Um arquivo por fila ou exchange, separando producer e consumer quando ambos existirem
- Nome das classes segue o padrão `[QueueOrExchangeName]Producer`, `[QueueOrExchangeName]Consumer` e `[QueueOrExchangeName]Message`
- Filas e exchanges nomeadas em `kebab-case` com sufixos `-main`, `-retry` e `-dead-letter`
- O mapeamento entre mensagens RabbitMQ e modelos internos é feito via AutoMapper
- Consumers são registrados como `BackgroundService` e não expõem endpoints
- `autoAck: false` é o padrão — o `BasicAck` é enviado apenas após o processamento bem-sucedido da mensagem
- A lógica de resiliência e roteamento entre filas é responsabilidade de `ResilientConsumerBase` — consulte `messaging-resilience.md`

---

## Injeção de Dependência

O registro dos producers, consumers, validators, conexão RabbitMQ e políticas de resiliência está centralizado em `[componente]/[componente].Infrastructure/InfrastructureDependency.cs`.