---
name: onboarding-checklist
description: 'Use this skill when the user asks to generate an onboarding checklist for a new team member. Trigger for prompts like "create an onboarding checklist", "generate onboarding for a new developer", "what does the new member need to do". Do not trigger for README generation or daily summary creation.'
allowed-tools: Read
---

## Guardrails

- **Sem alteração de código ou arquivos do repositório** — apenas leitura de contextos e geração de checklist
- **Sem criação de Issues ou PRs** — o checklist é exibido apenas no chat
- **Sem acesso a arquivos de configuração sensíveis** — nunca ler `appsettings.Production.json`
- **Checklist restrito ao perfil informado** — nunca incluir itens de nível superior sem confirmação
- **Sem atribuição de tarefas ou responsáveis** — apenas gerar o checklist; nunca criar cards automaticamente

# Skill: Onboarding Checklist

## MCP

### 1. Verificar contextos disponíveis via Filesystem MCP

```
list_directory → .github/context/
```

Para cada contexto referenciado no checklist, verificar se o arquivo existe. Omitir o item do checklist e alertar ao final se algum contexto estiver ausente.

---

## Objetivo

Gera um checklist de onboarding personalizado por perfil para novos membros do time, baseado nos contextos e padrões do projeto. O checklist é exibido no chat e adaptado conforme o nível e papel do novo membro.

---

## Contextos Necessários

- [onboarding-summary.md](../../context/documentation/onboarding-summary.md)
- [solution-architecture.md](../../context/architecture/solution-architecture.md)
- [project-structure.md](../../context/architecture/project-structure.md)
- [branching-strategy.md](../../context/engineering-process/branching-strategy.md)
- [commit-standards.md](../../context/engineering-process/commit-standards.md)
- [card-specification.md](../../context/agile/card-specification.md)
- [agile-ceremonies.md](../../context/agile/agile-ceremonies.md)
- [code-review-checklist.md](../../context/engineering-process/code-review-checklist.md)

---

## Entrada

O usuário deve fornecer:

- **Nome do novo membro** — para personalizar o checklist
- **Perfil** — se não informado, perguntar:

```
Qual o perfil do novo membro?
1. Developer Júnior — foco em padrões, fluxo de desenvolvimento e primeiros cards
2. Developer Pleno — foco em arquitetura, padrões avançados e autonomia
3. Developer Sênior — foco em arquitetura, decisões técnicas e mentoria
4. Tech Lead — foco em visão arquitetural, processo e qualidade
```

---

## Passos

### 1. Confirmar entradas
Se nome ou perfil não foram informados, perguntar antes de prosseguir.

### 2. Selecionar itens do checklist por perfil

Cada perfil possui um conjunto base de itens obrigatórios e itens adicionais conforme a senioridade.

---

## Template de Checklist

```markdown
# Onboarding — [Nome do Membro]
**Perfil:** [Perfil]
**Data de início:** [data atual]

---

## 🚀 Setup Inicial
> Aplicável a todos os perfis

- [ ] Leu o `README.md` e configurou o ambiente local
- [ ] Conseguiu rodar o projeto localmente (`dotnet run`)
- [ ] Conseguiu executar os testes (`dotnet test`)
- [ ] Tem acesso ao repositório no GitHub
- [ ] Tem acesso ao board do projeto no GitHub
- [ ] Entende a estrutura de pastas do repositório — consulte `project-structure.md`

---

## 🏛️ Arquitetura
> Aplicável a todos os perfis

- [ ] Leu `solution-architecture.md` e entende o fluxo de dados entre camadas
- [ ] Entende quais objetos pertencem a cada camada — consulte `layer-objects.md`
- [ ] Entende como o AutoMapper é usado entre camadas — consulte `automapper-profiles.md`
- [ ] Leu os contextos das camadas: `layer-presentation.md`, `layer-application.md`, `layer-domain.md`, `layer-infrastructure.md`

### Apenas Developer Pleno, Sênior e Tech Lead
- [ ] Entende os princípios SOLID aplicados no projeto — consulte `solid.md`
- [ ] Entende o Result Pattern e quando aplicá-lo — consulte `result-pattern.md`
- [ ] Entende os patterns Generic Repository e Unit of Work — consulte `generic-repository.md` e `unit-of-work.md`

### Apenas Developer Sênior e Tech Lead
- [ ] Entende as decisões arquiteturais do projeto e seus trade-offs
- [ ] Conhece os contextos de integração — `apis-integrations.md`, `aws-integrations.md`, `kafka-integrations.md`, `rabbit-mq-integrations.md`
- [ ] Entende o padrão de resiliência de mensageria — consulte `messaging-resilience.md`

---

## 💻 Desenvolvimento
> Aplicável a todos os perfis

- [ ] Entende como criar endpoints com Minimal APIs — consulte `minimal-apis.md`
- [ ] Entende como validators são aplicados — consulte `validators.md`
- [ ] Entende como App Services funcionam — consulte `app-services.md`
- [ ] Sabe como a DI é configurada por camada — consulte `dependency-injection.md`
- [ ] Entende os padrões de log do projeto — consulte `logging-standards.md`

### Apenas Developer Pleno, Sênior e Tech Lead
- [ ] Entende o padrão de autenticação e autorização — consulte `auth.md`
- [ ] Entende o tratamento de exceções e o formato de erro padrão — consulte `exception-handling.md`
- [ ] Conhece os padrões de persistência SQL e NoSQL — consulte `ef-standards.md`, `dapper-standards.md`, `query-patterns.md`

### Apenas Developer Sênior e Tech Lead
- [ ] Entende quando usar EF Core vs Dapper e sabe justificar a decisão — consulte `query-patterns.md`
- [ ] Conhece os padrões de documentação de API — consulte `api-documentation.md`

---

## 🧪 Testes
> Aplicável a todos os perfis

- [ ] Entende a arquitetura de testes do projeto — consulte `test-architecture.md`
- [ ] Sabe escrever testes unitários seguindo os padrões — consulte `unit-tests.md`
- [ ] Sabe criar Data Mocks e Mock Classes — consulte `data-mocks.md` e `mock-classes.md`
- [ ] Escreveu ao menos um teste unitário no projeto

### Apenas Developer Pleno, Sênior e Tech Lead
- [ ] Entende e sabe criar testes de integração — consulte `integration-tests.md`

---

## ⚙️ Processo e Git
> Aplicável a todos os perfis

- [ ] Entende o fluxo GitFlow do projeto — consulte `branching-strategy.md`
- [ ] Criou sua primeira branch seguindo a nomenclatura correta
- [ ] Entende o padrão de commits do projeto — consulte `commit-standards.md`
- [ ] Fez seu primeiro commit seguindo o padrão Conventional Commits
- [ ] Entende o processo de Pull Request e revisão de código
- [ ] Leu o checklist de code review — consulte `code-review-checklist.md`

### Apenas Developer Pleno, Sênior e Tech Lead
- [ ] Entende o processo de release — consulte `release-process.md`
- [ ] Entende como o CHANGELOG é mantido — consulte `changelog.md`

### Apenas Tech Lead
- [ ] Entende o pipeline de CI/CD — consulte `ci-cd-overview.md`
- [ ] Conhece as métricas de cobertura exigidas e sabe verificá-las

---

## 🔀 Ágil
> Aplicável a todos os perfis

- [ ] Entende o formato das cerimônias assíncronas — consulte `agile-ceremonies.md`
- [ ] Participou de ao menos uma daily assíncrona
- [ ] Entende como os cards são especificados — consulte `card-specification.md`
- [ ] Abriu seu primeiro card no board do projeto

### Apenas Tech Lead
- [ ] Entende o processo de Sprint Planning — consulte `sprint-planning.md`
- [ ] Consegue conduzir o refinamento e o planning de forma assíncrona

---

## ✅ Marco de Conclusão

O onboarding é considerado concluído quando:

- [ ] Todos os itens do perfil estão marcados
- [ ] O primeiro PR foi aberto, revisado e mergeado em `develop`
- [ ] O primeiro card foi entregue com todos os critérios de aceite validados
```

---

## Output Esperado

Checklist exibido no chat personalizado conforme o perfil do novo membro, com:

- Seções organizadas por tema
- Itens filtrados por perfil — júnior vê menos itens que sênior
- Marco de conclusão ao final

---

## Validação

Antes de entregar o checklist, verificar:

- [ ] Nome do membro e perfil incluídos no cabeçalho
- [ ] Itens filtrados corretamente por perfil — itens de nível superior não aparecem para níveis inferiores
- [ ] Todos os contextos referenciados existem no índice — consulte `indice.md`
- [ ] Marco de conclusão presente ao final
- [ ] Data de início preenchida com a data atual

---

## Prompt Examples

- "cria o checklist de onboarding para o João"
- "gera o onboarding de um novo developer júnior"
- "quero o checklist de entrada para um tech lead"
- "novo membro entrando no time, gera o onboarding"
- "cria o guia de onboarding para a Maria, developer sênior"

---

## Error Handling

- **Nome não informado** — perguntar o nome antes de gerar o checklist
- **Perfil não informado** — nunca assumir o perfil; sempre perguntar antes de prosseguir
- **Perfil não reconhecido** — se o perfil informado não corresponder a nenhum dos quatro definidos, apresentar as opções e aguardar nova resposta
- **Contextos referenciados ausentes** — se algum arquivo de contexto referenciado no checklist não existir no repositório, omitir o item e alertar ao final da geração