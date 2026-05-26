# Pattern: Generic Repository

## Visão Geral

O Generic Repository é um padrão de acesso a dados que abstrai as operações de persistência em uma interface genérica reutilizável. Ele centraliza operações comuns de CRUD, paginação e filtragem, evitando duplicação entre repositórios específicos e desacoplando as camadas superiores dos detalhes de infraestrutura.

O projeto adota duas variações do padrão — uma para bancos relacionais (SQL) e outra para bancos de documentos (NoSQL) — cada uma com sua própria interface base, refletindo as diferenças de consulta e persistência entre os dois paradigmas.

---

## Interface Base — SQL

```csharp
public interface ISqlRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
```

---

## Interface Base — NoSQL

```csharp
public interface INoSqlRepository<TDocument> where TDocument : class
{
    Task<TDocument?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<IEnumerable<TDocument>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<TDocument>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IEnumerable<TDocument>> FindAsync(FilterDefinition<TDocument> filter, CancellationToken cancellationToken);
    Task InsertAsync(TDocument document, CancellationToken cancellationToken);
    Task ReplaceAsync(string id, TDocument document, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}
```

---

## Convenções

- Repositórios concretos herdam da implementação base e podem estender com métodos específicos do contexto
- Operações de escrita nunca retornam a entidade persistida — use uma consulta subsequente se necessário
- `FindAsync` em SQL recebe uma `Expression<Func<TEntity, bool>>` para compatibilidade com LINQ e Entity Framework Core
- `FindAsync` em NoSQL recebe um `FilterDefinition<TDocument>` do MongoDB Driver, permitindo filtros nativos do MongoDB
- Paginação é baseada em `page` (índice iniciado em 1) e `pageSize`
- `CancellationToken` é obrigatório em todas as operações assíncronas
- Repositórios não expõem detalhes de infraestrutura (contexto, coleção, sessão) para as camadas superiores