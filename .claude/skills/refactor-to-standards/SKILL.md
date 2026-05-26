---
name: refactor-to-standards
description: 'Use this skill when the user asks to refactor code to follow project standards. Trigger for prompts like "refactor this to standards", "fix the patterns in this file", "update this code to follow conventions". Do not trigger for new feature creation or bug fixes.'
allowed-tools: Read, Edit, Glob, Grep, Bash(git *)
---

## Guardrails

- **Sem criação de novos artefatos** — apenas refatoração de arquivos existentes; nunca criar novos arquivos sem confirmação
- **Sem alteração de lógica de negócio** — apenas adequação aos padrões; nunca alterar comportamento funcional
- **Sem alteração de testes** — nunca modificar classes de teste durante a refatoração de código de produção sem informar o usuário
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler ou alterar `appsettings.Production.json`
- **Sem aplicação automática** — sempre apresentar diagnóstico e aguardar confirmação antes de aplicar alterações
- **Opção de undo obrigatória** — sempre oferecer `keep` e `undo` por arquivo refatorado

# Skill: Refactor to Standards

## MCP

### 1. Coletar arquivos via Git e Filesystem MCP

**Se staging:**

```
git_status → listar arquivos modificados em staging
git_diff_staged → obter diff dos arquivos para análise
```

**Para leitura do conteúdo atual:**
```
read_file → src/... (para cada arquivo a ser refatorado)
```

### 2. Escrever arquivos refatorados via Filesystem MCP
Após confirmação do usuário para cada arquivo:
write_file → src/... (arquivo refatorado)

---

## Objetivo

Analisa um ou mais arquivos — fornecidos pelo usuário ou detectados em staging — e refatora o código para aderência aos padrões do projeto. As alterações são aplicadas diretamente nos arquivos, com opção de manter (`keep`) ou desfazer (`undo`) cada refatoração.

---

## Contextos Necessários

Consulte os contextos relevantes conforme os arquivos identificados:

| Artefato | Contextos |
|----------|-----------|
| Endpoint | [minimal-apis.md](../../context/development/minimal-apis.md) · [filters.md](../../context/development/filters.md) · [api-documentation.md](../../context/development/api-documentation.md) · [auth.md](../../context/development/auth.md) |
| AppService | [app-services.md](../../context/development/app-services.md) |
| Validator | [validators.md](../../context/development/validators.md) |
| Service | [layer-application.md](../../context/architecture/layer-application.md) · [result-pattern.md](../../context/patterns/result-pattern.md) |
| Repository | [generic-repository.md](../../context/patterns/generic-repository.md) · [ef-standards.md](../../context/persistence/ef-standards.md) · [dapper-standards.md](../../context/persistence/dapper-standards.md) · [query-patterns.md](../../context/persistence/query-patterns.md) |
| Entidade | [ef-standards.md](../../context/persistence/ef-standards.md) · [sql.md](../../context/persistence/sql.md) |
| Testes | [unit-tests.md](../../context/testing/unit-tests.md) · [mock-classes.md](../../context/testing/mock-classes.md) · [data-mocks.md](../../context/testing/data-mocks.md) |
| Integração | [apis-integrations.md](../../context/integrations/apis-integrations.md) · [aws-integrations.md](../../context/integrations/aws-integrations.md) · [kafka-integrations.md](../../context/integrations/kafka-integrations.md) · [rabbit-mq-integrations.md](../../context/integrations/rabbit-mq-integrations.md) |
| Geral | [solid.md](../../context/patterns/solid.md) · [layer-objects.md](../../context/architecture/layer-objects.md) · [automapper-profiles.md](../../context/architecture/automapper-profiles.md) · [dependency-injection.md](../../context/development/dependency-injection.md) · [logging-standards.md](../../context/development/logging-standards.md) |

---

## Entrada

O usuário deve fornecer uma das seguintes opções:

- **Código diretamente** — um ou mais arquivos colados ou referenciados
- **Alterações em staging** — o agente detecta automaticamente os arquivos modificados

Se nenhuma opção for fornecida, perguntar:

```
O que deseja refatorar?
1. Fornecer o código — colar ou referenciar os arquivos
2. Usar alterações em staging — analisar arquivos modificados automaticamente
```

---

## Passos

### 1. Coletar arquivos

- Se **código fornecido** → processar os arquivos informados
- Se **staging** → listar todos os arquivos com alterações e confirmar com o usuário quais devem ser refatorados:

```
Os seguintes arquivos foram detectados em staging:
- [arquivo1.cs]
- [arquivo2.cs]
- [arquivo3.cs]

Deseja refatorar todos ou selecionar específicos?
1. Todos
2. Selecionar — informar quais
```

### 2. Identificar desvios por arquivo

Para cada arquivo, identificar os desvios em relação aos padrões do projeto:

| Categoria | Verificações |
|-----------|-------------|
| Construtor | Usa construtor primário? |
| Mapeamento | AutoMapper em todos os mapeamentos entre camadas? |
| Result Pattern | Services e repositories retornam `Result<T>`? |
| Objetos por camada | Objetos corretos sendo usados em cada camada? |
| Endpoints | `TypedResults`, `.RequireAuthorization()`, `.WithOpenApi()` declarados? |
| Soft delete | Usa `DeletedAt` em vez de `Remove()`? |
| Queries | Queries Dapper inline ou em constantes do Domain? |
| Logging | Sintaxe de template — nunca interpolação de string? |
| SOLID | Responsabilidade única, dependências via abstração? |
| Testes | Padrão AAA, nomenclatura, Shouldly, CancellationToken.None? |
| DI | Novos serviços registrados nas XDependency corretas? |
| Segurança | Dados sensíveis logados? Hardcode de credenciais? |

### 3. Apresentar diagnóstico antes de refatorar

Antes de aplicar qualquer alteração, apresentar o diagnóstico por arquivo:

```
📋 Diagnóstico — [NomeDoArquivo.cs]

🔴 Blockers (serão refatorados):
- Construtor não usa padrão primário
- Mapeamento manual entre camadas — AutoMapper não utilizado
- `Results` em vez de `TypedResults` no endpoint

🟡 Warnings (serão refatorados):
- Log com interpolação de string em vez de template

🔵 Suggestions (opcionais — confirmar):
- Extração de método para reduzir responsabilidade da classe

Deseja prosseguir com a refatoração?
1. Sim — refatorar tudo
2. Sim — refatorar apenas Blockers e Warnings
3. Personalizar — selecionar o que refatorar
```

### 4. Aplicar refatorações

Para cada desvio confirmado, aplicar a correção diretamente no arquivo seguindo os padrões dos contextos correspondentes. Após cada arquivo refatorado, apresentar as opções:

```
✅ [NomeDoArquivo.cs] refatorado.

O que deseja fazer?
- keep — manter as alterações e seguir para o próximo arquivo
- undo — desfazer as alterações deste arquivo e seguir para o próximo
- review — ver o diff das alterações antes de decidir
```

### 5. Relatório final

Após processar todos os arquivos, apresentar o relatório consolidado:

```markdown
# Relatório de Refatoração

## Resumo
- **Arquivos processados:** N
- **Arquivos refatorados:** N
- **Arquivos desfeitos (undo):** N

## Alterações por Arquivo

### ✅ [NomeDoArquivo.cs] — mantido
- Construtor refatorado para padrão primário
- Mapeamento manual substituído por AutoMapper
- `Results` substituído por `TypedResults`

### ↩️ [NomeDoArquivo.cs] — desfeito
- Alterações revertidas a pedido do usuário

## Desvios não corrigidos
- [arquivo.cs] — [desvio identificado mas não refatorado — motivo]
```

---

## Output Esperado

- Arquivos refatorados aplicados diretamente no código
- Relatório consolidado com todas as alterações realizadas e desfeitas
- Lista de desvios não corrigidos com justificativa

---

## Validação

Antes de entregar o output, verificar por arquivo refatorado:

- [ ] Construtores primários em todas as classes com DI
- [ ] AutoMapper em todos os mapeamentos entre camadas
- [ ] `Result<T>` retornado em todas as operações de service e repository
- [ ] `TypedResults` usado nos endpoints
- [ ] `CancellationToken` propagado em todas as operações assíncronas
- [ ] Queries Dapper referenciando constantes do Domain — nunca inline
- [ ] Soft delete via `DeletedAt` — nunca `Remove()`
- [ ] Logs usando sintaxe de template — nunca interpolação de string
- [ ] Nenhum dado sensível logado
- [ ] Nenhuma credencial hardcoded
- [ ] Objetos corretos em cada camada — consulte [layer-objects.md](../../context/architecture/layer-objects.md)
- [ ] Novos serviços registrados nas `XDependency.cs` corretas após refatoração

---

## Prompt Examples

- "refatora esse arquivo para os padrões do projeto"
- "corrige os desvios de padrão no OrderService"
- "atualiza o código em staging para seguir as convenções"
- "esse código está fora do padrão, corrige"
- "adequa o repositório de pagamentos aos padrões"

---

## Related Skills

- `check-standards` — verificar desvios antes de refatorar sem alterar o código
- `create-unit-test` — gerar testes para classes refatoradas que não possuem cobertura

---

## Error Handling

- **Sem alterações em staging e nenhum arquivo informado** — alertar e aguardar nova entrada do usuário
- **Arquivo não encontrado** — se um arquivo informado não existir no projeto, alertar e continuar com os demais
- **Refatoração que altera comportamento** — se uma correção de padrão puder alterar o comportamento funcional, alertar explicitamente e aguardar confirmação antes de aplicar
- **Arquivo de configuração ou migration** — nunca refatorar `XDependency.cs`, migrations ou `appsettings`; ignorar e informar ao usuário