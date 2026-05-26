# Changelog

## Visão Geral

O `CHANGELOG.md` registra todas as mudanças relevantes do projeto organizadas por versão. Ele é atualizado a cada release publicada, com base no draft gerado automaticamente pelo Release Drafter. O formato é baseado no **Keep a Changelog** com categorias alinhadas aos tipos do Conventional Commits adotados no projeto.

-----

## Localização

```
[nome-do-projeto]/
└── CHANGELOG.md
```

-----

## Estrutura do Arquivo

```markdown
# Changelog

Todas as mudanças relevantes deste projeto serão documentadas neste arquivo.

## [Unreleased]

## [1.1.0] - 2024-04-10

### 🚀 Novas Funcionalidades
- Adicionar endpoint de criação de pedido (#42)
- Integrar gateway de pagamento (#57)

### 🐛 Correções de Bug
- Corrigir validação de token expirado (#63)

### ♻️ Refatorações
- Extrair lógica de cálculo de frete para ValueObject (#71)

### 📦 Dependências e Configurações
- Atualizar pacotes NuGet para versões mais recentes (#48)

## [1.0.0] - 2024-03-15

### 🚀 Novas Funcionalidades
- Implementar autenticação via Basic Auth e Bearer JWT (#10)
- Adicionar endpoint de listagem de pedidos (#12)
- Configurar pipeline de CI com GitHub Actions (#15)

### 📝 Documentação
- Adicionar README com instruções de setup (#20)
```

-----

## Categorias

|Categoria                     |Tipo de Commit|Quando usar                                 |
|------------------------------|--------------|--------------------------------------------|
|🚀 Novas Funcionalidades       |`feat`        |Funcionalidades novas entregues na versão   |
|🐛 Correções de Bug            |`fix`         |Bugs corrigidos                             |
|⚡ Melhorias de Performance    |`perf`        |Otimizações de performance                  |
|♻️ Refatorações                |`refactor`    |Refatorações sem alteração de comportamento |
|🧪 Testes                      |`test`        |Adições ou correções relevantes de testes   |
|📦 Dependências e Configurações|`chore`       |Atualizações de dependências e configurações|
|📝 Documentação                |`docs`        |Alterações em documentação                  |

-----

## Convenções

- Sempre em **português**
- A seção `[Unreleased]` fica no topo e acumula mudanças ainda não publicadas — é populada automaticamente pelo Release Drafter
- A cada release publicada, o conteúdo de `[Unreleased]` é movido para uma nova seção versionada com a data de publicação
- Versões são listadas em ordem decrescente — a mais recente sempre no topo
- Cada entrada referencia o número do Pull Request entre parênteses — ex: `(#42)`
- Categorias sem entradas são omitidas da versão
- Mudanças internas sem impacto para o consumidor da API (`style`, `revert`) não são incluídas no changelog