# EF Standards — Entity Framework Core

## Visão Geral

O acesso a dados relacional é feito via **Entity Framework Core** com abordagem **Code First**. As migrations são geradas a partir das entidades e aplicadas pelo pipeline de CI ou manualmente. O soft delete é aplicado automaticamente via query filter global no `AppDbContext`.

---

## BaseEntity

Toda entidade herda de `BaseEntity`, que define os campos obrigatórios comuns a todas as tabelas.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;
}
```

---

## DbContext

O `AppDbContext` centraliza os `DbSet` e aplica as configurações de entidade via `ApplyConfigurationsFromAssembly`. O soft delete global é configurado via `HasQueryFilter` nas configurações de cada entidade.

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Configuração de Entidades

Cada entidade possui sua própria classe de configuração implementando `IEntityTypeConfiguration<T>`, localizada em `[componente]/[componente].Infrastructure/Data/Configurations/`.

```csharp
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.HasIndex(x => x.CustomerId).HasDatabaseName("ix_orders_customer_id");
    }
}
```

---

## Soft Delete

O soft delete é aplicado automaticamente via `HasQueryFilter` em todas as entidades. Nunca utilizar `Remove()` diretamente — sempre definir `DeletedAt`.

```csharp
public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
{
    var entity = await dbContext.Orders.FindAsync(id, cancellationToken);

    if (entity is null) return;

    entity.DeletedAt = DateTime.UtcNow;

    await dbContext.SaveChangesAsync(cancellationToken);
}
```

Para consultas que precisam incluir registros deletados, use `IgnoreQueryFilters()`:

```csharp
var order = await dbContext.Orders
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
```

---

## Migrations

```
[componente]/[componente].Infrastructure/Data/Migrations/
```

```bash
# Criar nova migration
dotnet ef migrations add [NomeDaMigration] \
  --project src/3\ -\ Infrastructure/[componente].Infrastructure \
  --startup-project src/0\ -\ Presentation/[componente].Api

# Aplicar migrations pendentes
dotnet ef database update \
  --project src/3\ -\ Infrastructure/[componente].Infrastructure \
  --startup-project src/0\ -\ Presentation/[componente].Api
```

---

## Convenções

- Toda entidade herda de `BaseEntity` — nunca definir `Id`, `CreatedAt`, `UpdatedAt` ou `DeletedAt` diretamente na entidade
- `CreatedAt` e `UpdatedAt` são preenchidos automaticamente pelo `SaveChangesAsync` — nunca atribuir manualmente
- Toda entidade possui uma classe de configuração `IEntityTypeConfiguration<T>` dedicada — nunca configurar via Fluent API direto no `OnModelCreating`
- O soft delete é aplicado via `HasQueryFilter` em todas as entidades — nunca usar `Remove()` diretamente
- Enums são sempre armazenados como `string` via `HasConversion<string>()`
- Timestamps são sempre em **UTC**
- Nomes de tabelas e colunas seguem `snake_case` conforme definido em `sql.md`
- O `AppDbContext` nunca é injetado fora da camada de Infrastructure