# Release Process

## Visão Geral

O processo de release segue o fluxo do GitFlow, utilizando **GitHub Releases** com tags de versão em `main`. O `CHANGELOG.md` é gerado automaticamente via **Release Drafter**, que analisa os Pull Requests mergeados desde a última release e agrupa as mudanças por tipo.

-----

## Fluxo de Release

```
1. Criar branch release/[versao] a partir de develop
2. Ajustes finais e correções de última hora
3. Abrir Pull Request para main
4. Aprovação e merge em main
5. Release Drafter gera o draft de release automaticamente
6. Revisar e publicar a release no GitHub
7. Tag de versão é criada em main
8. Merge de volta em develop
```

-----

## Versionamento

O projeto adota **Semantic Versioning** (`MAJOR.MINOR.PATCH`):

|Parte  |Quando incrementar                                      |
|-------|--------------------------------------------------------|
|`MAJOR`|Mudanças incompatíveis com versões anteriores           |
|`MINOR`|Novas funcionalidades compatíveis com versões anteriores|
|`PATCH`|Correções de bugs compatíveis com versões anteriores    |

Exemplos: `1.0.0`, `1.1.0`, `1.1.1`, `2.0.0`

-----

## Release Drafter

O **Release Drafter** automatiza a geração do changelog analisando os títulos dos Pull Requests mergeados desde a última release. A cada merge em `main`, ele atualiza automaticamente o draft da próxima release no GitHub.

### Localização

```
[nome-do-projeto]/
└── .github/
    ├── release-drafter.yml
    └── workflows/
        ├── ci-staging.yml
        ├── ci-production.yml
        └── release-drafter.yml
```

### Configuração — `.github/release-drafter.yml`

```yaml
name-template: 'v$RESOLVED_VERSION'
tag-template: 'v$RESOLVED_VERSION'
categories:
  - title: '🚀 Novas Funcionalidades'
    labels:
      - 'feat'
  - title: '🐛 Correções de Bug'
    labels:
      - 'fix'
  - title: '⚡ Melhorias de Performance'
    labels:
      - 'perf'
  - title: '♻️ Refatorações'
    labels:
      - 'refactor'
  - title: '🧪 Testes'
    labels:
      - 'test'
  - title: '📦 Dependências e Configurações'
    labels:
      - 'chore'
  - title: '📝 Documentação'
    labels:
      - 'docs'
change-template: '- $TITLE (#$NUMBER)'
change-title-escapes: '\<*_&'
template: |
  ## O que mudou

  $CHANGES

  **Release completa:** $RELEASE_URL
```

### Workflow — `.github/workflows/release-drafter.yml`

```yaml
name: Release Drafter

on:
  push:
    branches: [main]
  pull_request:
    types: [opened, reopened, synchronize]

jobs:
  update-release-draft:
    runs-on: ubuntu-latest
    permissions:
      contents: write
      pull-requests: write
    steps:
      - uses: release-drafter/release-drafter@v6
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

-----

## Labels nos Pull Requests

Para que o Release Drafter agrupe corretamente as mudanças, cada Pull Request deve ter a label correspondente ao tipo de alteração:

|Label     |Agrupamento                 |
|----------|----------------------------|
|`feat`    |Novas Funcionalidades       |
|`fix`     |Correções de Bug            |
|`perf`    |Melhorias de Performance    |
|`refactor`|Refatorações                |
|`test`    |Testes                      |
|`chore`   |Dependências e Configurações|
|`docs`    |Documentação                |

-----

## Publicação da Release

```
1. Acessar GitHub → Releases → Draft gerado pelo Release Drafter
2. Revisar o changelog gerado
3. Confirmar a versão (MAJOR.MINOR.PATCH)
4. Publicar a release — a tag é criada automaticamente em main
5. Atualizar o CHANGELOG.md com o conteúdo da release publicada
```

-----

## Convenções

- Toda release é criada exclusivamente a partir de `main`
- Tags seguem o padrão `v[MAJOR].[MINOR].[PATCH]` — ex: `v1.0.0`, `v1.1.0`
- O draft gerado pelo Release Drafter deve sempre ser revisado antes da publicação
- Hotfixes geram uma release `PATCH` imediatamente após o merge em `main`
- O `CHANGELOG.md` na raiz do repositório é atualizado manualmente a cada release publicada, com o conteúdo gerado pelo Release Drafter