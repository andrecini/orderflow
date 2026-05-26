---
name: create-unit-test
description: 'Use this skill when the user asks to create unit tests for a specific class or set of classes. Trigger for prompts like "create tests for X", "write unit tests for Y", "add test coverage to Z". Do not trigger for integration test creation — use create-integration-test instead.'
allowed-tools: Read, Edit, Write, Glob, Grep, Bash(dotnet test *)
---

## Guardrails

- **Escopo restrito ao projeto de testes correspondente** — nunca criar arquivos de teste fora de `[componente].X.Tests/`
- **Sem alteração de código de produção** — apenas leitura de arquivos de produção para identificar cenários
- **Sem criação de testes de integração** — responsabilidade exclusiva da skill `create-integration-test`
- **Sem alteração de Data Mocks ou Mock Classes de outros recursos** — apenas adicionar ou atualizar os relacionados à classe testada
- **Sem uso de `Assert` nativo do xUnit** — exclusivamente Shouldly para asserções
- **Sem criação de testes sem o conjunto completo** — sempre gerar Data Mock + Mock Class + Teste juntos
- **Perguntar antes de sobrescrever** — se classe de teste já existir, nunca sobrescrever sem confirmação do usuário

# Skill: Create Unit Test

## MCP

### 1. Verificar arquivos existentes via Filesystem MCP
```
list_directory → src/Tests/[camada]/[componente].X.Tests/DataMocks/
list_directory → src/Tests/[camada]/[componente].X.Tests/Mocks/
list_directory → src/Tests/[camada]/[componente].X.Tests/Tests/
Para cada arquivo já existente, informar o usuário antes de atualizar.
```
### 2. Escrever arquivos via Filesystem MCP
```
write_file → src/Tests/[camada]/[componente].X.Tests/DataMocks/...
write_file → src/Tests/[camada]/[componente].X.Tests/Mocks/...
write_file → src/Tests/[camada]/[componente].X.Tests/Tests/...
```

---

## Objetivo

Guia a criação do conjunto completo de testes unitários para uma classe — Data Mocks, Mock Classes e classes de teste. Se o usuário informar a classe, os cenários são identificados automaticamente. Se fornecer o código, os cenários são extraídos diretamente. Data Mocks e Mock Classes existentes são atualizados, nunca duplicados.

---

## Contextos Necessários

- [unit-tests.md](../../context/testing/unit-tests.md)
- [mock-classes.md](../../context/testing/mock-classes.md)
- [data-mocks.md](../../context/testing/data-mocks.md)
- [test-architecture.md](../../context/testing/test-architecture.md)
- [result-pattern.md](../../context/patterns/result-pattern.md)
- [builder.md](../../context/patterns/builder.md)
- [layer-objects.md](../../context/architecture/layer-objects.md)

---

## Entrada

O usuário deve fornecer uma das seguintes opções:

- **Nome da classe** — o agente identifica automaticamente os métodos, dependências e cenários testáveis
- **Código da classe** — o agente extrai os métodos, dependências e cenários diretamente do código fornecido

Se nenhuma das opções for fornecida, perguntar:

```
Qual classe deseja testar?
Informe o nome da classe ou cole o código diretamente.
```

---

## Passos

### 1. Identificar a classe e seu contexto

A partir do nome ou código fornecido, identificar:

- **Camada** — Presentation, Application, Domain ou Infrastructure
- **Dependências injetadas** — interfaces que precisarão de Mock Classes
- **Métodos públicos** — operações que serão testadas
- **Objetos de entrada e saída** — tipos que precisarão de Data Mocks
- **Projeto de testes correspondente** — conforme [test-architecture.md](../../context/testing/test-architecture.md)

### 2. Mapear cenários testáveis

Para cada método público, mapear:

- Cenário de **sucesso** — entrada válida, comportamento esperado
- Cenários de **falha** — entradas inválidas, dependências que retornam falha, estados inesperados
- Cenários de **borda** — valores nulos, listas vazias, limites

> A cobertura mínima exigida é de **85% dos cenários testáveis** — consulte [unit-tests.md](../../context/testing/unit-tests.md)

### 3. Verificar Data Mocks existentes

Para cada objeto de entrada e saída identificado:

- Se o Data Mock **não existe** → criar em `[componente].X.Tests/DataMocks/[Tipo]/`
- Se o Data Mock **já existe** → adicionar apenas os novos métodos de cenário necessários
- Usar **Builder** quando o objeto tiver mais de 4 variações de cenário — consulte [builder.md](../../context/patterns/builder.md)
- Garantir que o método `Valid()` existe em todo Data Mock

```
[componente].X.Tests/DataMocks/
├── Requests/[NomeDoRequest]Mock.cs
├── Responses/[NomeDoResponse]Mock.cs
└── Models/[NomeDoModel]Mock.cs
```

### 4. Verificar Mock Classes existentes

Para cada dependência injetada identificada:

- Se a Mock Class **não existe** → criar em `[componente].X.Tests/Mocks/[Contexto]/`
- Se a Mock Class **já existe** → adicionar apenas os novos métodos de setup necessários
- Seguir o padrão `BaseMock<T>` com métodos encadeáveis — consulte [mock-classes.md](../../context/testing/mock-classes.md)
- Adicionar método `Verify*` para dependências críticas

```
[componente].X.Tests/Mocks/
├── Services/[NomeDaInterface]Mock.cs
├── Repositories/[NomeDaInterface]Mock.cs
├── AppServices/[NomeDaInterface]Mock.cs
└── Integrations/[NomeDaInterface]Mock.cs
```

### 5. Gerar classe de testes

Seguindo [unit-tests.md](../../context/testing/unit-tests.md):

- Criar `[NomeDaClasse]Tests` em `[componente].X.Tests/Tests/[Contexto]/`
- Um método de teste por cenário — nunca múltiplos cenários em um único teste
- Seguir o padrão AAA com comentários `// Arrange`, `// Act` e `// Assert`
- Nomenclatura: `MétodoASerTestado_Cenário_ComportamentoEsperado`
- Sufixo `_Async` em testes assíncronos
- Asserções exclusivamente via **Shouldly**
- `CancellationToken.None` em todas as operações assíncronas

```
[componente].X.Tests/Tests/
├── AppServices/[NomeDaClasse]Tests.cs
├── Services/[NomeDaClasse]Tests.cs
├── Validators/[NomeDaClasse]Tests.cs
└── Repositories/[NomeDaClasse]Tests.cs
```

### 6. Validar cobertura dos cenários

Após gerar os testes, listar os cenários cobertos e os não cobertos:

```
✅ Cobertos:
- CreateAsync_ValidRequest_ReturnsSuccessAsync
- CreateAsync_CustomerNotFound_ReturnsNotFoundAsync
- CreateAsync_RepositoryFails_ReturnsInternalErrorAsync

⚠️ Não cobertos (abaixo de 85%):
- [cenário identificado mas não gerado — justificar]
```

---

## Output Esperado

```
[componente].X.Tests/
├── DataMocks/
│   ├── Requests/[NomeDoRequest]Mock.cs        — criado ou atualizado
│   ├── Responses/[NomeDoResponse]Mock.cs      — criado ou atualizado
│   └── Models/[NomeDoModel]Mock.cs            — criado ou atualizado
├── Mocks/
│   └── [Contexto]/[NomeDaDependencia]Mock.cs  — criado ou atualizado
└── Tests/
    └── [Contexto]/[NomeDaClasse]Tests.cs      — sempre criado
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] Todos os métodos públicos da classe possuem ao menos um teste de sucesso e um de falha
- [ ] Cobertura mínima de **85% dos cenários testáveis** atingida
- [ ] Padrão AAA com comentários `// Arrange`, `// Act`, `// Assert` em todos os testes
- [ ] Nomenclatura `MétodoASerTestado_Cenário_ComportamentoEsperado` em todos os testes
- [ ] Sufixo `_Async` em testes assíncronos
- [ ] Asserções via **Shouldly** — nunca `Assert` nativo do xUnit
- [ ] `CancellationToken.None` em todas as operações assíncronas
- [ ] Método `Valid()` presente em todos os Data Mocks
- [ ] Mock Classes seguem o padrão `BaseMock<T>` com métodos encadeáveis
- [ ] Data Mocks e Mock Classes existentes foram atualizados, nunca duplicados
- [ ] Cada cenário testado em método isolado — nunca múltiplos cenários em um único teste

---

## Prompt Examples

- "cria testes unitários para a OrderService"
- "adiciona cobertura de testes para o CreateOrderRequestValidator"
- "gera os testes do OrderAppService"
- "quero testes para essa classe"
- "cobre os cenários de falha do PaymentService"

---

## Related Skills

- `create-integration-test` — gerar testes de integração complementares
- `check-coverage` — verificar se a cobertura atingiu o threshold de 85% após geração

---

## Error Handling

- **Classe não encontrada** — se a classe informada não existir no projeto, alertar e perguntar se deseja informar o código diretamente
- **Classe sem métodos públicos testáveis** — alertar o usuário e listar apenas os métodos identificados antes de prosseguir
- **Data Mock ou Mock Class com conflito** — se um método de cenário ou setup já existir com nome igual mas implementação diferente, alertar antes de atualizar
- **Cobertura insuficiente após geração** — se os cenários gerados não atingirem 85%, listar os cenários faltantes e perguntar se deseja complementar