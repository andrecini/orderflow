# Onboarding Summary

## Bem-vindo ao projeto!

Este documento é o ponto de entrada para novos desenvolvedores. Ele apresenta o projeto, orienta sobre os padrões adotados e referencia os documentos de contexto que devem ser lidos antes de começar a contribuir.

---

## Antes de começar

Leia o `README.md` na raiz do repositório para configurar o ambiente local, entender os pré-requisitos e aprender como rodar o projeto e os testes.

---

## Arquitetura

| Documento | O que cobre |
|-----------|-------------|
| `solution-architecture.md` | Visão geral da arquitetura, estrutura da solution e fluxo de dados |
| `project-structure.md` | Organização de pastas e arquivos no repositório |
| `layer-presentation.md` | Responsabilidades e componentes da camada de Presentation |
| `layer-application.md` | Responsabilidades e componentes da camada de Application |
| `layer-domain.md` | Responsabilidades e componentes da camada de Domain |
| `layer-infrastructure.md` | Responsabilidades e componentes da camada de Infrastructure |
| `layer-objects.md` | Quais objetos são utilizados em cada camada |
| `automapper-profiles.md` | Como o mapeamento entre objetos de camadas é feito |

---

## Padrões de Desenvolvimento

| Documento | O que cobre |
|-----------|-------------|
| `solid.md` | Princípios SOLID adotados no projeto |
| `result-pattern.md` | Como erros de negócio são tratados sem exceções |
| `builder.md` | Pattern Builder — quando e como usar |
| `generic-repository.md` | Pattern de repositório genérico para SQL e NoSQL |
| `unit-of-work.md` | Gerenciamento de transações e repositórios |
| `dependency-injection.md` | Como a DI é configurada por camada |
| `app-services.md` | Camada de orquestração entre Presentation e Application |
| `minimal-apis.md` | Como os endpoints são organizados e implementados |
| `validators.md` | Como a validação de requests é feita |
| `api-documentation.md` | Padrões de documentação de endpoints via Swagger |
| `auth.md` | Autenticação e autorização com Basic Auth e Bearer JWT |
| `exception-handling.md` | Tratamento centralizado de exceções inesperadas |
| `logging-standards.md` | Padrões de log com ILogger e CorrelationId |

---

## Integrações

| Documento | O que cobre |
|-----------|-------------|
| `apis-integrations.md` | Integrações com APIs externas via HttpClient |
| `aws-integrations.md` | Integrações com serviços AWS via AWSSDK |
| `kafka-integrations.md` | Producers e consumers Kafka via Confluent.Kafka |
| `rabbit-mq-integrations.md` | Producers e consumers RabbitMQ via RabbitMQ.Client |
| `messaging-resilience.md` | Padrão de resiliência com três filas e circuit breaker |

---

## Testes

| Documento | O que cobre |
|-----------|-------------|
| `test-architecture.md` | Estrutura dos projetos de teste por camada |
| `unit-tests.md` | Como os testes unitários são escritos |
| `mock-classes.md` | Como as dependências são mockadas com Moq |
| `data-mocks.md` | Como os objetos de teste são construídos e reutilizados |

---

## Processo e Qualidade

| Documento | O que cobre |
|-----------|-------------|
| `branching-strategy.md` | GitFlow — branches, nomenclatura e fluxo de trabalho |
| `commit-standards.md` | Conventional Commits em português |
| `code-review-checklist.md` | O que verificar ao revisar um Pull Request |
| `card-specification.md` | Templates de cards por tipo (feature, bug, tech debt, spike) |
| `ci-cd-overview.md` | Pipelines de CI com GitHub Actions |
| `release-process.md` | Como as releases são criadas e publicadas |
| `changelog.md` | Como o CHANGELOG.md é estruturado e atualizado |

---

## Primeiros passos recomendados

1. Configure o ambiente local seguindo o `README.md`
2. Leia `solution-architecture.md` para entender a estrutura geral
3. Leia `layer-objects.md` e `automapper-profiles.md` para entender o fluxo de dados
4. Leia `branching-strategy.md` e `commit-standards.md` antes de criar sua primeira branch
5. Leia `code-review-checklist.md` antes de abrir seu primeiro Pull Request
6. Consulte os demais documentos conforme a necessidade do que estiver desenvolvendo