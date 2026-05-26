# README

## Visão Geral

O `README.md` é o documento de entrada do repositório. Ele apresenta o projeto, orienta o setup local e referencia os padrões de contribuição adotados. Todo `README.md` gerado deve seguir a estrutura e convenções definidas neste arquivo de contexto.

-----

## Localização

```
[nome-do-projeto]/
└── README.md
```

-----

## Estrutura do Arquivo

```markdown
# [Nome do Projeto]

> Breve descrição do que o projeto faz e qual problema resolve.

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

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker](https://www.docker.com/) _(opcional, para dependências locais)_
- Acesso às variáveis de ambiente necessárias

---

## Instalação

```bash
git clone https://github.com/[org]/[nome-do-projeto].git
cd [nome-do-projeto]
dotnet restore src/[componente].sln
```

-----

## Configuração

Copie o arquivo de exemplo de variáveis de ambiente e preencha com os valores do ambiente local:

```bash
cp src/[componente].Api/appsettings.Example.json src/[componente].Api/appsettings.Development.json
```

Principais variáveis de ambiente:

|Variável                      |Descrição                               |
|------------------------------|----------------------------------------|
|`ConnectionStrings__SqlServer`|String de conexão com o banco relacional|
|`ConnectionStrings__MongoDb`  |String de conexão com o MongoDB         |
|`Jwt__Secret`                 |Chave secreta para geração do token JWT |
|`Jwt__ExpiresInSeconds`       |Tempo de expiração do token em segundos |

-----

## Como rodar

```bash
dotnet run --project src/0\ -\ Presentation/[componente].Api
```

A aplicação estará disponível em `https://localhost:5001`. O Swagger UI pode ser acessado em `https://localhost:5001/swagger`.

-----

## Testes

Para executar todos os testes unitários:

```bash
dotnet test src/[componente].sln
```

Para executar com relatório de cobertura:

```bash
dotnet test src/[componente].sln \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage
```

A cobertura mínima exigida é de **85%**.

-----

## Estrutura do projeto

```
[nome-do-projeto]/
├── src/
│   ├── 0 - Presentation/
│   ├── 1 - Application/
│   ├── 2 - Domain/
│   ├── 3 - Infrastructure/
│   ├── Tests/
│   └── [componente].sln
├── README.md
└── CHANGELOG.md
```

Consulte `solution-architecture.md` para detalhes completos sobre a arquitetura e responsabilidades de cada camada.

-----

## Contribuição

Antes de contribuir, leia os padrões adotados no projeto:

- **Branching:** seguimos o GitFlow — consulte `branching-strategy.md`
- **Commits:** seguimos Conventional Commits em português — consulte `commit-standards.md`
- **Pull Requests:** todo PR deve referenciar a issue correspondente e ter ao menos uma aprovação antes do merge

-----

## Changelog

Todas as mudanças relevantes são documentadas no [CHANGELOG.md](./CHANGELOG.md).

```
---

## Convenções

- Sempre em **português**
- A descrição no topo deve ser objetiva — uma ou duas frases explicando o que o projeto faz
- O índice é obrigatório para facilitar a navegação
- Variáveis de ambiente sensíveis nunca são documentadas com valores reais — sempre usar placeholders descritivos
- O arquivo `appsettings.Example.json` deve existir no projeto com todas as chaves necessárias e valores vazios ou de exemplo
- Seções adicionais podem ser incluídas conforme a necessidade do projeto — ex: arquitetura de decisão, dependências externas, contatos
- O README não deve duplicar conteúdo já documentado nos arquivos de contexto — referenciar os arquivos relevantes quando necessário
```