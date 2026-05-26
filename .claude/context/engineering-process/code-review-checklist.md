# Code Review Checklist

## Visão Geral

Este checklist orienta o revisor durante a análise de Pull Requests. O objetivo é garantir consistência arquitetural, qualidade de código e aderência aos padrões definidos nos arquivos de contexto do projeto. Um PR só deve ser aprovado quando todos os itens aplicáveis estiverem satisfeitos.

---

## Critérios de Aprovação

- Ao menos **uma aprovação** de outro membro da equipe
- Pipeline de CI passando com sucesso
- Cobertura de testes mínima de **85%** validada pela pipeline
- Nenhum warning de compilação

---

## Checklist

### Arquitetura e Camadas
- [ ] Os objetos corretos são utilizados em cada camada — Request/Response na Presentation, Models na Application, Entities na Infrastructure — consulte `layer-objects.md`
- [ ] Nenhuma regra de negócio está presente na camada de Presentation ou Infrastructure
- [ ] As dependências entre camadas seguem a direção correta — consulte `solution-architecture.md`
- [ ] Nenhuma camada referencia diretamente a implementação de outra — apenas abstrações via interfaces

### Código
- [ ] Os princípios SOLID são respeitados — consulte `solid.md`
- [ ] Construtores primários são utilizados em classes com injeção de dependência
- [ ] Nenhuma lógica de mapeamento manual entre objetos de camadas diferentes — AutoMapper é obrigatório
- [ ] `CancellationToken` é propagado em todas as operações assíncronas
- [ ] Nenhuma exceção de negócio é lançada — o Result Pattern é utilizado — consulte `result-pattern.md`
- [ ] Nenhum valor hardcoded que deveria estar em configuração (`appsettings`)

### Minimal APIs e Endpoints
- [ ] Endpoints seguem o padrão definido em `minimal-apis.md`
- [ ] `.WithName()`, `.WithSummary()`, `.WithTags()` e `.WithOpenApi()` estão declarados
- [ ] Endpoints autenticados declaram `.RequireAuthorization("policy-name")`
- [ ] Retornos utilizam `TypedResults` — consulte `minimal-apis.md`

### Validação
- [ ] Toda request possui um validator correspondente em `Validators/`
- [ ] O validator cobre todos os campos obrigatórios e regras de formato
- [ ] Nenhuma regra de negócio está presente no validator — apenas validações de entrada

### App Services
- [ ] A AppService apenas mapeia e delega — sem regras de negócio — consulte `app-services.md`
- [ ] Os mapeamentos são feitos via AutoMapper — consulte `automapper-profiles.md`

### Testes
- [ ] Toda classe com regra de negócio possui testes unitários correspondentes
- [ ] Os testes seguem o padrão AAA com comentários `// Arrange`, `// Act`, `// Assert`
- [ ] A nomenclatura segue `MétodoASerTestado_Cenário_ComportamentoEsperado`
- [ ] Testes assíncronos incluem o sufixo `_Async`
- [ ] Asserções são feitas via **Shouldly** — nunca `Assert` nativo do xUnit
- [ ] Data Mocks e Mock Classes são utilizados para construção de cenários e dependências — consulte `data-mocks.md` e `mock-classes.md`
- [ ] O método `Valid()` existe em todo Data Mock novo adicionado

### Integrações
- [ ] Clients de API externa, AWS, Kafka e RabbitMQ seguem os padrões definidos nos respectivos arquivos de contexto
- [ ] Requests de integração são validados com FluentValidation antes do envio
- [ ] Consumers de mensageria herdam de `ResilientConsumerBase` — consulte `messaging-resilience.md`
- [ ] Políticas de circuit breaker são aplicadas nas integrações

### Injeção de Dependência
- [ ] Novos serviços estão registrados na `XDependency.cs` da camada correspondente
- [ ] O lifetime do serviço registrado é adequado ao seu uso — consulte `dependency-injection.md`
- [ ] Nenhum serviço `Scoped` é injetado em serviço `Singleton`

### Documentação e Padrões
- [ ] Novos endpoints possuem XML comments ou `.WithSummary()` e `.WithDescription()` — consulte `api-documentation.md`
- [ ] Logs utilizam a sintaxe de template do `ILogger` — nunca interpolação de string — consulte `logging-standards.md`
- [ ] O nível de log utilizado é adequado ao contexto — consulte `logging-standards.md`
- [ ] O PR possui a label correspondente ao tipo de alteração para o Release Drafter

### Segurança
- [ ] Nenhum dado sensível é logado — senhas, tokens, dados pessoais ou financeiros
- [ ] Senhas e segredos nunca são armazenados em texto plano
- [ ] Nenhuma credencial ou chave está hardcoded no código