---
name: write-readme
description: 'Use this skill when the user asks to create or update the README.md. Trigger for prompts like "generate the README", "update the README", "write the project documentation". Do not trigger for changelog updates or onboarding checklist generation.'
allowed-tools: Read, Edit, Write, Glob
---

## Guardrails

- **Escopo restrito ao `README.md`** — nunca criar ou alterar outros arquivos de documentação
- **Sem exposição de credenciais** — nunca incluir valores reais de variáveis de ambiente; apenas placeholders descritivos
- **Sem leitura de `appsettings.Production.json`** — apenas `appsettings.json` e `appsettings.Example.json`
- **Sem alteração de código-fonte** — apenas leitura para extração de informações
- **Sem desvio do template** — seguir estritamente a estrutura definida em `readme-template.md`

# Skill: Write README

## MCP

### 1. Coletar informações via Filesystem MCP

```
search_files → src/ → "*.sln" (detectar nome da solution)
read_file → src/[componente].Api/appsettings.Example.json
read_file → src/[componente].Api/Properties/launchSettings.json
list_directory → src/ (detectar estrutura de camadas)
```

### 2. Verificar README existente via Filesystem MCP

```
read_file → README.md (se existir)
```

### 3. Escrever README via Filesystem MCP

```
write_file → README.md
```

---

## Objetivo

Gera ou atualiza o `README.md` do repositório seguindo estritamente o template e padrões definidos em `readme.md`. As informações do projeto são detectadas automaticamente a partir do código e configurações do repositório.

---

## Contextos Necessários

- [readme.md](../../context/documentation/readme-template.md)
- [solution-architecture.md](../../context/architecture/solution-architecture.md)
- [project-structure.md](../../context/architecture/project-structure.md)
- [branching-strategy.md](../../context/engineering-process/branching-strategy.md)
- [commit-standards.md](../../context/engineering-process/commit-standards.md)

---

## Entrada

O usuário deve informar a intenção. Se não informado, perguntar:

```
O que deseja fazer?
1. Gerar — criar o README.md do zero
2. Atualizar — atualizar seções específicas do README.md existente
```

Se **atualizar**, perguntar quais seções devem ser atualizadas:

```
Quais seções deseja atualizar?
1. Todas
2. Selecionar — informar quais seções
```

---

## Passos

### 1. Detectar informações do projeto

Analisar automaticamente o repositório para coletar:

| Informação | Fonte |
|------------|-------|
| Nome do projeto | Nome do repositório |
| Descrição | `[componente].sln`, comentários no `Program.cs` ou entrada do usuário |
| Pré-requisitos | `.csproj` — versão do .NET SDK, dependências externas |
| Nome da solution | Arquivo `.sln` em `src/` |
| Nome do componente | Namespace raiz dos projetos |
| Variáveis de ambiente | `appsettings.Example.json` ou `appsettings.json` — apenas chaves, nunca valores reais |
| Porta da aplicação | `launchSettings.json` |
| Estrutura do projeto | Pastas em `src/` |

Se alguma informação não puder ser detectada automaticamente, perguntar ao usuário antes de prosseguir.

### 2. Gerar ou atualizar README

#### Gerar do zero
Seguindo estritamente o template de [readme.md](../../context/documentation/readme-template.md), preencher todas as seções com as informações detectadas:

- **Título e descrição** — nome do projeto e o que ele faz
- **Índice** — links para todas as seções
- **Pré-requisitos** — .NET 8 SDK e dependências detectadas
- **Instalação** — comando `git clone` e `dotnet restore` com o caminho correto da solution
- **Configuração** — variáveis de ambiente detectadas no `appsettings.Example.json`
- **Como rodar** — comando `dotnet run` com o caminho correto do projeto de API
- **Testes** — comandos `dotnet test` com o caminho correto da solution
- **Estrutura do projeto** — árvore gerada a partir das pastas detectadas em `src/`
- **Contribuição** — referências ao branching strategy e commit standards
- **Changelog** — link para o `CHANGELOG.md`

#### Atualizar seções específicas
Para cada seção selecionada, re-detectar as informações correspondentes e substituir apenas o conteúdo daquela seção, preservando o restante do README existente.

### 3. Confirmar variáveis de ambiente

Antes de incluir variáveis de ambiente no README, confirmar com o usuário:

```
As seguintes variáveis foram detectadas no appsettings:
- ConnectionStrings__PostgreSQL
- ConnectionStrings__MongoDB
- Jwt__Secret
- Jwt__ExpiresInSeconds

Deseja adicionar mais alguma variável ou ajustar as descrições?
1. Não — usar as detectadas
2. Sim — informar ajustes
```

---

## Output Esperado

```
[nome-do-projeto]/
└── README.md — gerado ou atualizado
```

Exemplo de estrutura gerada:

```markdown
# [Nome do Projeto]

> [Descrição detectada]

---

## Índice
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Como rodar](#como-rodar)
- [Testes](#testes)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Contribuição](#contribuição)
- [Changelog](#changelog)

---

## Pré-requisitos
...

## Instalação
...

## Configuração
...
```

---

## Validação

Antes de entregar o output, verificar:

- [ ] Todas as seções do template estão presentes — consulte [readme.md](../../context/documentation/readme-template.md)
- [ ] Conteúdo está em **português**
- [ ] Nenhuma credencial ou valor real de variável de ambiente incluído — apenas placeholders descritivos
- [ ] Comandos CLI com caminhos corretos baseados na estrutura detectada
- [ ] Índice referencia todas as seções corretamente
- [ ] Seção de contribuição referencia `branching-strategy.md` e `commit-standards.md`
- [ ] Link para `CHANGELOG.md` presente e correto
- [ ] Estrutura do projeto reflete a estrutura real detectada em `src/`

---

## Prompt Examples

- "gera o README do projeto"
- "cria a documentação inicial do repositório"
- "atualiza a seção de instalação do README"
- "o README está desatualizado, corrige"
- "escreve o README completo"

---

## Error Handling

- **`appsettings.Example.json` ausente** — alertar o usuário e solicitar que informe as variáveis de ambiente manualmente antes de gerar a seção de configuração
- **`launchSettings.json` ausente** — usar porta padrão `5001` e alertar que a porta pode diferir do ambiente real
- **Solution file não encontrada** — alertar e solicitar que o usuário informe o nome do componente manualmente
- **README já existente** — nunca sobrescrever sem perguntar; sempre oferecer a opção de atualizar seções específicas