---
name: code-review
description: 'Use this skill when the user asks to review a Pull Request or staged changes. Trigger for prompts like "review this PR", "check my code", "review staged changes", "analyze this pull request". Do not trigger for refactoring, standard creation, or test generation.'
allowed-tools: Read, Glob, Grep, Bash(git *), Bash(gh *)
---

## Guardrails

- **Sem alteração de código** — apenas leitura e análise; nunca modificar arquivos durante o review
- **Sem acesso a branches externas** — apenas leitura da branch atual ou do PR informado
- **Sem execução de comandos no repositório** — apenas análise estática do código
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler `appsettings.Production.json` ou arquivos com credenciais
- **Sem aprovação automática de PRs** — apenas gerar o relatório; a aprovação é sempre manual
- **Relatório apenas no chat** — nunca criar arquivos ou comentários no PR automaticamente

# Skill: Code Review

## MCP

### 1. Coletar dados via GitHub e Git MCP

**Se PR informado — GitHub MCP:**
```
get_pull_request → obter metadados e título do PR
get_pull_request_diff → obter diff completo dos arquivos alterados
list_commits → listar commits do PR para contexto
```

**Se staging — Git MCP:**
```
git_status → listar arquivos em staging
git_diff_staged → obter diff completo dos arquivos em staging
git_branch → confirmar branch atual antes de prosseguir
```

---

## Objetivo

Executa o code review de um Pull Request ou das alterações em staging, gerando um relatório estruturado com resultado de aprovação, pontos de melhoria e sugestões de correção com código.

---

## Contextos Necessários

- [code-review-checklist.md](../../context/engineering-process/code-review-checklist.md)
- [solution-architecture.md](../../context/architecture/solution-architecture.md)
- [layer-objects.md](../../context/architecture/layer-objects.md)
- [automapper-profiles.md](../../context/architecture/automapper-profiles.md)
- [result-pattern.md](../../context/patterns/result-pattern.md)
- [solid.md](../../context/patterns/solid.md)
- [minimal-apis.md](../../context/development/minimal-apis.md)
- [validators.md](../../context/development/validators.md)
- [dependency-injection.md](../../context/development/dependency-injection.md)
- [logging-standards.md](../../context/development/logging-standards.md)
- [unit-tests.md](../../context/testing/unit-tests.md)
- [mock-classes.md](../../context/testing/mock-classes.md)
- [data-mocks.md](../../context/testing/data-mocks.md)

---

## Entrada

O usuário deve fornecer uma das seguintes opções:

- **Link do PR** — o agente analisa as alterações do Pull Request
- **Alterações em staging** — o agente analisa o diff das alterações atuais

Se nenhuma das opções for fornecida, perguntar:

```
Como deseja realizar o code review?
1. Informar o link do Pull Request
2. Usar as alterações em staging
```

---

## Passos

### 1. Coletar alterações

- Se **link do PR** → analisar todos os arquivos alterados no PR
- Se **staging** → analisar todos os arquivos com alterações não commitadas

### 2. Identificar contexto das alterações

Para cada arquivo alterado, identificar:

- **Camada** — Presentation, Application, Domain, Infrastructure ou Tests
- **Tipo de artefato** — Endpoint, AppService, Service, Repository, Validator, Filter, Middleware, Test, etc.
- **Contextos de revisão aplicáveis** — conforme [code-review-checklist.md](../../context/engineering-process/code-review-checklist.md)

### 3. Executar checklist por categoria

Executar apenas as categorias do checklist relevantes às alterações identificadas:

- **Arquitetura e Camadas** — sempre
- **Código** — sempre
- **Minimal APIs e Endpoints** — se há alterações em endpoints
- **Validação** — se há alterações em validators ou requests
- **App Services** — se há alterações em AppServices
- **Testes** — se há alterações em classes de teste ou classes testáveis
- **Integrações** — se há alterações em clientes de integração ou mensageria
- **Injeção de Dependência** — se há novos serviços ou alterações em XDependency
- **Documentação e Padrões** — sempre
- **Segurança** — sempre

### 4. Classificar problemas encontrados

Cada problema encontrado deve ser classificado em:

| Classificação | Descrição |
|---------------|-----------|
| 🔴 **Blocker** | Viola padrão arquitetural, regra de segurança ou impede aprovação |
| 🟡 **Warning** | Desvio de padrão que deve ser corrigido mas não bloqueia |
| 🔵 **Suggestion** | Melhoria opcional que agrega qualidade sem ser obrigatória |

### 5. Gerar relatório

Produzir o relatório estruturado conforme o template abaixo.

---

## Template de Relatório

```markdown
# Code Review — [Nome do PR ou "Staging"]

## Resultado

> ✅ Aprovado | ⚠️ Aprovado com ressalvas | ❌ Reprovado

**Motivo:** [resumo objetivo do resultado]

---

## Resumo das Alterações

- **Arquivos analisados:** N
- **Camadas afetadas:** [Presentation, Application, Domain, Infrastructure, Tests]
- **Blockers:** N
- **Warnings:** N
- **Suggestions:** N

---

## Problemas Encontrados

### 🔴 Blockers

#### [NomeDoArquivo.cs] — [Linha ou trecho]
**Problema:** [descrição clara do problema e qual padrão está sendo violado]

**Como está:**
```csharp
// código atual
```

**Como deve ficar:**
```csharp
// código corrigido
```

---

### 🟡 Warnings

#### [NomeDoArquivo.cs] — [Linha ou trecho]
**Problema:** [descrição clara do problema]

**Como está:**
```csharp
// código atual
```

**Como deve ficar:**
```csharp
// código corrigido
```

---

### 🔵 Suggestions

#### [NomeDoArquivo.cs]
**Sugestão:** [descrição da melhoria e por que agrega valor]

**Exemplo:**
```csharp
// código sugerido
```

---

## Checklist de Aprovação

- [ ] Nenhum Blocker identificado
- [ ] Pipeline de CI passando
- [ ] Cobertura mínima de 85% mantida
- [ ] Nenhum warning de compilação

---

## Critérios de Resultado

| Resultado | Condição |
|-----------|----------|
| ✅ Aprovado | Nenhum Blocker identificado |
| ⚠️ Aprovado com ressalvas | Nenhum Blocker, mas há Warnings que devem ser endereçados |
| ❌ Reprovado | Um ou mais Blockers identificados |

---

## Validação

Antes de entregar o relatório, verificar:

- [ ] Todas as categorias aplicáveis do checklist foram executadas
- [ ] Todo problema classificado como Blocker possui sugestão de correção com código
- [ ] Todo problema classificado como Warning possui sugestão de correção com código
- [ ] O resultado final está coerente com os problemas encontrados
- [ ] Nenhum item do checklist foi ignorado sem justificativa
- [ ] O relatório referencia o arquivo e trecho exato de cada problema encontrado

--- 

## Prompt Examples

- "revisa esse PR"
- "faz o code review das minhas alterações"
- "analisa o código em staging"
- "verifica se meu código está seguindo os padrões"
- "revisa o pull request #42"

---

## Related Skills

- `refactor-to-standards` — corrigir os desvios identificados no review
- `create-unit-test` — gerar testes ausentes identificados durante o review

---

## Error Handling

- **PR não encontrado** — se o link do PR for inválido ou inacessível, alertar e oferecer a opção de usar as alterações em staging
- **Sem alterações em staging** — se não houver alterações em staging e nenhum PR for informado, alertar e aguardar nova entrada do usuário
- **Arquivo binário ou gerado** — ignorar arquivos de migration gerados, binários e arquivos fora do escopo de código-fonte; informar ao usuário quais foram ignorados
- **PR de outra branch que não `develop` ou `main`** — alertar que o review está sendo feito em uma branch não padrão e confirmar antes de prosseguir