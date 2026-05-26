---
name: create-card
description: 'Use this skill when the user asks to create a GitHub Issue. Trigger for prompts like "create a card", "open an issue", "add a task to the board", "create a bug report". Do not trigger for code creation, PR reviews, or commit generation.'
allowed-tools: Bash(gh *)
---

## Guardrails

- **Escopo restrito à criação de Issues** — nunca criar PRs, comentários em código ou qualquer outro artefato do GitHub
- **Sem acesso ao código-fonte** — apenas leitura do repositório para contexto; nunca modificar arquivos
- **Sem criação de Issues em repositórios externos** — apenas no repositório atual
- **Sem atribuição automática de responsáveis** — nunca atribuir assignees sem confirmação do usuário
- **Sem definição automática de milestones ou projetos** — apenas title, body e labels
- **Labels restritas ao conjunto definido** — apenas `feat`, `fix`, `chore`, `spike`

# Skill: Create Card

## MCP

### 1. Criar Issue via GitHub MCP

```
create_issue → criar a Issue com title, body e labels
```
Se a tool falhar, gerar cont conteúdo formatado para criação manual.

---

## Objetivo

Guia a criação de uma Issue no GitHub seguindo os templates definidos por tipo de card. A Issue é criada diretamente via GitHub API. Em caso de falha, gera o conteúdo formatado para preenchimento manual.

---

## Contextos Necessários

- [card-specification.md](../../context/agile/card-specification.md)
- [sprint-planning.md](../../context/agile/sprint-planning.md)

---

## Entrada

O usuário deve fornecer:

- **Tipo do card** — Feature, Bug, Tech Debt ou Spike. Se não informado, perguntar:

```
Qual o tipo do card?
1. Feature — nova funcionalidade
2. Bug — comportamento incorreto
3. Tech Debt — melhoria técnica
4. Spike — investigação ou prova de conceito
```

- **Título** — descrição breve do card
- **Detalhes** — informações suficientes para preencher o template correspondente

Se os detalhes forem insuficientes para preencher alguma seção, perguntar antes de criar.

---

## Passos

### 1. Confirmar entradas
Se tipo ou título não foram informados, perguntar antes de prosseguir.

### 2. Selecionar template
Conforme o tipo informado, selecionar o template correspondente em [card-specification.md](../../context/agile/card-specification.md):

| Tipo | Template | Label |
|------|----------|-------|
| Feature | Template Feature | `feat` |
| Bug | Template Bug | `fix` |
| Tech Debt | Template Tech Debt | `chore` |
| Spike | Template Spike | `spike` |

### 3. Preencher template
Preencher todas as seções do template com base nas informações fornecidas pelo usuário:

- **Feature:** Descrição, Critérios de Aceite (Given/When/Then), Tasks Técnicas, Observações
- **Bug:** Descrição, Como Reproduzir, Critérios de Aceite, Tasks Técnicas, Observações
- **Tech Debt:** Descrição, Motivação, Critérios de Aceite, Tasks Técnicas, Observações
- **Spike:** Descrição, Objetivo, Critérios de Conclusão, Tasks de Investigação, Resultado Esperado, Observações

### 4. Criar Issue via GitHub API

```
POST /repos/{owner}/{repo}/issues
Authorization: Bearer {GITHUB_TOKEN}
Content-Type: application/json

{
  "title": "[Tipo]: [Título]",
  "body": "[conteúdo do template preenchido]",
  "labels": ["[label correspondente ao tipo]"]
}
```

Se a chamada for bem-sucedida, retornar:
- URL da Issue criada
- Número da Issue para referência nos commits (`CARD: #[id]`)

### 5. Fallback — falha na API
Se a chamada à GitHub API falhar, gerar o conteúdo completo formatado para preenchimento manual:

```markdown
## Como criar manualmente

1. Acesse: GitHub → Issues → New Issue
2. Título: [título gerado]
3. Label: [label correspondente]
4. Conteúdo:

[template preenchido completo]
```

---

## Output Esperado

### Sucesso via API
```
✅ Issue criada com sucesso!

Título: [Tipo]: [Título]
URL: https://github.com/[owner]/[repo]/issues/[id]
Referência para commits: CARD: #[id]
```

### Fallback — conteúdo manual
```
⚠️ Não foi possível criar a Issue via API.
Utilize o conteúdo abaixo para criação manual:

[conteúdo completo do template preenchido]
```

---

## Validação

Antes de criar a Issue, verificar:

- [ ] Tipo do card definido e label correspondente selecionada
- [ ] Título claro e objetivo
- [ ] Template preenchido com todas as seções obrigatórias
- [ ] Ao menos um critério de aceite no formato Given/When/Then (Feature, Bug e Tech Debt)
- [ ] Tasks técnicas definidas
- [ ] Spike possui Resultado Esperado preenchido — consulte [card-specification.md](../../context/agile/card-specification.md)

---

## Prompt Examples

- "cria um card de feature para o endpoint de pedidos"
- "abre uma issue de bug para o erro de autenticação"
- "adiciona um tech debt para refatorar o OrderService"
- "cria um spike para investigar o uso de Redis"
- "quero criar uma tarefa no board"

---

## Related Skills

- `create-feature` — iniciar o desenvolvimento após a criação do card
- `write-commit` — referenciar o card criado no commit correspondente

---

## Error Handling

- **Token GitHub inválido ou ausente** — informar que não é possível criar via API e gerar o conteúdo formatado para criação manual
- **Tipo de card não informado** — nunca assumir o tipo; sempre perguntar antes de prosseguir
- **Título não informado** — nunca gerar um card sem título; sempre solicitar ao usuário
- **Critérios de aceite insuficientes** — se a descrição fornecida não permitir gerar ao menos um critério de aceite no formato Given/When/Then, perguntar mais detalhes antes de criar