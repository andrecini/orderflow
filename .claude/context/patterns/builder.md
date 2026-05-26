# Pattern: Builder

## Visão Geral

O Builder é um padrão de criação que separa a construção de um objeto complexo da sua representação final. Ele permite construir objetos passo a passo, encadeando métodos de configuração e produzindo o resultado através de um método terminal — tipicamente `Build()`.

É indicado quando a criação de um objeto envolve múltiplos parâmetros opcionais, etapas de configuração interdependentes, ou quando expor um construtor com muitos parâmetros prejudicaria a legibilidade e a segurança do código.

---

## Estrutura

```csharp
public class OrderBuilder
{
    private Guid _customerId;
    private List<OrderItem> _items = new();
    private string _currency = "BRL";

    public OrderBuilder WithCustomer(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder WithItem(Guid productId, int quantity)
    {
        _items.Add(new OrderItem(productId, quantity));
        return this;
    }

    public OrderBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public Order Build()
    {
        return new Order(_customerId, _items, _currency);
    }
}
```

---

## Uso

```csharp
var order = new OrderBuilder()
    .WithCustomer(customerId)
    .WithItem(productId, quantity: 2)
    .WithCurrency("USD")
    .Build();
```

---

## Convenções

- O Builder é uma classe mutável — o objeto final produzido por `Build()` deve ser imutável
- Cada método `With*` retorna a própria instância do builder (`return this`) para permitir encadeamento fluente
- O método `Build()` é sempre o ponto terminal da construção
- Valores padrão razoáveis podem ser definidos diretamente na declaração dos campos privados, evitando configurações obrigatórias desnecessárias
- Validações de consistência do objeto final, quando necessárias, são feitas dentro do método `Build()` antes de instanciar o objeto