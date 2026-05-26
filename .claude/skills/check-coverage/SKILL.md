---
name: check-coverage
description: 'Use this skill when the user asks to check test coverage. Trigger for prompts like "check coverage", "what is the test coverage", "which classes are not tested", "verify the 85% threshold". Do not trigger for test creation — use create-unit-test instead.'
allowed-tools: Read, Glob, Grep, Bash(dotnet test *)
---

## Guardrails

- **Sem alteração de código** — apenas execução de testes e análise de relatório; nunca modificar arquivos
- **Sem acesso a bancos de produção** — testes executados apenas no ambiente de desenvolvimento
- **Sem acesso a `appsettings.Production.json`** — nunca ler arquivos com credenciais
- **Relatório apenas no chat** — nunca criar arquivos de relatório automaticamente
- **Sem acionamento automático de `create-unit-test`** — apenas sugerir; aguardar confirmação do usuário
- **Threshold fixo em 85%** — nunca alterar o threshold mínimo sem instrução explícita do usuário

# Skill: Check Coverage

## MCP

### 1. Ler relatório de cobertura via Filesystem MCP

Após execução dos testes, ler o relatório gerado:

```
read_file → coverage/coverage.xml
Usar os dados do relatório para identificar classes abaixo de 85% e classes sem cobertura.
```

---

## Objetivo

Executa os testes unitários, coleta o relatório de cobertura e identifica classes sem testes ou abaixo do threshold mínimo de 85%. Ao final, sugere a execução do `create-unit-test` para as classes com cobertura insuficiente e aguarda a resposta do usuário.

---

## Contextos Necessários

- [unit-tests.md](../../context/testing/unit-tests.md)
- [test-architecture.md](../../context/testing/test-architecture.md)
- [ci-cd-overview.md](../../context/engineering-process/ci-cd-overview.md)

---

## Entrada

Por padrão, a skill analisa todo o repositório. O usuário pode restringir o escopo:

```
Deseja verificar a cobertura do repositório completo ou de um escopo específico?
1. Completo — todos os projetos de testes
2. Por camada — Presentation, Application, Domain ou Infrastructure
3. Por projeto de testes — informar o projeto específico
```

---

## Passos

### 0. Verificar projetos de testes existentes

Antes de executar qualquer teste, verificar se existe um projeto `.X.Tests` para cada camada presente na solution:

```
Tests/
├── 0 - Presentation/  → [componente].Api.Tests
├── 1 - Application/   → [componente].Application.Tests
├── 2 - Domain/        → [componente].Domain.Tests       (se a camada existir)
└── 3 - Infrastructure/ → [componente].Infrastructure.Tests
```

Se algum projeto de testes estiver ausente:
```
❌ Projeto de testes ausente: [componente].Infrastructure.Tests
   A camada Infrastructure não possui projeto de testes. A cobertura dessa camada será 0%.
   Deseja criar os testes agora via create-unit-test antes de continuar?
   1. Sim — executar create-unit-test para a camada ausente
   2. Não — continuar a análise sem a camada
```

Também verificar se cada projeto de testes tem `<Include>` configurado no `.csproj`. Sem esse filtro, o coverlet mede todos os assemblies transitivos e o resultado é incorreto:

```xml
<!-- obrigatório em cada projeto de testes -->
<Include>[NomeDoProjeto.NomeDaCamada]*</Include>
```

Se `<Include>` estiver ausente em algum projeto, alertar antes de prosseguir:
```
⚠️ [componente].Api.Tests não tem <Include> configurado no .csproj.
   A cobertura medida incluirá assemblies de outras camadas, tornando o resultado impreciso.
   Consulte tests-architecture.md para o padrão correto.
```

### 1. Definir escopo

- Se **completo** → executar todos os projetos de testes da solution
- Se **por camada** → executar apenas o projeto de testes correspondente
- Se **por projeto** → executar apenas o projeto informado

### 2. Executar testes com coleta de cobertura

O projeto usa **`coverlet.msbuild`** com formato **OpenCover**. Não usar `--collect:"XPlat Code Coverage"` nem `dotnet-coverage merge`.

#### Completo
```bash
dotnet test [componente].slnx \
  --configuration Release \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat=opencover \
  "-p:CoverletOutput=./TestResults/" \
  -p:Threshold=85 \
  -p:ThresholdType=line \
  -p:ThresholdStat=Total
```

#### Por projeto
```bash
dotnet test "Tests/[camada]/[componente].[Escopo].Tests/[componente].[Escopo].Tests.csproj" \
  --configuration Release \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat=opencover \
  "-p:CoverletOutput=./TestResults/" \
  -p:Threshold=85 \
  -p:ThresholdType=line \
  -p:ThresholdStat=Total
```

Reportar o resultado da execução antes de prosseguir:

```
✅ Testes executados com sucesso.
Total: N | Passou: N | Falhou: N | Ignorados: N
```

Se houver falhas, alertar o usuário:

```
⚠️ N teste(s) falharam durante a execução.
A análise de cobertura será feita apenas sobre os testes que passaram.
Deseja ver os detalhes das falhas antes de continuar?
1. Sim — exibir detalhes
2. Não — continuar com a análise de cobertura
```

### 3. Ler relatório de cobertura

O relatório é gerado em `TestResults/coverage.opencover.xml` dentro de cada projeto de testes. Ler o arquivo para extrair cobertura por classe:

```
read_file → [projeto].Tests/TestResults/coverage.opencover.xml
```

### 4. Analisar cobertura por classe

Para cada classe de negócio identificada no escopo, extrair:

- **Cobertura de linhas** — percentual de linhas cobertas
- **Cobertura de branches** — percentual de branches cobertos
- **Métodos não cobertos** — lista de métodos sem cobertura

Classificar cada classe em:

| Status | Condição |
|--------|----------|
| ✅ Conforme | Cobertura ≥ 85% |
| ⚠️ Atenção | Cobertura entre 60% e 84% |
| ❌ Crítico | Cobertura < 60% ou sem testes |

### 5. Gerar relatório

```markdown
# Relatório de Cobertura — [escopo analisado]
**Data:** [data atual]

---

## Resumo Executivo

| Métrica | Valor |
|---------|-------|
| Projetos analisados | N |
| Classes analisadas | N |
| Cobertura geral | N% |
| Classes conformes (≥ 85%) | N |
| Classes em atenção (60-84%) | N |
| Classes críticas (< 60%) | N |
| Classes sem testes | N |
| Threshold mínimo | 85% |
| Status geral | ✅ Aprovado / ❌ Reprovado |

---

## Cobertura por Camada

### 0 - Presentation
| Classe | Cobertura | Status |
|--------|-----------|--------|
| `OrderAppService` | 92% | ✅ Conforme |
| `CreateOrderRequestValidator` | 78% | ⚠️ Atenção |

### 1 - Application
| Classe | Cobertura | Status |
|--------|-----------|--------|
| `OrderService` | 45% | ❌ Crítico |
| `PaymentService` | 0% | ❌ Sem testes |

### 2 - Domain
_Todas as classes conformes._

### 3 - Infrastructure
| Classe | Cobertura | Status |
|--------|-----------|--------|
| `OrderRepository` | 70% | ⚠️ Atenção |

---

## Classes que precisam de atenção

### ❌ Críticas — cobertura < 60% ou sem testes
| Classe | Cobertura | Métodos não cobertos |
|--------|-----------|---------------------|
| `OrderService` | 45% | `UpdateAsync`, `DeleteAsync` |
| `PaymentService` | 0% | Todos |

### ⚠️ Em atenção — cobertura entre 60% e 84%
| Classe | Cobertura | Métodos não cobertos |
|--------|-----------|---------------------|
| `CreateOrderRequestValidator` | 78% | Cenários de itens inválidos |
| `OrderRepository` | 70% | `GetPagedAsync` |
```

### 6. Sugerir criação de testes

Após o relatório, apresentar sugestão para as classes com cobertura insuficiente:

```
As seguintes classes estão abaixo do threshold mínimo de 85%:

❌ OrderService (45%) — métodos não cobertos: UpdateAsync, DeleteAsync
❌ PaymentService (0%) — sem testes
⚠️ CreateOrderRequestValidator (78%) — cenários de itens inválidos
⚠️ OrderRepository (70%) — GetPagedAsync não coberto

Deseja criar os testes faltantes agora?
1. Sim, todas as classes — executar create-unit-test para cada uma
2. Selecionar — informar quais classes priorizar
3. Não — apenas registrar o relatório
```

Aguardar resposta do usuário antes de prosseguir.

Se o usuário confirmar, executar `create-unit-test` para cada classe selecionada na ordem de criticidade — classes sem testes primeiro, depois as abaixo de 60%, depois as entre 60% e 84%.

---

## Output Esperado

- Relatório de cobertura exibido no chat
- Sugestão de execução do `create-unit-test` para classes com cobertura insuficiente
- Execução do `create-unit-test` se confirmado pelo usuário

---

## Validação

Antes de entregar o relatório, verificar:

- [ ] Testes executados com sucesso — falhas reportadas ao usuário
- [ ] Cobertura calculada para todas as classes do escopo
- [ ] Classes sem testes identificadas e sinalizadas como críticas
- [ ] Threshold mínimo de 85% aplicado corretamente
- [ ] Status geral (Aprovado/Reprovado) coerente com os resultados
- [ ] Sugestão de `create-unit-test` apresentada para classes com cobertura insuficiente
- [ ] Resposta do usuário aguardada antes de executar qualquer skill adicional

---

## Prompt Examples

- "verifica a cobertura de testes do projeto"
- "quais classes estão abaixo de 85%?"
- "checa a cobertura da camada de Application"
- "quero saber quais classes não têm testes"
- "roda os testes e me mostra a cobertura"

---

## Related Skills

- `create-unit-test` — gerar testes para classes com cobertura insuficiente
- `check-standards` — verificar padrões em conjunto com a cobertura

---

## Error Handling

- **Projeto de testes ausente para uma camada** — alertar como ❌ crítico e sugerir `create-unit-test` antes de continuar
- **`<Include>` ausente no `.csproj`** — alertar que o resultado de cobertura será impreciso; orientar a adicionar conforme `tests-architecture.md`
- **Falha na execução dos testes** — exibir os testes que falharam e perguntar se deseja continuar a análise apenas com os testes que passaram
- **Cobertura total abaixo de 85% com todos os testes passando** — verificar primeiro se `<Include>` está configurado; pode ser falso negativo por scope de assembly incorreto
- **Relatório de cobertura vazio** — verificar se `coverlet.msbuild` está referenciado como `PackageReference` no projeto de testes e se `-p:CollectCoverage=true` foi passado