# Índice de Contextos

Mapa completo de todos os arquivos de contexto do agente SQUAD, organizados por diretório temático.

---

## 🔀 Agile
| Arquivo | Descrição |
|---------|-----------|
| [agile-ceremonies.md](agile/agile-ceremonies.md) | Cerimônias ágeis conduzidas de forma assíncrona via GitHub |
| [card-specification.md](agile/card-specification.md) | Templates de cards por tipo: feature, bug, tech debt e spike |
| [sprint-planning.md](agile/sprint-planning.md) | Processo de planejamento de sprint com Story Points |

---

## 🏛️ Architecture
| Arquivo | Descrição |
|---------|-----------|
| [solution-architecture.md](architecture/solution-architecture.md) | Visão geral da arquitetura, estrutura da solution e fluxo de dados |
| [project-structure.md](architecture/project-structure.md) | Organização de pastas e arquivos no repositório |
| [layer-presentation.md](architecture/layer-presentation.md) | Responsabilidades e componentes da camada de Presentation |
| [layer-application.md](architecture/layer-application.md) | Responsabilidades e componentes da camada de Application |
| [layer-domain.md](architecture/layer-domain.md) | Responsabilidades e componentes da camada de Domain |
| [layer-infrastructure.md](architecture/layer-infrastructure.md) | Responsabilidades e componentes da camada de Infrastructure |
| [layer-objects.md](architecture/layer-objects.md) | Quais objetos são utilizados em cada camada |
| [automapper-profiles.md](architecture/automapper-profiles.md) | Mapeamento entre objetos de camadas via AutoMapper |

---

## 💻 Development
| Arquivo | Descrição |
|---------|-----------|
| [app-services.md](development/app-services.md) | Camada de orquestração entre Presentation e Application |
| [minimal-apis.md](development/minimal-apis.md) | Organização e implementação de endpoints com Minimal APIs |
| [validators.md](development/validators.md) | Validação de requests com FluentValidation e filtro global |
| [dependency-injection.md](development/dependency-injection.md) | Configuração de DI por camada via XDependency.cs |
| [auth.md](development/auth.md) | Autenticação Basic Auth e autorização Bearer JWT |
| [exception-handling.md](development/exception-handling.md) | Tratamento centralizado de exceções com ProblemDetails |
| [logging-standards.md](development/logging-standards.md) | Padrões de log com ILogger e CorrelationId |
| [api-documentation.md](development/api-documentation.md) | Documentação de endpoints via Swagger/OpenAPI |

---

## 📝 Documentation
| Arquivo | Descrição |
|---------|-----------|
| [readme-template.md](documentation/readme-template.md) | Estrutura e padrões do README.md do repositório |
| [changelog-template.md](documentation/changelog-template.md) | Estrutura e padrões do CHANGELOG.md |
| [onboarding-summary.md](documentation/onboarding-summary.md) | Guia de entrada para novos desenvolvedores |

---

## 🔌 Integrations
| Arquivo | Descrição |
|---------|-----------|
| [apis-integrations.md](integrations/apis-integrations.md) | Integrações com APIs externas via HttpClient e Polly |
| [aws-integrations.md](integrations/aws-integrations.md) | Integrações com serviços AWS via AWSSDK |
| [kafka-integrations.md](integrations/kakfa-integrations.md) | Producers e consumers Kafka via Confluent.Kafka |
| [rabbit-mq-integrations.md](integrations/rabbit-mq-integrations.md) | Producers e consumers RabbitMQ via RabbitMQ.Client |
| [messaging-resilience.md](integrations/messaging-resilience.md) | Padrão de resiliência com três filas e circuit breaker |

---

## 🧩 Patterns
| Arquivo | Descrição |
|---------|-----------|
| [solid.md](patterns/solid.md) | Princípios SOLID com exemplos práticos |
| [builder.md](patterns/builder.md) | Pattern Builder para construção de objetos complexos |
| [result-pattern.md](patterns/result-pattern.md) | Tratamento de erros de negócio sem exceções |
| [generic-repository.md](patterns/generic-repository.md) | Repositório genérico para SQL e NoSQL |
| [unit-of-work.md](patterns/unit-of-work.md) | Gerenciamento de transações e repositórios |

---

## 🗄️ Persistence
| Arquivo | Descrição |
|---------|-----------|
| [sql.md](persistence/sql.md) | Padrões de acesso a dados relacionais |
| [nosql.md](persistence/nosql.md) | Padrões de acesso a dados com MongoDB |
| [ef-standards.md](persistence/ef-standards.md) | Padrões de uso do Entity Framework Core |
| [dapper-standards.md](persistence/dapper-standards.md) | Padrões de uso do Dapper para queries customizadas |
| [query-patterns.md](persistence/query-patterns.md) | Padrões de construção e organização de queries |

---

## ⚙️ Engineering Process
| Arquivo | Descrição |
|---------|-----------|
| [branching-strategy.md](engineering-process/branching-strategy.md) | GitFlow — branches, nomenclatura e fluxo de trabalho |
| [commit-standards.md](engineering-process/commit-standards.md) | Conventional Commits em português |
| [code-review-checklist.md](engineering-process/code-review-checklist.md) | Checklist de revisão de Pull Requests |
| [ci-cd-overview.md](engineering-process/ci-cd-overview.md) | Pipelines de CI com GitHub Actions |
| [release-process.md](engineering-process/release-process.md) | Processo de criação e publicação de releases |

---

## 🧪 Testing
| Arquivo | Descrição |
|---------|-----------|
| [test-architecture.md](testing/tests-architecture.md) | Estrutura dos projetos de teste por camada |
| [unit-tests.md](testing/unit-tests.md) | Padrões de escrita de testes unitários com xUnit e Shouldly |
| [mock-classes.md](testing/mock-classes.md) | Mock classes de dependências com Moq e pattern Builder |
| [data-mocks.md](testing/data-mocks.md) | Objetos de teste reutilizáveis por cenário |

---

**Total: 46 arquivos de contexto** organizados em **9 diretórios**