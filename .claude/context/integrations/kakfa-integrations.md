# Kafka Integrations

## Visão Geral

As integrações com Kafka seguem os mesmos princípios da Clean Architecture adotados nas demais integrações. Os contratos são definidos no `[componente].Domain` e as implementações residem no `[componente].Infrastructure`, utilizando **Confluent.Kafka** como biblioteca de comunicação. Um componente pode atuar como producer, consumer, ou ambos, dependendo do contexto.

---

## Localização

### Contratos (Domain)
```
[componente]/[componente].Domain/Integrations/Kafka/[TopicName]/
[componente]/[componente].Domain/Integrations/Kafka/[TopicName]/Interfaces/
[componente]/[componente].Domain/Integrations/Kafka/[TopicName]/Interfaces/I[TopicName]Producer.cs
[componente]/[componente].Domain/Integrations/Kafka/[TopicName]/Interfaces/I[TopicName]Consumer.cs
[componente]/[componente].Domain/Integrations/Kafka/[TopicName]/[TopicName]Message.cs
```

### Implementação (Infrastructure)
```
[componente]/[componente].Infrastructure/Integrations/Kafka/[TopicName]/
[componente]/[componente].Infrastructure/Integrations/Kafka/[TopicName]/[TopicName]Producer.cs
[componente]/[componente].Infrastructure/Integrations/Kafka/[TopicName]/[TopicName]Consumer.cs
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

Cada tópico possui sua própria classe de mensagem, tipada e exclusiva da integração Kafka. Não são reutilizadas como DTOs internos da aplicação.

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

```csharp
public class OrderCreatedProducer(IProducer<string, string> producer, IMapper mapper) : IOrderCreatedProducer
{
    private const string Topic = "order-created-main";

    public async Task ProduceAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);

        await producer.ProduceAsync(Topic, new Message<string, string>
        {
            Key = message.OrderId.ToString(),
            Value = payload
        }, cancellationToken);
    }
}
```

---

## Implementação do Consumer

Consumers herdam de `ResilientConsumerBase<TMessage>` e são implementados como `BackgroundService`. O padrão de três tópicos (`[nome]-main`, `[nome]-retry`, `[nome]-dead-letter`) e o roteamento entre eles é gerenciado pela classe base — consulte `messaging-resilience.md`.

```csharp
public class OrderCreatedConsumer(
    IConsumer<string, string> consumer,
    IProducer<string, string> producer,
    IMapper mapper,
    IAsyncPolicy resiliencePolicy) : ResilientConsumerBase<OrderCreatedMessage>(resiliencePolicy), IOrderCreatedConsumer
{
    private const string MainTopic = "order-created-main";
    private const string RetryTopic = "order-created-retry";
    private const string DeadLetterTopic = "order-created-dead-letter";

    public async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        consumer.Subscribe(MainTopic);

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = consumer.Consume(cancellationToken);
            var message = JsonSerializer.Deserialize<OrderCreatedMessage>(result.Message.Value);
            await HandleAsync(message, attemptCount: 0, cancellationToken);
        }
    }

    protected override async Task ProcessAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var model = mapper.Map<OrderCreatedModel>(message);
        // invocar a service correspondente
    }

    protected override async Task SendToRetryAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);
        await producer.ProduceAsync(RetryTopic, new Message<string, string> { Key = message.OrderId.ToString(), Value = payload }, cancellationToken);
    }

    protected override async Task SendToDeadLetterAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);
        await producer.ProduceAsync(DeadLetterTopic, new Message<string, string> { Key = message.OrderId.ToString(), Value = payload }, cancellationToken);
    }
}
```

---

## Validação de Mensagens

As mensagens produzidas são validadas com FluentValidation antes de serem enviadas ao tópico.

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

- Um arquivo por tópico, separando producer e consumer quando ambos existirem
- Nome das classes segue o padrão `[TopicName]Producer`, `[TopicName]Consumer` e `[TopicName]Message`
- Tópicos nomeados em `kebab-case` com sufixos `-main`, `-retry` e `-dead-letter`
- O mapeamento entre mensagens Kafka e modelos internos é feito via AutoMapper
- Consumers são registrados como `BackgroundService` e não expõem endpoints
- A lógica de resiliência e roteamento entre tópicos é responsabilidade de `ResilientConsumerBase` — consulte `messaging-resilience.md`

---

## Injeção de Dependência

O registro dos producers, consumers, validators e políticas de resiliência está centralizado em `[componente]/[componente].Infrastructure/InfrastructureDependency.cs`.