# NoSQL — MongoDB

## Visão Geral

O banco de dados de documentos utilizado é o **MongoDB Atlas**. O acesso é feito via **MongoDB Driver** oficial para .NET. A configuração da conexão é gerenciada via `appsettings` e registrada na `InfrastructureDependency.cs`.

---

## Configuração

### Connection String

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb+srv://[user]:[password]@[cluster].mongodb.net"
  },
  "MongoDB": {
    "DatabaseName": "[db_name]"
  }
}
```

### Registro na InfrastructureDependency

```csharp
services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(configuration.GetConnectionString("MongoDB")));

services.AddScoped<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase(configuration["MongoDB:DatabaseName"]));
```

---

## Convenções de Nomenclatura

### Collections
- Nome em `kebab-case` no plural — ex: `order-documents`, `user-services`, `application-settings`
- Nomes sempre em inglês
- Sem prefixos — ex: nunca `col-orders`

### Campos
- Nome em `snake_case` com `-` como separador — ex: `customer-id`, `created-at`, `total-amount`
- Chave primária mapeada para `_id` do MongoDB
- Chaves de referência seguem o padrão `[documento_referenciado-singular]-id` — ex: `order-id`, `customer-id`
- Datas sempre com sufixo `-at` — ex: `created-at`, `updated-at`, `deleted-at`
- Booleanos com prefixo `is-` ou `has-` — ex: `is-active`, `has-discount`

---

## Mapeamento de Documentos

Os documentos são mapeados via atributos do MongoDB Driver diretamente nas entidades.

```csharp
[BsonCollection("order-documents")]
public class OrderDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("customer-id")]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("total-amount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("created-at")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated-at")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("deleted-at")]
    public DateTime? DeletedAt { get; set; }
}
```

---

## Acesso às Collections

O acesso às collections é feito via `IMongoDatabase` injetado nos repositórios.

```csharp
public class OrderDocumentRepository(IMongoDatabase database) : IOrderDocumentRepository
{
    private readonly IMongoCollection<OrderDocument> _collection =
        database.GetCollection<OrderDocument>("order-documents");

    public async Task<OrderDocument?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<OrderDocument>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
```

---

## Soft Delete

Assim como no SQL, registros no MongoDB nunca são removidos fisicamente. O soft delete é implementado via campo `deleted-at` nullable. Todas as queries devem filtrar documentos com `deleted-at` nulo por padrão.

```csharp
var filter = Builders<OrderDocument>.Filter.And(
    Builders<OrderDocument>.Filter.Eq(x => x.Id, id),
    Builders<OrderDocument>.Filter.Eq(x => x.DeletedAt, null)
);
```

---

## Convenções

- Toda collection deve ter os campos `_id`, `created-at`, `updated-at` e `deleted-at` como padrão mínimo
- Timestamps são sempre armazenados em **UTC**
- Enums são armazenados como `string` — nunca como inteiro
- Nenhuma lógica de negócio é implementada via aggregations complexas no banco — apenas queries de leitura
- O cliente MongoDB (`IMongoClient`) é registrado como `Singleton` — a conexão é reutilizada entre requests
- Collections são sempre acessadas pelo nome definido na convenção — nunca hardcoded fora do repositório
- O nome da collection é definido uma única vez no repositório — preferencialmente como constante privada