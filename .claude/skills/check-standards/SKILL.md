---
name: check-standards
description: 'Use this skill when the user asks to verify code adherence to project standards. Trigger for prompts like "check standards", "verify patterns", "analyze code quality", "check if this follows the conventions". Do not trigger for refactoring — use refactor-to-standards instead.'
allowed-tools: Read, Glob, Grep, Bash(git *)
---

## Guardrails

- **Sem alteração de código** — apenas leitura e análise; nunca modificar arquivos
- **Sem execução de comandos no repositório** — apenas análise estática
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler `appsettings.Production.json`
- **Relatório apenas no chat** — nunca criar arquivos ou Issues automaticamente
- **Sem acionamento automático de refatoração** — apenas sugerir; aguardar confirmação do usuário para acionar `refactor-to-standards`

# Skill: Check Standards

## Objetivo

Analisa o repositório — completo ou por escopo selecionado — e gera um relatório de aderência aos padrões do projeto sem aplicar nenhuma alteração. Ao final, sugere a execução do `refactor-to-standards` nos arquivos com problemas identificados.

---

## Contextos Necessários

- [solution-architecture.md](../../context/architecture/solution-architecture.md)
- [layer-objects.md](../../context/architecture/layer-objects.md)
- [automapper-profiles.md](../../context/architecture/automapper-profiles.md)
- [solid.md](../../context/patterns/solid.md)
- [result-pattern.md](../../context/patterns/result-pattern.md)
- [minimal-apis.md](../../context/development/minimal-apis.md)
- [validators.md](../../context/development/validators.md)
- [filters.md](../../context/development/filters.md)
- [app-services.md](../../context/development/app-services.md)
- [dependency-injection.md](../../context/development/dependency-injection.md)
- [logging-standards.md](../../context/development/logging-standards.md)
- [ef-standards.md](../../context/persistence/ef-standards.md)
- [dapper-standards.md](../../context/persistence/dapper-standards.md)
- [query-patterns.md](../../context/persistence/query-patterns.md)
- [unit-tests.md](../../context/testing/unit-tests.md)
- [mock-classes.md](../../context/testing/mock-classes.md)
- [data-mocks.md](../../context/testing/data-mocks.md)

---

## Entrada

Por padrão, a skill analisa todo o repositório. O usuário pode restringir o escopo:

```
Deseja analisar o repositório completo ou um escopo específico?
1. Completo — analisar todos os arquivos
2. Por camada — Presentation, Application, Domain, Infrastructure ou Tests
3. Por arquivo — informar os arquivos específicos
```

---

## Passos

## MCP

### 1. Coletar arquivos via Filesystem e Git MCP

**Para escopo completo ou por camada — Filesystem MCP:**

```
list_directory → src/[escopo selecionado]
read_multiple_files → arquivos .cs identificados no escopo
```

**Para detectar alterações recentes — Git MCP (opcional):**

```
git_status → identificar arquivos modificados recentemente
git_diff → obter contexto das alterações
```

---

### 1. Definir escopo de análise

- Se **completo** → analisar todos os arquivos `.cs` do repositório exceto migrations e arquivos gerados
- Se **por camada** → analisar apenas os arquivos da camada selecionada
- Se **por arquivo** → analisar apenas os arquivos informados

### 2. Classificar arquivos por tipo de artefato

Para cada arquivo identificado, classificar em:

| Tipo | Verificações aplicáveis |
|------|------------------------|
| Endpoint | Minimal API, TypedResults, RequireAuthorization, WithOpenApi |
| AppService | Sem regras de negócio, AutoMapper, construtor primário |
| Validator | FluentValidation, sem regras de negócio |
| Service | Result Pattern, AutoMapper, construtor primário |
| Repository | EF Core vs Dapper, soft delete, queries no Domain |
| Entity | BaseEntity, configuração EF, snake_case |
| Integration Client | Contrato no Domain, circuit breaker, AutoMapper |
| Consumer | ResilientConsumerBase, três filas/tópicos |
| Test | AAA, nomenclatura, Shouldly, CancellationToken.None |
| Mock Class | BaseMock<T>, métodos encadeáveis |
| Data Mock | Valid(), Builder se > 4 variações |
| XDependency | Lifetimes corretos, sem captive dependency |

### 3. Executar verificações por arquivo

Para cada arquivo, verificar os desvios e classificar:

| Classificação | Descrição |
|---------------|-----------|
| 🔴 **Blocker** | Viola padrão arquitetural ou regra de segurança |
| 🟡 **Warning** | Desvio de padrão que deve ser corrigido |
| 🔵 **Suggestion** | Melhoria opcional que agrega qualidade |

### 4. Consolidar e gerar relatório

---

## Template de Relatório

```markdown
# Relatório de Padrões — [escopo analisado]
**Data:** [data atual]

---

## Resumo Executivo

| Métrica | Valor |
|---------|-------|
| Arquivos analisados | N |
| Arquivos com desvios | N |
| Arquivos conformes | N |
| Blockers | N |
| Warnings | N |
| Suggestions | N |
| Aderência geral | N% |

---

## Desvios por Camada

### 0 - Presentation

#### 🔴 Blockers
| Arquivo | Desvio |
|---------|--------|
| `CreateOrderEndpoint.cs` | `Results` em vez de `TypedResults` |
| `OrderAppService.cs` | Mapeamento manual entre camadas — AutoMapper não utilizado |

#### 🟡 Warnings
| Arquivo | Desvio |
|---------|--------|
| `CreateOrderRequest.cs` | Construtor não usa padrão primário |

#### 🔵 Suggestions
| Arquivo | Sugestão |
|---------|----------|
| `OrderAppService.cs` | Extrair validação de nulo para método privado |

---

### 1 - Application

#### 🔴 Blockers
| Arquivo | Desvio |
|---------|--------|
| `OrderService.cs` | Exceção de negócio lançada em vez de Result Pattern |

---

### 2 - Domain
_Nenhum desvio encontrado._

---

### 3 - Infrastructure

#### 🟡 Warnings
| Arquivo | Desvio |
|---------|--------|
| `OrderRepository.cs` | Query Dapper inline — deve estar em constante no Domain |
| `OrderRepository.cs` | `Remove()` usado em vez de soft delete via `DeletedAt` |

---

### Tests

#### 🟡 Warnings
| Arquivo | Desvio |
|---------|--------|
| `OrderServiceTests.cs` | Asserção via `Assert.Equal` — deve usar Shouldly |
| `OrderServiceTests.cs` | Teste sem sufixo `_Async` em método assíncrono |

---

## Arquivos Conformes

- `[Recurso]Domain.cs`
- `[Recurso]Model.cs`
- _(lista completa)_

---

## Próximos Passos

### Arquivos que precisam de atenção

Os seguintes arquivos possuem Blockers ou Warnings e devem ser refatorados:

- `CreateOrderEndpoint.cs`
- `OrderAppService.cs`
- `OrderService.cs`
- `OrderRepository.cs`
- `OrderServiceTests.cs`

### Sugestão de refatoração

Para corrigir os desvios identificados, execute a skill `refactor-to-standards` nos arquivos acima:

> "Refatore os seguintes arquivos para os padrões do projeto:
> CreateOrderEndpoint.cs, OrderAppService.cs, OrderService.cs, OrderRepository.cs, OrderServiceTests.cs"
```

---

## Output Esperado

Relatório exibido no chat conforme o template acima, com:

- Resumo executivo com métricas consolidadas
- Desvios agrupados por camada e classificados por severidade
- Lista de arquivos conformes
- Sugestão de execução do `refactor-to-standards` com os arquivos problemáticos

---

## Validação

Antes de entregar o relatório, verificar:

- [ ] Todos os arquivos do escopo foram analisados
- [ ] Nenhuma alteração foi aplicada — apenas diagnóstico
- [ ] Desvios classificados corretamente em Blocker, Warning ou Suggestion
- [ ] Arquivos conformes listados separadamente
- [ ] Percentual de aderência calculado corretamente
- [ ] Sugestão de `refactor-to-standards` incluída ao final com os arquivos corretos
- [ ] Camadas sem desvios sinalizadas explicitamente como conformes

---

## Prompt Examples

- "verifica se o código está seguindo os padrões"
- "analisa a qualidade do código do projeto"
- "quais arquivos estão fora do padrão?"
- "faz um diagnóstico de conformidade do repositório"
- "checa os padrões da camada de Infrastructure"

---

## Related Skills

- `refactor-to-standards` — corrigir os desvios identificados no relatório
- `check-coverage` — verificar cobertura de testes em conjunto com os padrões

---

## Error Handling

- **Projeto sem estrutura reconhecível** — se a solution não seguir o padrão de `project-structure.md`, alertar e listar apenas os arquivos que foi possível analisar
- **Arquivo inacessível** — ignorar arquivos inacessíveis e informar ao usuário quais foram pulados
- **Escopo muito amplo** — se o repositório tiver mais de 200 arquivos, sugerir análise por camada para evitar respostas incompletas