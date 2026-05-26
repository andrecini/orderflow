# Índice de Skills — Claude Code

Mapa completo das skills disponíveis para o agente SQUAD no Claude Code. Invoque com `/nome-da-skill`.

---

## Criação de Artefatos

| Skill | Arquivo | Descrição |
|-------|---------|-----------|
| `/create-feature` | `create-feature/SKILL.md` | Feature completa ponta a ponta — endpoint, service, repository e testes |
| `/create-endpoint` | `create-endpoint/SKILL.md` | Endpoint isolado com Minimal API, Validator, AppService e Swagger |
| `/create-service` | `create-service/SKILL.md` | Service com interface no Domain e implementação no Application |
| `/create-repository` | `create-repository/SKILL.md` | Repository com decisão automática EF Core vs Dapper |
| `/create-migration` | `create-migration/SKILL.md` | Migration EF Core com inspeção de schema |
| `/create-dapper-query` | `create-dapper-query/SKILL.md` | Query Dapper com constante no Domain e implementação no repositório |
| `/create-integration` | `create-integration/SKILL.md` | Integração externa — API, AWS, Kafka ou RabbitMQ |

---

## Testes

| Skill | Arquivo | Descrição |
|-------|---------|-----------|
| `/create-unit-test` | `create-unit-test/SKILL.md` | Conjunto completo — Data Mock, Mock Class e Teste |
| `/create-integration-test` | `create-integration-test/SKILL.md` | Testes de integração com WebApplicationFactory e rollback |
| `/check-coverage` | `check-coverage/SKILL.md` | Execução de testes e análise de cobertura (meta: 85%) |

---

## Qualidade e Padrões

| Skill | Arquivo | Descrição |
|-------|---------|-----------|
| `/code-review` | `code-review/SKILL.md` | Review estruturado com Blockers, Warnings e Suggestions |
| `/check-standards` | `check-standards/SKILL.md` | Diagnóstico de aderência aos padrões sem alterações |
| `/refactor-to-standards` | `refactor-to-standards/SKILL.md` | Refatoração com opção keep/undo por arquivo |

---

## Documentação e Git

| Skill | Arquivo | Descrição |
|-------|---------|-----------|
| `/write-readme` | `write-readme/SKILL.md` | Geração ou atualização do README.md |
| `/write-commit` | `write-commit/SKILL.md` | Mensagem de commit no padrão Conventional Commits |
| `/write-changelog-entry` | `write-changelog-entry/SKILL.md` | Entrada no CHANGELOG.md com sugestão de versão |

---

## Ágil e Processo

| Skill | Arquivo | Descrição |
|-------|---------|-----------|
| `/create-card` | `create-card/SKILL.md` | Issue no GitHub seguindo templates por tipo |
| `/daily-summary` | `daily-summary/SKILL.md` | Issue de daily assíncrona coletiva |
| `/onboarding-checklist` | `onboarding-checklist/SKILL.md` | Checklist de onboarding personalizado por perfil |

---

## Encadeamento Recomendado

```
Nova feature:
  create-card → create-feature → create-migration → create-unit-test → code-review → write-commit

Revisão de qualidade:
  check-standards → refactor-to-standards → check-coverage

Release:
  write-changelog-entry
```

**Total: 19 skills** em 4 categorias.
