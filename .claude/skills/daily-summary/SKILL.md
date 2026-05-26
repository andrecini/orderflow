---
name: daily-summary
description: 'Use this skill when the user asks to create the daily async Issue on GitHub. Trigger for prompts like "create the daily", "open the daily issue", "generate today daily summary". Do not trigger for sprint planning, retrospective, or card creation.'
allowed-tools: Bash(gh *), Bash(git *)
---

## Guardrails

- **Escopo restrito à criação da Issue de daily** — nunca criar outros tipos de Issue ou artefatos do GitHub
- **Sem acesso ao código-fonte** — apenas leitura de commits e PRs para detecção de atividades
- **Sem atribuição automática de responsáveis** — a Issue é sempre coletiva e sem assignees
- **Sem criação de Issues em repositórios externos** — apenas no repositório atual
- **Labels restritas a `chore`** — nunca aplicar outras labels
- **Sem leitura de arquivos de configuração sensíveis** — nunca ler `appsettings.Production.json`

# Skill: Daily Summary

## MCP

### 1. Coletar atividades via Git e GitHub MCP

**Se detecção automática:**

```
git_log → commits do dia anterior filtrados por autor
list_pull_requests → PRs abertos ou atualizados no dia anterior
list_issues → Issues movidas ou fechadas no dia anterior
```

### 2. Criar Issue via GitHub MCP

```
create_issue → criar a daily com title, body e label "chore"
Se a tool falhar, gerar conteúdo formatado para criação manual.
```

---

## Objetivo

Gera e cria a Issue de daily assíncrona coletiva no GitHub seguindo o padrão definido em `agile-ceremonies.md`. As atividades do dia anterior podem ser detectadas automaticamente a partir de commits e PRs ou informadas manualmente pelo usuário.

---

## Contextos Necessários

- [agile-ceremonies.md](../../context/agile/agile-ceremonies.md)

---

## Entrada

A skill não exige entrada obrigatória. Opcionalmente o usuário pode:

- **Informar atividades manualmente** — descrever o que foi feito e o que será feito
- **Usar detecção automática** — a skill analisa commits e PRs do dia anterior

Se não informado, perguntar:

```
Como deseja preencher as atividades do dia anterior?
1. Detectar automaticamente — a partir de commits e PRs
2. Informar manualmente — descrever as atividades
3. Deixar em branco — membros preencherão nos comentários
```

---

## Passos

### 1. Definir data da daily

Usar a data atual como referência. O título da Issue seguirá o padrão:

```
Daily — [DD/MM/YYYY]
```

### 2. Coletar atividades do dia anterior (se automático)

Analisar o repositório para identificar:

- **Commits** feitos pelo usuário no dia anterior — tipo, escopo e descrição
- **PRs** abertos, atualizados ou mergeados no dia anterior — título e status
- **Issues** movidas ou fechadas no dia anterior

Consolidar em uma lista de atividades objetivas.

### 3. Gerar template da daily

Seguindo o padrão de [agile-ceremonies.md](../../context/agile/agile-ceremonies.md), gerar o corpo da Issue:

```markdown
## Daily — [DD/MM/YYYY]

> Responda nos comentários abaixo seguindo o template:

---

## Template de resposta

**O que fiz ontem:**
- [atividade 1]
- [atividade 2]

**O que farei hoje:**
- [atividade 1]

**Impedimentos:**
- [impedimento ou "nenhum"]

---

## Atividades detectadas — [DD/MM/YYYY]

> Resumo automático das atividades do repositório ontem:

[lista de commits e PRs detectados — ou "Nenhuma atividade detectada" se não houver]
```

### 4. Criar Issue via GitHub API

```
POST /repos/{owner}/{repo}/issues
Authorization: Bearer {GITHUB_TOKEN}
Content-Type: application/json

{
  "title": "Daily — [DD/MM/YYYY]",
  "body": "[template gerado]",
  "labels": ["chore"]
}
```

Se a chamada for bem-sucedida, retornar:

```
✅ Issue de daily criada com sucesso!

Título: Daily — [DD/MM/YYYY]
URL: https://github.com/[owner]/[repo]/issues/[id]

Compartilhe o link com o time para preenchimento nos comentários.
```

### 5. Fallback — falha na API

Se a chamada à GitHub API falhar, gerar o conteúdo completo para criação manual:

```
⚠️ Não foi possível criar a Issue via API.
Utilize o conteúdo abaixo para criação manual:

Título: Daily — [DD/MM/YYYY]
Label: chore

[template completo gerado]
```

---

## Output Esperado

### Sucesso via API
```
✅ Issue de daily criada com sucesso!

Título: Daily — [DD/MM/YYYY]
URL: https://github.com/[owner]/[repo]/issues/[id]

Compartilhe o link com o time para preenchimento nos comentários.
```

### Fallback — conteúdo manual
```
⚠️ Não foi possível criar a Issue via API.
Utilize o conteúdo abaixo para criação manual:

[template completo preenchido]
```

---

## Validação

Antes de criar a Issue, verificar:

- [ ] Data correta no título e no corpo da Issue
- [ ] Template de resposta presente e completo
- [ ] Atividades detectadas automaticamente revisadas — remover ruídos como commits de merge ou style
- [ ] Label `chore` aplicada
- [ ] Issue criada como coletiva — nunca individual

---

## Prompt Examples

- "cria a daily de hoje"
- "abre a issue da daily"
- "gera o resumo diário do time"
- "cria a daily assíncrona"
- "quero criar a issue de daily para hoje"

---

## Error Handling

- **Token GitHub inválido ou ausente** — informar que não é possível criar via API e gerar o conteúdo formatado para criação manual
- **Issue de daily já criada para a data** — alertar que já existe uma daily para hoje e perguntar se deseja criar uma nova ou reabrir a existente
- **Nenhuma atividade detectada automaticamente** — informar ao usuário e oferecer a opção de preencher manualmente ou deixar em branco para preenchimento nos comentários