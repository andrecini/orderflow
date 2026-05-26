# 📅 Gerando planejamento para **Semana 1 — Polly v8 + Resiliência HTTP**

> Data de geração: 2026-05-23 | Roadmap iniciado em: 2026-05-23 | semana_atual = 1

---

## BLOCO 1 — PROJETO PRÁTICO DA SEMANA

### 🎯 OBJETIVO DA SEMANA

Você vai construir um `HttpClient` verdadeiramente resiliente para consumir uma API externa usando Polly v8 e `HttpClientFactory` no .NET 8. Ao final da semana, você será capaz de configurar um `ResiliencePipeline` com Retry exponencial, Circuit Breaker, Timeout e Fallback, entender quando cada estratégia dispara, e simular falhas reais via Toxiproxy no Docker para validar o comportamento do sistema sob estresse — sem que a aplicação exponha exceções ao caller.

---

### 🗂 CONTEXTO NO PROJETO MAIOR

**Semana anterior:** nenhuma — esta é a semana inaugural do OrderFlow.

**Esta semana:** você está criando a camada de integração externa do OrderFlow. O projeto integrador precisará consultar fornecedores, gateways de pagamento e serviços de CEP. Sem resiliência nessa camada, qualquer instabilidade de terceiros derruba o sistema. O `HttpClient` resiliente que você constrói agora será reaproveitado em todas as semanas seguintes que envolvam chamadas externas.

**Semana seguinte (Semana 2):** o foco muda para o banco de dados — análise de execution plans e otimização de queries. A infraestrutura Docker que você sobe agora (com Toxiproxy) já servirá de base para adicionar PostgreSQL na semana seguinte.

---

### 📋 ESPECIFICAÇÃO TÉCNICA

**Stack utilizada:**
- .NET 8 (ASP.NET Core Web API + Worker Services)
- Polly v8 (`Microsoft.Extensions.Http.Resilience` — integração nativa com HttpClientFactory)
- `Polly.Core` v8 para pipelines manuais quando necessário
- Docker + Docker Compose
- Toxiproxy (`ghcr.io/shopify/toxiproxy`) para simulação de falhas de rede
- xUnit + `Microsoft.AspNetCore.Mvc.Testing` para testes
- Serilog para logging estruturado das tentativas/circuit state

**Estrutura de arquivos esperada ao final:**
```
OrderFlow/
├── docker-compose.yml
├── docker-compose.toxiproxy.yml
├── src/
│   └── OrderFlow.Integrations/
│       ├── OrderFlow.Integrations.csproj
│       ├── Clients/
│       │   ├── IPokemonClient.cs
│       │   └── PokemonClient.cs
│       ├── Models/
│       │   └── PokemonDto.cs
│       ├── Resilience/
│       │   ├── ResiliencePipelineExtensions.cs
│       │   └── FallbackResponses.cs
│       └── DependencyInjection/
│           └── IntegrationsServiceCollectionExtensions.cs
├── tests/
│   └── OrderFlow.Integrations.Tests/
│       ├── OrderFlow.Integrations.Tests.csproj
│       ├── Clients/
│       │   └── PokemonClientResilienceTests.cs
│       └── Fakes/
│           └── FakeHttpMessageHandler.cs
└── OrderFlow.sln
```

**Contratos principais (interfaces/classes):**

```csharp
// IPokemonClient.cs
public interface IPokemonClient
{
    Task<PokemonDto?> GetByNameAsync(string name, CancellationToken ct = default);
}

// PokemonDto.cs
public record PokemonDto(int Id, string Name, int BaseExperience);

// FallbackResponses.cs
public static class FallbackResponses
{
    public static PokemonDto? Pokemon => null; // retorna null como fallback gracioso
}

// ResiliencePipelineExtensions.cs
public static class ResiliencePipelineExtensions
{
    // Configura o pipeline padrão para chamadas HTTP externas no OrderFlow
    public static IHttpStandardResilienceOptionsBuilder AddOrderFlowResilienceHandler(
        this IHttpClientBuilder builder);
}

// IntegrationsServiceCollectionExtensions.cs
public static class IntegrationsServiceCollectionExtensions
{
    public static IServiceCollection AddOrderFlowIntegrations(
        this IServiceCollection services, IConfiguration configuration);
}
```

**Critérios de aceite:**
- [ ] `docker-compose up` sobe a API, o Toxiproxy e as dependências sem erro
- [ ] Chamada normal para a PokeAPI retorna dados corretamente
- [ ] Com latência de 4s injetada no Toxiproxy, o Timeout dispara antes de chegar ao caller
- [ ] Com proxy derrubado, o Retry com backoff exponencial é executado (log mostra tentativas)
- [ ] Após 5 falhas consecutivas, o Circuit Breaker abre (log mostra estado `Open`)
- [ ] Quando o CB está aberto, o Fallback retorna `null` imediatamente sem tentar a rede
- [ ] Testes unitários passam usando `FakeHttpMessageHandler` sem dependência de rede real
- [ ] Código commitado com mensagem semântica e README com instruções de execução

---

### 📅 PLANO DIÁRIO (1h/dia)

**Dia 1 (segunda) — Scaffolding e Docker Compose:**

1. Crie a solution `OrderFlow.sln` e os projetos `OrderFlow.Integrations` e `OrderFlow.Integrations.Tests`
2. Escreva o `docker-compose.yml` com dois serviços: `api` (sua aplicação) e `toxiproxy` (imagem `ghcr.io/shopify/toxiproxy`)
3. Configure o Toxiproxy via arquivo `toxiproxy.json`:
   ```json
   [{ "name": "pokeapi", "listen": "0.0.0.0:8474", "upstream": "pokeapi.co:443" }]
   ```
4. Suba com `docker-compose up` e valide que o Toxiproxy responde em `localhost:8474`
5. Instale os pacotes: `Microsoft.Extensions.Http.Resilience`, `Serilog.AspNetCore`, `Polly.Core`
6. Crie `PokemonDto` e `IPokemonClient` — apenas as interfaces, sem implementação ainda

**Dia 2 (terça) — Implementação do ResiliencePipeline:**

1. Implemente `PokemonClient` com `HttpClient` injetado via construtor
2. Em `IntegrationsServiceCollectionExtensions`, registre o `HttpClient` com `AddHttpClient<IPokemonClient, PokemonClient>`
3. Encadeie as estratégias do Polly v8 usando `AddResilienceHandler`:
   ```csharp
   .AddResilienceHandler("orderflow-external", builder =>
   {
       builder
           .AddFallback(new FallbackStrategyOptions<HttpResponseMessage> { ... })
           .AddTimeout(TimeSpan.FromSeconds(3))
           .AddRetry(new HttpRetryStrategyOptions
           {
               MaxRetryAttempts = 3,
               Delay = TimeSpan.FromMilliseconds(500),
               BackoffType = DelayBackoffType.Exponential,
               UseJitter = true
           })
           .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
           {
               SamplingDuration = TimeSpan.FromSeconds(30),
               MinimumThroughput = 5,
               FailureRatio = 0.5,
               BreakDuration = TimeSpan.FromSeconds(15)
           });
   });
   ```
4. Adicione logs estruturados via Serilog em cada evento do pipeline (OnRetry, OnCircuitOpened, OnFallback)
5. Suba a API e faça uma chamada manual via Swagger/curl — valide o caminho feliz

**Dia 3 (quarta) — Simulação de falhas com Toxiproxy:**

1. Instale o CLI do Toxiproxy ou use a API REST (`curl http://localhost:8475/proxies`)
2. Injete latência alta para disparar o Timeout:
   ```bash
   curl -X POST http://localhost:8475/proxies/pokeapi/toxics \
     -d '{"type":"latency","attributes":{"latency":4000}}'
   ```
3. Observe nos logs que o Timeout dispara em ~3s e o Retry tenta novamente
4. Remova o tóxico e injete `return_error` para simular instabilidade:
   ```bash
   curl -X POST http://localhost:8475/proxies/pokeapi/toxics \
     -d '{"type":"limit_data","attributes":{"bytes":0}}'
   ```
5. Dispare 10 chamadas seguidas e observe o Circuit Breaker abrindo no log (`CircuitState: Open`)
6. Aguarde 15s, dispare outra chamada — o CB entra em `HalfOpen` e tenta recuperar
7. Documente os resultados no README com os logs capturados

**Dia 4 (quinta) — Testes unitários de resiliência:**

1. Implemente `FakeHttpMessageHandler`:
   ```csharp
   public class FakeHttpMessageHandler : HttpMessageHandler
   {
       private readonly Queue<HttpResponseMessage> _responses = new();
       public void EnqueueResponse(HttpResponseMessage response) => _responses.Enqueue(response);
       protected override Task<HttpResponseMessage> SendAsync(
           HttpRequestMessage request, CancellationToken ct)
           => Task.FromResult(_responses.Dequeue());
   }
   ```
2. Escreva os testes em `PokemonClientResilienceTests`:
   - `GetByName_WhenApiReturns200_ReturnsPokemonDto`
   - `GetByName_WhenApiReturns500ThreeTimes_RetriesAndFallsBack`
   - `GetByName_WhenTimeoutExceeded_ReturnsFallback`
   - `GetByName_WhenCircuitIsOpen_ReturnsFallbackWithoutCallingApi`
3. Use `ResiliencePipelineBuilder` diretamente nos testes para criar pipelines com configurações de teste (durações menores)
4. Execute `dotnet test` e confirme 100% de sucesso

**Dia 5 (sexta) — Revisão, documentação e commit:**

1. Revise o código: verifique `ConfigureAwait(false)` em todos os métodos assíncronos do `PokemonClient`
2. Confirme que o `CancellationToken` é propagado de `GetByNameAsync` até o `HttpClient.SendAsync`
3. Escreva o README com:
   - Como subir o ambiente (`docker-compose up`)
   - Como executar os testes (`dotnet test`)
   - Diagrama ASCII do fluxo do ResiliencePipeline
4. Commit com mensagem semântica:
   ```
   feat(integrations): add resilient HttpClient with Polly v8 pipeline
   
   - ResiliencePipeline: Fallback → Timeout → Retry (exp backoff) → CircuitBreaker
   - Toxiproxy Docker setup for failure simulation
   - Unit tests covering all resilience scenarios
   ```
5. Atualize o board do GitHub: mova todos os cards de In Progress para Done

---

### 🧪 TESTES ESPERADOS

```csharp
// Cenário 1: caminho feliz
[Fact]
public async Task GetByName_WhenApiReturns200_ReturnsPokemonDto()
// Valida: cliente desserializa corretamente e retorna o DTO populado

// Cenário 2: retry com backoff
[Fact]
public async Task GetByName_WhenApiReturns500Twice_RetriesAndSucceedsOnThirdAttempt()
// Valida: handler foi chamado 3 vezes, retorno final é sucesso

// Cenário 3: fallback após esgotar retries
[Fact]
public async Task GetByName_WhenApiAlwaysReturns503_ReturnsFallbackAfterRetries()
// Valida: retorno é null (fallback), nenhuma exceção vazou para o caller

// Cenário 4: timeout
[Fact]
public async Task GetByName_WhenResponseDelayExceedsTimeout_ReturnsFallback()
// Valida: operação encerra antes do delay completo, retorna fallback

// Cenário 5: circuit breaker aberto
[Fact]
public async Task GetByName_WhenCircuitIsOpen_ReturnsFallbackWithoutCallingUpstream()
// Valida: handler HTTP não é chamado quando CB está em estado Open
// Implementação: abra o CB manualmente via pipeline de teste, então dispare chamada

// Cenário 6: CancellationToken propagado
[Fact]
public async Task GetByName_WhenCancellationRequested_ThrowsOperationCanceledException()
// Valida: token cancelado antes da chamada → OperationCanceledException sobe corretamente
```

---

### 🔗 RECURSOS DE ESTUDO

1. **[Documentação oficial — Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)**
   Documentação da integração nativa do Polly v8 com `HttpClientFactory` no .NET 8. Cobre exatamente as APIs que você vai usar — `AddResilienceHandler`, `AddStandardResilienceHandler` e as opções de cada estratégia.

2. **[Toxiproxy GitHub + Exemplos de Uso](https://github.com/Shopify/toxiproxy)**
   Referência do Toxiproxy com todos os tipos de "tóxicos" disponíveis (latency, bandwidth, return_error, slow_close). Indispensável para simular os cenários de Dia 3 da semana.

3. **[Polly v8 Migration Guide — App vNext](https://www.pollydocs.org/migration-v8.html)**
   Guia oficial de migração do Polly v7 para v8. Fundamental porque a maioria dos exemplos que você vai encontrar no Google ainda usa a API antiga — este guia mostra o que mudou e como as novas `ResiliencePipeline` e `ResilienceStrategyOptions` funcionam.

---

### 💡 ARMADILHAS COMUNS

1. **Ordem errada das estratégias no pipeline**
   O Polly v8 executa as estratégias de fora para dentro. A ordem correta é: `Fallback → Timeout → Retry → CircuitBreaker`. Se você colocar o Timeout depois do Retry, o timeout se aplica a *cada tentativa individual* — que é o comportamento correto. Mas se colocar o Fallback depois do Retry, ele não captura as exceções dos retries. Lembre-se: a primeira estratégia adicionada é a mais externa.

2. **Usar `AddStandardResilienceHandler` sem entender os defaults**
   O método helper `AddStandardResilienceHandler()` parece conveniente, mas seus defaults (ex: timeout de 30s por tentativa, até 3 retries) podem ser inadequados para o seu caso. Comece com `AddResilienceHandler` configurando tudo explicitamente para realmente aprender o que cada parâmetro faz — use o helper só quando já entender o que está delegando.

3. **Não testar o estado `HalfOpen` do Circuit Breaker**
   Devs testam o CB abrindo (`Open`) mas esquecem de validar que ele se recupera (`HalfOpen → Closed`). Em produção, um CB que nunca fecha é tão ruim quanto não ter um. No teste, use `BreakDuration` de 100ms e dispare uma chamada bem-sucedida após aguardar — confirme nos logs que o estado voltou para `Closed`.

---

## BLOCO 2 — CARDS DO GITHUB PROJECTS

---

#### CARD 1

**Título:** `[S01] Setup inicial: solution, projetos e Docker Compose com Toxiproxy`

**Labels:** `semana-01` `polly-resiliencia` `tipo: chore`

**Milestone:** Semana 01 — Polly v8 + Resiliência HTTP

**Descrição:**
```markdown
## Contexto
Ponto de partida do OrderFlow. Antes de escrever qualquer lógica de resiliência,
precisamos da infraestrutura base: solution .NET 8, projeto de integrações, testes
e o ambiente Docker com Toxiproxy para simular falhas de rede na semana toda.

## O que fazer
- [ ] Criar `OrderFlow.sln` com `dotnet new sln`
- [ ] Criar projeto `OrderFlow.Integrations` (classlib ou webapi mínimo)
- [ ] Criar projeto `OrderFlow.Integrations.Tests` (xUnit)
- [ ] Adicionar ambos à solution
- [ ] Escrever `docker-compose.yml` com serviços `api` e `toxiproxy`
- [ ] Criar `toxiproxy.json` com proxy apontando para `pokeapi.co:443`
- [ ] Validar que `docker-compose up` sobe sem erros
- [ ] Instalar pacotes: `Microsoft.Extensions.Http.Resilience`, `Serilog.AspNetCore`, `Polly.Core`

## Critério de aceite
- [ ] `docker-compose up` sobe sem erros e ambos os serviços ficam healthy
- [ ] Toxiproxy responde em `http://localhost:8475/proxies`
- [ ] `dotnet build` compila sem warnings

## Notas técnicas
- Toxiproxy image: `ghcr.io/shopify/toxiproxy:2.9.0`
- A porta 8474 é onde o proxy fica escutando; 8475 é a API de controle do Toxiproxy
- Use `healthcheck` no docker-compose para o serviço api aguardar o toxiproxy estar pronto
```

**Coluna inicial:** Backlog

---

#### CARD 2

**Título:** `[S01] Implementar IPokemonClient com ResiliencePipeline (Polly v8)`

**Labels:** `semana-01` `polly-resiliencia` `tipo: feat`

**Milestone:** Semana 01 — Polly v8 + Resiliência HTTP

**Descrição:**
```markdown
## Contexto
Núcleo da semana. Implementar o cliente HTTP com o pipeline completo de resiliência
usando a integração nativa do Polly v8 com HttpClientFactory no .NET 8.
Este cliente é o template que será replicado em todas as integrações externas do OrderFlow.

## O que fazer
- [ ] Criar interface `IPokemonClient` e record `PokemonDto`
- [ ] Implementar `PokemonClient` com `HttpClient` injetado
- [ ] Criar `IntegrationsServiceCollectionExtensions` com registro do `HttpClient`
- [ ] Configurar `AddResilienceHandler` com as 4 estratégias na ordem correta:
      Fallback → Timeout (3s) → Retry (3x, exp backoff, jitter) → CircuitBreaker
- [ ] Adicionar logs Serilog nos eventos: OnRetry, OnCircuitOpened, OnCircuitClosed, OnFallback
- [ ] Configurar a base URL do Toxiproxy via `IConfiguration` (não hardcoded)
- [ ] Testar caminho feliz via Swagger/curl

## Critério de aceite
- [ ] Chamada bem-sucedida retorna `PokemonDto` populado
- [ ] Logs estruturados aparecem a cada evento do pipeline
- [ ] BaseUrl configurável via appsettings.json / environment variable
- [ ] Nenhuma exceção não tratada vaza para o controller

## Notas técnicas
- Usar `Microsoft.Extensions.Http.Resilience` (não o `Polly` diretamente para HttpClient)
- Referência: https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience
- `UseJitter = true` no Retry evita thundering herd quando múltiplos clientes falham juntos
- Fallback deve retornar `null` (ou um `PokemonDto` padrão) — nunca propagar a exceção
```

**Coluna inicial:** Backlog

---

#### CARD 3

**Título:** `[S01] Simular falhas com Toxiproxy e validar comportamento do pipeline`

**Labels:** `semana-01` `polly-resiliencia` `tipo: chore`

**Milestone:** Semana 01 — Polly v8 + Resiliência HTTP

**Descrição:**
```markdown
## Contexto
A teoria é boa, mas o pipeline só prova seu valor quando testado com falhas reais.
Usar o Toxiproxy para injetar latência, erros de conexão e instabilidade intermitente
e observar cada estratégia do Polly disparando nos logs.

## O que fazer
- [ ] Instalar Toxiproxy CLI localmente (`brew install toxiproxy` ou binary no Linux)
- [ ] Cenário 1 — Timeout: injetar latência de 4000ms e confirmar que Timeout dispara em 3s
- [ ] Cenário 2 — Retry: injetar `limit_data` (bytes: 0) e observar 3 tentativas nos logs
- [ ] Cenário 3 — Circuit Breaker: disparar 10 chamadas seguidas com erro e observar estado `Open`
- [ ] Cenário 4 — Recuperação: aguardar 15s e disparar chamada para observar estado `HalfOpen`→`Closed`
- [ ] Documentar cada cenário no README com o comando curl usado e o log resultante

## Critério de aceite
- [ ] README contém seção "Simulação de Falhas" com todos os 4 cenários documentados
- [ ] Log de cada cenário está colado no README (pode ser trecho resumido)
- [ ] Nenhum cenário retornou exceção não tratada para o caller

## Notas técnicas
- API de controle do Toxiproxy: `POST /proxies/{proxy}/toxics`
- Para remover um tóxico: `DELETE /proxies/{proxy}/toxics/{toxic_name}`
- `toxic_name` é o campo "name" que você define na criação — use nomes descritivos
```

**Coluna inicial:** Backlog

---

#### CARD 4

**Título:** `[S01] Escrever testes unitários de resiliência com FakeHttpMessageHandler`

**Labels:** `semana-01` `polly-resiliencia` `tipo: test`

**Milestone:** Semana 01 — Polly v8 + Resiliência HTTP

**Descrição:**
```markdown
## Contexto
Os testes manuais com Toxiproxy validam o ambiente, mas não entram no CI.
Precisamos de testes rápidos, determinísticos e sem dependência de rede que cobram
todos os cenários do pipeline de resiliência.

## O que fazer
- [ ] Implementar `FakeHttpMessageHandler` com fila de respostas (`Queue<HttpResponseMessage>`)
- [ ] Escrever 6 testes cobrindo: caminho feliz, retry com sucesso no 3º, fallback após esgotar retries,
      timeout, circuit breaker aberto, cancelamento de token
- [ ] Criar helper `BuildTestPipeline()` que monta o ResiliencePipeline com durações reduzidas para teste
      (ex: BreakDuration = 100ms, Timeout = 200ms)
- [ ] Garantir que os testes não têm `Thread.Sleep` — usar `ResiliencePipeline` com clock falso ou
      durações mínimas
- [ ] Rodar `dotnet test --verbosity normal` e confirmar todos passando

## Critério de aceite
- [ ] 6 testes passando, 0 falhando
- [ ] Tempo total de execução dos testes < 5 segundos
- [ ] Nenhum teste usa rede real ou depende de ordem de execução

## Notas técnicas
- Para testar o estado Open do CB: configure `MinimumThroughput = 2, FailureRatio = 1.0`
  e dispare 2 falhas antes do teste real
- `FakeHttpMessageHandler` deve ser thread-safe se usar paralelismo nos testes
- Referência para clock virtual no Polly v8: `TimeProvider.System` pode ser substituído
  por `FakeTimeProvider` do pacote `Microsoft.Extensions.TimeProvider.Testing`
```

**Coluna inicial:** Backlog

---

#### CARD 5

**Título:** `[S01] Documentação final: README, diagrama do pipeline e commit semântico`

**Labels:** `semana-01` `polly-resiliencia` `tipo: docs`

**Milestone:** Semana 01 — Polly v8 + Resiliência HTTP

**Descrição:**
```markdown
## Contexto
Um projeto bem documentado vale tanto quanto o código. O README é o primeiro contato
de qualquer recrutador técnico ou colaborador com o trabalho. Esta task fecha a semana
com o repositório em estado publicável.

## O que fazer
- [ ] Escrever README com seções: visão geral, pré-requisitos, como executar, como testar
- [ ] Adicionar diagrama ASCII do ResiliencePipeline mostrando a ordem das estratégias
- [ ] Adicionar seção "Simulação de Falhas" com os 4 cenários do Toxiproxy
- [ ] Verificar `ConfigureAwait(false)` em todos os métodos async do projeto principal
- [ ] Verificar que `CancellationToken` é propagado até o `HttpClient.SendAsync`
- [ ] Fazer commit final com mensagem semântica adequada
- [ ] Mover todos os cards para coluna Done no GitHub Projects

## Critério de aceite
- [ ] README renderiza corretamente no GitHub sem broken links
- [ ] Diagrama ASCII está correto e legível
- [ ] `git log --oneline` mostra mensagens semânticas claras durante toda a semana
- [ ] Board do GitHub Projects com todos os cards em Done

## Notas técnicas
- Template de commit: `feat(integrations): add resilient HttpClient with Polly v8 pipeline`
- Diagrama sugerido:
  ```
  Request → [Fallback] → [Timeout 3s] → [Retry 3x exp] → [CircuitBreaker] → HttpClient → API
  ```
- Publicar o repositório como público no GitHub — é portfólio, deve ser visível
```

**Coluna inicial:** Backlog

---

## BLOCO 3 — POST LINKEDIN: TERÇA

---

POST TERÇA:

Seu HttpClient cai junto com a API que você consome?

Passei anos achando que `try/catch` em volta do `await client.GetAsync(...)` era suficiente.

Funcionava — até o dia em que a API de CEP de um cliente ficou instável por 40 minutos. Cada request novo travava por 30 segundos antes de jogar exceção. A thread pool foi embora. O sistema inteiro foi junto.

O problema não era falta de tratamento de erro. Era falta de **resiliência estruturada**.

No .NET 8, o Polly v8 integrado ao `HttpClientFactory` resolve isso com um pipeline declarativo:

```csharp
builder.AddResilienceHandler("external", pipeline =>
{
    pipeline
        .AddFallback(...)      // resposta padrão se tudo falhar
        .AddTimeout(TimeSpan.FromSeconds(3))
        .AddRetry(new() { MaxRetryAttempts = 3, BackoffType = Exponential })
        .AddCircuitBreaker(...); // para de tentar após X falhas seguidas
});
```

A ordem importa: Fallback envolve tudo. Timeout limita cada tentativa. Retry repete. CircuitBreaker protege o downstream quando ele já está no chão.

Não é over-engineering. É o mínimo aceitável para qualquer integração que vai para produção.

#dotnet #csharp #backend #polly #resiliencia

---
Caracteres: 788

---

## BLOCO 4 — POST LINKEDIN: DOMINGO

---

POST DOMINGO:

Implementei Circuit Breaker do zero no .NET 8 com Polly v8. Três coisas que eu não esperava encontrar:

**1. A ordem das estratégias no pipeline não é intuitiva**

O Polly executa de fora para dentro. Então `Fallback → Timeout → Retry → CircuitBreaker` significa: o Fallback captura erros de tudo abaixo, o Timeout se aplica a *cada tentativa individual* do Retry, e o CircuitBreaker conta falhas depois que o Retry já desistiu.

Inverta qualquer par e o comportamento muda completamente.

**2. O Circuit Breaker tem três estados — e o `HalfOpen` é o mais importante**

`Closed` (funcionando) → `Open` (bloqueando, retorna erro imediato) → `HalfOpen` (deixa passar 1 req para testar recuperação) → de volta ao início.

Todo dev testa o CB abrindo. Quase ninguém testa ele *fechando*. Se o HalfOpen nunca funcionar direito, seu serviço fica bloqueado para sempre após qualquer falha prolongada.

**3. Sem jitter no Retry, você cria thundering herd**

Com `UseJitter = true`, cada instância da sua API espera um tempo ligeiramente diferente antes de retenter. Sem isso, 50 pods retentam exatamente ao mesmo tempo e transformam uma API já instável em uma completamente derrubada.

O Polly v8 integrado ao `HttpClientFactory` tem isso como opção de uma linha. Não tem desculpa para não usar.

#dotnet #csharp #backend #polly #distributedsystems

---
Caracteres: 1.121

---

## BLOCO 5 — CHECKLIST DE SÁBADO

### ✅ Setup de Sábado

**Repositório e board:**
- [ ] Criar repositório público `orderflow` no GitHub com `.gitignore` para .NET e `LICENSE` MIT
- [ ] Criar milestone "Semana 01 — Polly v8 + Resiliência HTTP" no GitHub Projects
- [ ] Criar as 5 issues do Bloco 2 no board, todas na coluna Backlog

**Ambiente local:**
- [ ] Instalar Docker Desktop (ou confirmar que está rodando) e testar com `docker ps`
- [ ] Baixar imagem do Toxiproxy antecipadamente: `docker pull ghcr.io/shopify/toxiproxy:2.9.0`
- [ ] Confirmar .NET 8 SDK instalado: `dotnet --version` (deve ser 8.x)
- [ ] Instalar Toxiproxy CLI para uso manual nos testes de Dia 3

**Conteúdo:**
- [ ] Agendar o post de terça no LinkedIn (ou rascunho salvo pronto para publicar na manhã de terça)
- [ ] Salvar o post de domingo em rascunho no LinkedIn para revisar após a semana

**Estudo preparatório (20 min):**
- [ ] Ler a documentação oficial `Microsoft.Extensions.Http.Resilience` (link no Bloco 1) e identificar a diferença entre `AddStandardResilienceHandler` e `AddResilienceHandler`

**Alinhamento mental:**
- [ ] Escrever em 2 frases o problema que o ResiliencePipeline resolve — se você conseguir explicar sem jargão, vai conseguir implementar sem se perder
- [ ] Revisar o projeto de testes de integração atual (se houver): há algum `HttpClient` sem política de retry que você poderia melhorar com o que aprender essa semana?
