# Agile Ceremonies

## Visão Geral

As cerimônias ágeis são conduzidas de forma **assíncrona via GitHub**, utilizando Issues, comentários e o board do projeto. O objetivo é manter o ritmo do time sem depender de reuniões síncronas, garantindo rastreabilidade de todas as decisões e discussões no próprio repositório.

---

## Sprint Planning

Conduzido no início de cada sprint. Consulte `sprint-planning.md` para detalhes completos.

**Como funciona no GitHub:**
- O Scrum Master abre uma Issue do tipo `chore` com o título `Sprint Planning — [período]`
- A Issue lista os cards candidatos ao sprint com seus Story Points
- Os membros do time comentam na Issue para validar estimativas, sinalizar impedimentos ou sugerir ajustes
- Após alinhamento, o Scrum Master move os cards aprovados para a coluna `In Progress` do board e fecha a Issue de planning

---

## Daily

Atualização diária assíncrona do status de cada membro do time.

**Como funciona no GitHub:**
- O Scrum Master abre uma Issue do tipo `chore` com o título `Daily — [data]` no início de cada dia
- Cada membro comenta na Issue respondendo:
  - O que fez desde a última daily
  - O que fará hoje
  - Se há algum impedimento
- Impedimentos identificados são transformados em Issues separadas com a label `impediment` para rastreamento e resolução
- A Issue de daily é fechada ao final do dia pelo Scrum Master

```markdown
## Daily — [data]

**O que fiz:**
- [atividade 1]
- [atividade 2]

**O que farei hoje:**
- [atividade 1]

**Impedimentos:**
- [impedimento ou "nenhum"]
```

---

## Refinamento

Cerimônia de preparação do backlog para os próximos sprints. Garante que os cards estejam bem especificados e estimados antes do planning.

**Como funciona no GitHub:**
- O Product Owner abre uma Issue do tipo `chore` com o título `Refinamento — [data]` listando os cards a serem refinados
- Cada card candidato é discutido nos comentários da Issue — dúvidas, ajustes de escopo e definição de tasks técnicas
- Após refinamento, os cards são atualizados com critérios de aceite, tasks e estimativas conforme `card-specification.md` e `sprint-planning.md`
- A Issue de refinamento é fechada quando todos os cards listados estiverem com o status **Definition of Ready** atingido

---

## Sprint Review

Cerimônia de validação das entregas do sprint. O time apresenta o que foi concluído e valida com os critérios de aceite.

**Como funciona no GitHub:**
- O Scrum Master abre uma Issue do tipo `chore` com o título `Sprint Review — [período]`
- Cada card concluído no sprint é listado com link para o PR mergeado
- O Product Owner valida os critérios de aceite de cada card e registra sua aprovação ou pendências nos comentários
- Cards não aprovados retornam ao backlog com comentário descrevendo o que precisa ser ajustado
- A Issue de review é fechada após validação de todos os cards do sprint

```markdown
## Sprint Review — [período]

### Entregues
- [ ] #42 — Adicionar endpoint de criação de pedido — PR #85
- [ ] #57 — Integrar gateway de pagamento — PR #91

### Pendências
- [ ] [cards que não foram concluídos e motivo]
```

---

## Retrospectiva

Cerimônia de melhoria contínua. O time reflete sobre o sprint encerrado e define ações concretas de melhoria.

**Como funciona no GitHub:**
- O Scrum Master abre uma Issue do tipo `chore` com o título `Retrospectiva — [período]`
- Os membros comentam na Issue seguindo a estrutura abaixo
- O Scrum Master consolida os pontos levantados e define as ações de melhoria
- Cada ação de melhoria aprovada é transformada em uma Issue de `tech debt` ou `chore` para rastreamento
- A Issue de retrospectiva é fechada após consolidação das ações

```markdown
## Retrospectiva — [período]

**O que foi bem:**
- [ponto positivo]

**O que pode melhorar:**
- [ponto de melhoria]

**Ações para o próximo sprint:**
- [ ] [ação concreta com responsável]
```

---

## Convenções

- Todas as cerimônias são registradas como Issues no GitHub com a label `chore`
- Issues de cerimônias são fechadas pelo Scrum Master ao final de cada cerimônia
- Impedimentos identificados nas dailies são sempre rastreados como Issues separadas — nunca ficam apenas nos comentários
- Ações da retrospectiva são sempre transformadas em Issues rastreáveis — nunca ficam apenas como anotações
- O board do GitHub é a fonte de verdade do status do sprint — deve ser mantido atualizado ao longo do dia