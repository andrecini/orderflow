# SQL — PostgreSQL

## Visão Geral

O banco de dados relacional utilizado é o **PostgreSQL**. O acesso é feito via **Entity Framework Core** para operações ORM e **Dapper** para queries customizadas. A configuração da conexão é gerenciada via `appsettings` e registrada na `InfrastructureDependency.cs`.

---

## Configuração

### Connection String

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=[db_name];Username=[user];Password=[password]"
  }
}
```

### Registro na InfrastructureDependency

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));
```

---

## Convenções de Nomenclatura

### Tabelas
- Nome em `snake_case` no plural — ex: `orders`, `order_items`, `payment_methods`
- Nomes sempre em inglês
- Sem prefixos — ex: nunca `tbl_orders`

### Colunas
- Nome em `snake_case` — ex: `customer_id`, `created_at`, `total_amount`
- Chave primária sempre nomeada `id`
- Chaves estrangeiras seguem o padrão `[tabela_referenciada_singular]_id` — ex: `order_id`, `customer_id`
- Datas sempre com sufixo `_at` — ex: `created_at`, `updated_at`, `deleted_at`
- Booleanos com prefixo `is_` ou `has_` — ex: `is_active`, `has_discount`

### Índices
- Nome segue o padrão `ix_[tabela]_[coluna(s)]` — ex: `ix_orders_customer_id`
- Índices únicos seguem o padrão `ux_[tabela]_[coluna(s)]` — ex: `ux_users_email`

### Constraints
- Primary key: `pk_[tabela]` — ex: `pk_orders`
- Foreign key: `fk_[tabela]_[tabela_referenciada]` — ex: `fk_order_items_orders`

---

## Mapeamento EF Core para PostgreSQL

O EF Core é configurado para respeitar as convenções de nomenclatura do PostgreSQL via Npgsql. As configurações de mapeamento ficam em classes `IEntityTypeConfiguration<T>` — consulte `ef-standards.md`.

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

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
```

---

## Migrations

As migrations são gerenciadas pelo EF Core e ficam no projeto de Infrastructure.

```
[componente]/[componente].Infrastructure/Data/Migrations/
```

Comandos mais utilizados:

```bash
# Criar nova migration
dotnet ef migrations add [NomeDaMigration] --project src/3\ -\ Infrastructure/[componente].Infrastructure --startup-project src/0\ -\ Presentation/[componente].Api

# Aplicar migrations pendentes
dotnet ef database update --project src/3\ -\ Infrastructure/[componente].Infrastructure --startup-project src/0\ -\ Presentation/[componente].Api
```

---

## Convenções

- Toda tabela deve ter as colunas `id`, `created_at` e `updated_at` como padrão mínimo
- Soft delete é implementado via coluna `deleted_at` nullable — registros deletados nunca são removidos fisicamente
- Enums são armazenados como `string` — nunca como inteiro
- Timestamps são sempre armazenados em **UTC**
- Nenhuma lógica de negócio é implementada via stored procedures, triggers ou functions no banco