# CI/CD Overview

## Visão Geral

As pipelines de CI/CD são configuradas via **GitHub Actions**. O projeto possui dois workflows: `ci.yml` (validação de Pull Requests) e `deploy-staging.yml` (deploy ao fazer merge na `main`). As pipelines cobrem build, testes unitários, validação de cobertura de código e análise estática via SonarCloud.

---

## Localização

```
[nome-do-projeto]/
└── .github/
    └── workflows/
        ├── ci.yml               — dispara em Pull Requests
        └── deploy-staging.yml   — dispara em push para main
```

---

## Ferramentas

| Ferramenta           | Propósito                                                     |
|----------------------|---------------------------------------------------------------|
| GitHub Actions       | Orquestração das pipelines                                    |
| coverlet.msbuild     | Coleta e validação de cobertura (threshold enforcement)       |
| dotnet-sonarscanner  | Análise estática e envio para SonarCloud                      |
| SonarCloud           | Qualidade de código, cobertura e Quality Gate                 |
| Java 17              | Requisito do dotnet-sonarscanner                              |

---

## Stack de Cobertura

O projeto usa **`coverlet.msbuild`** com formato **OpenCover** — não `coverlet.collector` nem `dotnet-coverage merge`.

- `coverlet.msbuild` é referenciado como `PackageReference` em cada projeto de testes
- O formato OpenCover é obrigatório para integração com SonarCloud (`sonar.cs.opencover.reportsPaths`)
- O threshold é validado pelo próprio `coverlet.msbuild` via `/p:Threshold=85`

```bash
dotnet test [solucao].slnx \
  --configuration Release \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat=opencover \
  "-p:CoverletOutput=./TestResults/" \
  -p:Threshold=85 \
  -p:ThresholdType=line \
  -p:ThresholdStat=Total
```

---

## Integração com SonarCloud

### Fluxo obrigatório

O `dotnet-sonarscanner` exige um único job com a sequência: `begin` → `build` → `test` → `end`. Os steps de build e test **devem estar dentro do mesmo job** — caso contrário o scanner não consegue capturar os artefatos.

```
SonarCloud Begin → Build → Run Tests with Coverage → SonarCloud End
```

### Parâmetros importantes

- **`sonar.qualitygate.wait=true`** deve ser passado no **`begin`**, não no `end`
  - A partir da v11 do SonarScanner para .NET, esse parâmetro é inválido no `end` (erro de compilação na pipeline)
  - Com esse parâmetro, o `sonarscanner end` aguarda o resultado do Quality Gate e falha o step se o gate não passar
- **`fetch-depth: 0`** é obrigatório no `checkout` para que o SonarCloud analise o histórico de blame
- **Java 17** é obrigatório para executar o scanner

### Secrets e Variables necessárias

| Nome                 | Tipo     | Descrição                                              |
|----------------------|----------|--------------------------------------------------------|
| `SONAR_TOKEN`        | Secret   | Token de autenticação gerado em sonarcloud.io          |
| `SONAR_PROJECT_KEY`  | Variable | Chave do projeto no SonarCloud (ex: `org_sticker-manager`) |
| `SONAR_ORGANIZATION` | Variable | Slug da organização no SonarCloud (ex: `andrecini`)    |

---

## Workflows

### ci.yml — Pull Request

Dispara em Pull Requests para `main` ou `develop`. Bloqueia o merge se build, testes, threshold de cobertura ou Quality Gate do SonarCloud falharem.

```yaml
name: CI — Build and Test

on:
  pull_request:
    branches:
      - main
      - develop

jobs:
  build-and-test:
    name: Build, Test and Analyze
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0                          # obrigatório para SonarCloud blame

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"

      - uses: actions/setup-java@v4               # obrigatório para dotnet-sonarscanner
        with:
          distribution: temurin
          java-version: "17"

      - run: dotnet tool install --global dotnet-sonarscanner

      - run: dotnet restore [solucao].slnx

      - name: SonarCloud Begin
        run: >
          dotnet sonarscanner begin
          /k:"${{ vars.SONAR_PROJECT_KEY }}"
          /o:"${{ vars.SONAR_ORGANIZATION }}"
          /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
          /d:sonar.host.url="https://sonarcloud.io"
          /d:sonar.cs.opencover.reportsPaths="**/TestResults/coverage.opencover.xml"
          /d:sonar.exclusions="**/Migrations/**,**/obj/**,**/bin/**"
          /d:sonar.qualitygate.wait=true           # DEVE estar no begin, não no end

      - run: dotnet build [solucao].slnx --no-restore --configuration Release

      - name: Run tests with coverage
        run: >
          dotnet test [solucao].slnx
          --no-build
          --configuration Release
          -p:CollectCoverage=true
          -p:CoverletOutputFormat=opencover
          "-p:CoverletOutput=./TestResults/"
          -p:Threshold=85
          -p:ThresholdType=line
          -p:ThresholdStat=Total

      - name: SonarCloud End
        if: always()                              # garante envio mesmo se testes falharem
        run: dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

### deploy-staging.yml — Push para main

Dispara em push para `main`. O job `deploy-staging` só executa se `build-test-analyze` passar.

```yaml
name: Deploy — Staging

on:
  push:
    branches:
      - main

jobs:
  build-test-analyze:
    # mesmo conteúdo do ci.yml acima

  deploy-staging:
    needs: build-test-analyze
    runs-on: ubuntu-latest
    steps:
      # steps de deploy
```

---

## Branch Protection Rules

Para que o CI bloqueie merges de Pull Requests, é obrigatório configurar Branch Protection Rules no GitHub. **Sem essa configuração, o CI é apenas informativo — o PR pode ser mergeado mesmo que a pipeline falhe.**

### Como configurar

1. Acesse o repositório → **Settings → Branches**
2. Clique em **Add branch protection rule**
3. **Branch name pattern**: `main` (repetir para `develop` se necessário)
4. Marque **Require status checks to pass before merging**
5. Adicione o check: **`Build, Test and Analyze`** (nome exato do job no `ci.yml`)
6. Marque **Require branches to be up to date before merging**
7. Salve

---

## Etapas da Pipeline

| Etapa              | Descrição                                                                  |
|--------------------|---------------------------------------------------------------------------|
| Checkout           | Clona o repositório com histórico completo (`fetch-depth: 0`)             |
| Setup .NET 8       | Instala o SDK do .NET 8                                                   |
| Setup Java 17      | Instala Java 17 (requisito do dotnet-sonarscanner)                        |
| Install Scanner    | Instala o `dotnet-sonarscanner` como global tool                          |
| Restore            | Restaura os pacotes NuGet                                                 |
| SonarCloud Begin   | Inicia a análise; configura Quality Gate wait e caminhos de cobertura     |
| Build              | Compila em Release                                                        |
| Test + Coverage    | Executa testes com coverlet.msbuild; valida threshold de 85%              |
| SonarCloud End     | Envia resultados; aguarda Quality Gate (via `qualitygate.wait` no begin)  |

---

## Convenções

- Pull Requests para `main` e `develop` só podem ser mergeados após o CI passar — **requer Branch Protection Rules configuradas**
- Cobertura mínima: **85% de linhas** — pipelines abaixo desse threshold falham no step de testes
- `sonar.qualitygate.wait=true` deve estar sempre no `begin`, nunca no `end`
- Migrations do EF Core são excluídas da análise via `sonar.exclusions` e `ExcludeByFile` nos projetos de testes
- O SonarCloud End usa `if: always()` para garantir o envio dos dados mesmo quando os testes falham
