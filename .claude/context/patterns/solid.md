# Princípios SOLID

## Visão Geral

SOLID é um conjunto de cinco princípios de design de software orientado a objetos que promovem código mais legível, manutenível e extensível. São a base para decisões de arquitetura e organização de código no projeto.

---

## S — Single Responsibility Principle (SRP)

Uma classe deve ter apenas um motivo para mudar, ou seja, deve ser responsável por apenas uma parte do comportamento do sistema.

```csharp
// Violação — a classe faz coisas demais
public class OrderService
{
    public void Process(Order order) { }
    public void SendEmail(Order order) { }
    public void SaveToPdf(Order order) { }
}

// Correto
public class OrderService
{
    public void Process(Order order) { }
}

public class OrderNotificationService
{
    public void SendEmail(Order order) { }
}

public class OrderReportService
{
    public void SaveToPdf(Order order) { }
}
```

---

## O — Open/Closed Principle (OCP)

Uma classe deve estar aberta para extensão e fechada para modificação. Novo comportamento deve ser adicionado sem alterar o código existente.

```csharp
// Correto — novo tipo de desconto é adicionado sem modificar a classe base
public abstract class DiscountStrategy
{
    public abstract decimal Calculate(decimal price);
}

public class SeasonalDiscount : DiscountStrategy
{
    public override decimal Calculate(decimal price) => price * 0.9m;
}

public class LoyaltyDiscount : DiscountStrategy
{
    public override decimal Calculate(decimal price) => price * 0.85m;
}
```

---

## L — Liskov Substitution Principle (LSP)

Subtipos devem ser substituíveis por seus tipos base sem alterar o comportamento esperado do programa.

```csharp
// Violação — a subclasse quebra o contrato da base
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public int Area() => Width * Height;
}

public class Square : Rectangle
{
    public override int Width { set { base.Width = base.Height = value; } }
    public override int Height { set { base.Width = base.Height = value; } }
}

// Correto — modelar sem herança forçada
public interface IShape
{
    int Area();
}

public class Rectangle : IShape
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Area() => Width * Height;
}

public class Square : IShape
{
    public int Side { get; set; }
    public int Area() => Side * Side;
}
```

---

## I — Interface Segregation Principle (ISP)

Interfaces devem ser específicas para quem as consome. Nenhuma classe deve ser forçada a implementar métodos que não utiliza.

```csharp
// Violação — interface genérica demais
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<T>> RunRawQueryAsync(string sql);
}

// Correto — interfaces segregadas por capacidade
public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
}

public interface IWriteRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

---

## D — Dependency Inversion Principle (DIP)

Módulos de alto nível não devem depender de módulos de baixo nível. Ambos devem depender de abstrações. Abstrações não devem depender de detalhes — detalhes devem depender de abstrações.

```csharp
// Violação — dependência direta da implementação
public class OrderService
{
    private readonly SqlOrderRepository _repository = new();
}

// Correto — dependência via abstração injetada
public class OrderService(IOrderRepository repository)
{
    public async Task<Result<OrderModel>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(id, cancellationToken);
    }
}
```

---

## Convenções

- Interfaces são preferidas a classes abstratas quando não há comportamento compartilhado entre implementações
- Dependências são sempre injetadas via construtor — nunca instanciadas diretamente dentro de classes
- Classes com múltiplas responsabilidades identificadas durante revisão de código devem ser refatoradas antes de aprovação do PR
- O DIP é aplicado em todas as camadas — nenhuma camada superior referencia diretamente uma implementação de camada inferior