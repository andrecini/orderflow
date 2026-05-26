---
name: write-commit
description: 'Use this skill when the user asks to generate a commit message. Trigger for prompts like "generate a commit message", "write the commit", "what should my commit say". Do not trigger for changelog updates, PR descriptions, or card creation.'
allowed-tools: Read, Bash(git *)
---

## Guardrails

- **Escopo restrito à geração de mensagem** — nunca executar o commit diretamente; apenas exibir a mensagem
- **Sem acesso a branches externas** — apenas leitura das alterações em staging da branch atual
- **Sem geração de commit sem Card ID** — bloquear e orientar o usuário a criar um card antes
- **Sem commit de múltiplos tipos** — alertar e orientar separação quando múltiplos tipos forem detectados
- **Sem leitura de arquivos de configuração sensíveis** — nunca ler `appsettings.Production.json` ou arquivos com credenciais

# Skill: Write Commit

## MCP

### 1. Coletar alterações via Git MCP
```
git_status → listar arquivos em staging
git_diff_staged → obter diff completo para análise
git_branch → confirmar branch atual e nome para validar nomenclatura
git_log → obter último commit para referência de contexto
```

---

## Objetivo

Gera a mensagem de commit no padrão Conventional Commits em português a partir das alterações em staging. O campo `CARD` é obrigatório — o commit é bloqueado se o ID não for informado.

---

## Contextos Necessários

- [commit-standards.md](../../context/engineering-process/commit-standards.md)

---

## Entrada

A skill utiliza automaticamente as alterações em staging. Se não houver alterações em staging, alertar o usuário:

```
⚠️ Nenhuma alteração em staging encontrada.
Adicione os arquivos ao stage antes de gerar a mensagem de commit.
```

O usuário deve fornecer adicionalmente:

- **Card ID** — número da Issue no GitHub (`#[id]`). Se não informado, perguntar:

```
Qual o ID do card relacionado a essas alterações?
Informe o número da Issue — ex: 42
```

Se o usuário não souber ou não tiver um card, bloquear e orientar:

```
❌ O campo CARD é obrigatório para todos os commits.
Crie uma Issue no GitHub antes de commitar — utilize a skill create-card se necessário.
```

---

## Passos

### 1. Analisar alterações em staging

Inspecionar os arquivos em staging e identificar:

- **Arquivos alterados** — por camada e tipo de artefato
- **Natureza das alterações** — criação, modificação, remoção
- **Escopo** — recurso ou módulo afetado (ex: `orders`, `payments`, `auth`)

### 2. Determinar o tipo de commit

Com base nas alterações identificadas, determinar o tipo conforme [commit-standards.md](../../context/engineering-process/commit-standards.md):

| Alterações detectadas | Tipo sugerido |
|----------------------|---------------|
| Novos endpoints, services, features | `feat` |
| Correção de comportamento incorreto | `fix` |
| Refatoração sem mudança de comportamento | `refactor` |
| Adição ou correção de testes | `test` |
| Alterações em documentação | `docs` |
| Atualização de dependências, configurações, pipelines | `chore` |
| Formatação, espaçamento, sem mudança de lógica | `style` |
| Melhoria de performance | `perf` |

Se as alterações abrangerem múltiplos tipos, alertar o usuário:

```
⚠️ As alterações em staging envolvem múltiplos tipos de commit (feat + test).
Recomendado separar em commits distintos. Deseja continuar com um único commit ou separar?
1. Continuar com um único commit — informar o tipo predominante
2. Separar — orientarei quais arquivos incluir em cada commit
```

### 3. Determinar o escopo

O escopo é opcional mas recomendado quando as alterações são restritas a um contexto específico:

- Identificar automaticamente pelo recurso afetado — ex: `orders`, `payments`, `auth`
- Se as alterações afetarem múltiplos recursos, omitir o escopo

### 4. Gerar descrição breve

- Sintetizar as alterações em uma frase objetiva no infinitivo
- Máximo de 72 caracteres
- Sem ponto final

### 5. Gerar descrição completa

- Listar em tópicos as alterações realizadas por arquivo ou grupo de arquivos
- Omitir alterações triviais (ex: formatação, imports)
- Omitir se houver apenas uma alteração relevante

### 6. Confirmar Card ID e gerar mensagem

Com o Card ID confirmado, gerar a mensagem no formato:

```
TIPO(escopo): descrição breve

- detalhe 1
- detalhe 2
- detalhe 3

CARD: #[id]
```

---

## Output Esperado

```
✅ Mensagem de commit gerada:

─────────────────────────────────────────
feat(orders): adicionar endpoint de criação de pedido

- implementar CreateOrderEndpoint com Minimal API
- adicionar CreateOrderRequest e CreateOrderResponse
- adicionar CreateOrderRequestValidator
- configurar rota POST /api/v1/orders com autenticação

CARD: #42
─────────────────────────────────────────

Copie a mensagem acima para o seu commit.
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] Tipo de commit correto conforme as alterações detectadas — consulte [commit-standards.md](../../context/engineering-process/commit-standards.md)
- [ ] Descrição breve no infinitivo, sem ponto final e com máximo de 72 caracteres
- [ ] Descrição completa em tópicos quando há mais de uma alteração relevante
- [ ] Mensagem em **português**
- [ ] Campo `CARD: #[id]` presente e preenchido
- [ ] Alterações de múltiplos tipos sinalizadas ao usuário
- [ ] Nenhum commit gerado sem Card ID

---

## Prompt Examples

- "gera a mensagem de commit"
- "o que devo escrever no commit?"
- "cria o commit das minhas alterações"
- "escreve a mensagem de commit para o que está em stage"
- "qual o commit message correto para essas mudanças?"

---

## Error Handling

- **Sem alterações em staging** — alertar e orientar o usuário a adicionar arquivos ao stage antes de prosseguir
- **Card ID não informado e não detectável** — bloquear e orientar a criar um card via `create-card` antes de commitar
- **Múltiplos tipos detectados** — alertar e orientar a separar em commits distintos antes de gerar a mensagem
- **Alterações em arquivos de configuração sensíveis em staging** — alertar que `appsettings.Production.json` ou arquivos com credenciais não devem ser commitados