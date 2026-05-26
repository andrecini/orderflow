# Card Specification

## Visão Geral

Os cards do projeto são criados como **Issues no GitHub** e seguem templates específicos por tipo. Cada tipo possui uma estrutura padronizada para garantir clareza sobre o que deve ser feito, por que e como validar. Os cards são referenciados nos commits via `CARD: #[id]` e nos Pull Requests.

---

## Tipos de Cards

| Tipo | Label | Quando usar |
|------|-------|-------------|
| Feature | `feat` | Nova funcionalidade a ser desenvolvida |
| Bug | `fix` | Comportamento incorreto identificado |
| Tech Debt | `chore` | Melhoria técnica sem alteração de comportamento visível |
| Spike | `spike` | Investigação ou prova de conceito antes do desenvolvimento |

---

## Template — Feature

```markdown
## Descrição

> Descreva de forma clara e objetiva o que deve ser desenvolvido e qual valor entrega.

---

## Critérios de Aceite

**Cenário 1: [Nome do cenário]**
- **Dado** que [contexto inicial]
- **Quando** [ação realizada]
- **Então** [resultado esperado]

**Cenário 2: [Nome do cenário]**
- **Dado** que [contexto inicial]
- **Quando** [ação realizada]
- **Então** [resultado esperado]

---

## Tasks Técnicas

- [ ] tarefa 1
- [ ] tarefa 2
- [ ] tarefa 3

---

## Observações

> Informações adicionais, dependências, links úteis ou decisões técnicas relevantes.
```

---

## Template — Bug

```markdown
## Descrição

> Descreva o comportamento incorreto observado e qual o comportamento esperado.

---

## Como Reproduzir

1. passo 1
2. passo 2
3. passo 3

**Comportamento atual:** [o que acontece]
**Comportamento esperado:** [o que deveria acontecer]

---

## Critérios de Aceite

**Cenário 1: [Nome do cenário]**
- **Dado** que [contexto inicial]
- **Quando** [ação realizada]
- **Então** [resultado esperado]

---

## Tasks Técnicas

- [ ] tarefa 1
- [ ] tarefa 2

---

## Observações

> Logs, screenshots, ambiente onde o bug foi identificado, ou qualquer informação relevante para a investigação.
```

---

## Template — Tech Debt

```markdown
## Descrição

> Descreva o problema técnico atual e o que precisa ser melhorado. Explique o impacto de não resolver.

---

## Motivação

> Por que isso precisa ser feito agora? Qual risco ou limitação técnica está sendo endereçado?

---

## Critérios de Aceite

**Cenário 1: [Nome do cenário]**
- **Dado** que [contexto inicial]
- **Quando** [ação realizada]
- **Então** [resultado esperado]

---

## Tasks Técnicas

- [ ] tarefa 1
- [ ] tarefa 2

---

## Observações

> Referências, documentação técnica relevante ou decisões de arquitetura relacionadas.
```

---

## Template — Spike

```markdown
## Descrição

> Descreva a dúvida técnica ou decisão que precisa ser investigada antes do desenvolvimento.

---

## Objetivo

> O que se espera descobrir ou decidir ao final do spike?

---

## Critérios de Conclusão

- [ ] critério 1
- [ ] critério 2
- [ ] critério 3

---

## Tasks de Investigação

- [ ] tarefa 1
- [ ] tarefa 2

---

## Resultado Esperado

> Qual o entregável do spike? Ex: documento de decisão, prova de conceito, atualização de arquivo de contexto.

---

## Observações

> Links, referências ou contexto adicional relevante para a investigação.
```

---

## Convenções

- Todo card deve ter a label correspondente ao seu tipo
- Cards de feature e bug devem ter ao menos um critério de aceite no formato **Given/When/Then**
- Tasks técnicas são livres — devem refletir o trabalho real necessário para concluir o card
- O campo **Observações** é opcional mas recomendado quando houver dependências ou decisões relevantes
- Spikes devem sempre gerar um entregável concreto — nunca ficam sem resultado documentado
- Todo Pull Request deve referenciar o card correspondente via `Closes #[id]` na descrição