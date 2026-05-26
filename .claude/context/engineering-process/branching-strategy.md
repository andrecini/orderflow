# Branching Strategy

## Visão Geral

O projeto adota o **GitFlow** como estratégia de branching. O fluxo é baseado em branches de longa duração (`main` e `develop`) e branches de curta duração para features, releases, bugfixes e hotfixes. As mensagens de commit seguem o padrão **Conventional Commits** — documentado em arquivo de contexto específico.

-----

## Branches Principais

|Branch   |Descrição                                         |
|---------|--------------------------------------------------|
|`main`   |Código em produção — nunca recebe commits diretos |
|`develop`|Branch de integração — base para todas as features|

-----

## Branches de Suporte

### Feature

Criada a partir de `develop`. Usada para desenvolvimento de novas funcionalidades.

```
feature/[yyyyMM]/[descricao]
```

Exemplos:

```
feature/202503/create-order-endpoint
feature/202503/payment-gateway-integration
feature/202504/add-order-status-filter
```

### Release

Criada a partir de `develop` quando o conjunto de features de uma versão está completo. Usada para ajustes finais antes de ir para produção.

```
release/[versao]
```

Exemplos:

```
release/1.0.0
release/1.1.0
```

Ao finalizar, é mergeada em `main` e `develop`, e uma tag de versão é criada em `main`.

### Bugfix

Criada a partir de `develop`. Usada para correção de bugs identificados antes do release.

```
bugfix/[yyyyMM]/[descricao]
```

Exemplos:

```
bugfix/202503/fix-order-total-calculation
bugfix/202504/fix-null-reference-on-payment
```

### Hotfix

Criada a partir de `main`. Usada para correções urgentes em produção.

```
hotfix/[yyyyMM]/[descricao]
```

Exemplos:

```
hotfix/202503/fix-authentication-token-expiry
hotfix/202504/fix-critical-order-duplication
```

Ao finalizar, é mergeada em `main` e `develop`, e uma nova tag de versão é criada em `main`.

-----

## Fluxo GitFlow

```
main ←────────────────────────────────── hotfix/yyyyMM/descricao
  ↑                                              ↑
  └──────── release/versao ──────────────→ develop
                                              ↑     ↑
                               feature/yyyyMM/x   bugfix/yyyyMM/x
```

-----

## Regras

- `main` e `develop` são branches protegidas — não recebem commits diretos
- Todo merge em `main` ou `develop` é feito via **Pull Request**
- Pull Requests exigem ao menos uma aprovação antes do merge
- Branches de feature e bugfix são deletadas após o merge
- Tags de versão seguem **Semantic Versioning** (`MAJOR.MINOR.PATCH`) e são criadas sempre em `main`
- O board do GitHub é utilizado para rastreamento de tarefas — issues devem ser linkadas ao PR correspondente

-----

## Ciclo de Vida de uma Feature

```
1. Criar branch a partir de develop:
   git checkout -b feature/202503/create-order-endpoint develop

2. Desenvolver e commitar seguindo Conventional Commits

3. Abrir Pull Request para develop

4. Revisão e aprovação

5. Merge em develop

6. Deletar branch de feature
```