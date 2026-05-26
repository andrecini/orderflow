# Commit Standards

## Visão Geral

As mensagens de commit seguem o padrão **Conventional Commits** adaptado para o projeto. Todos os commits são escritos em **português** e seguem uma estrutura de três partes: cabeçalho, descrição e referência ao card do GitHub.

-----

## Estrutura

```
TIPO(escopo): descrição breve

- detalhe 1
- detalhe 2
- detalhe 3

CARD: #[id]
```

- **Escopo** é opcional — usar apenas quando o commit é restrito a um contexto específico
- **Descrição breve** deve ser clara e objetiva, no infinitivo — ex: “adicionar endpoint de criação de pedido”
- **Descrição completa** lista em tópicos o que foi feito — obrigatória quando o commit envolve mais de uma alteração
- **CARD** referencia o ID da issue/card no board do GitHub

-----

## Tipos

|Tipo      |Quando usar                                                       |
|----------|------------------------------------------------------------------|
|`feat`    |Nova funcionalidade                                               |
|`fix`     |Correção de bug                                                   |
|`refactor`|Refatoração sem alteração de comportamento                        |
|`test`    |Adição ou correção de testes                                      |
|`docs`    |Alterações em documentação                                        |
|`chore`   |Tarefas de manutenção — dependências, configurações, pipelines    |
|`style`   |Formatação, espaçamento, ponto e vírgula — sem alteração de lógica|
|`perf`    |Melhoria de performance                                           |
|`revert`  |Reversão de commit anterior                                       |

-----

## Exemplos

### Feature simples sem escopo

```
feat: adicionar endpoint de criação de pedido

- implementar CreateOrderEndpoint com Minimal API
- adicionar CreateOrderRequest e CreateOrderResponse
- configurar rota POST /api/v1/orders

CARD: #42
```

### Feature com escopo

```
feat(pagamentos): integrar gateway de pagamento

- implementar PaymentGatewayClient
- adicionar políticas de circuit breaker
- mapear request e response via AutoMapper

CARD: #57
```

### Correção de bug

```
fix(autenticação): corrigir validação de token expirado

- ajustar comparação de data de expiração para UTC
- adicionar log de warning para tokens próximos do vencimento

CARD: #63
```

### Refatoração

```
refactor(pedidos): extrair lógica de cálculo de frete para ValueObject

- criar ValueObject Freight com regras de cálculo
- remover lógica duplicada em OrderService
- atualizar testes unitários

CARD: #71
```

### Chore

```
chore: atualizar pacotes NuGet para versões mais recentes

- atualizar Swashbuckle.AspNetCore para 6.9.0
- atualizar FluentValidation para 11.10.0
- atualizar Confluent.Kafka para 2.6.0

CARD: #48
```

### Commit simples sem descrição completa

```
docs: atualizar README com instruções de setup

CARD: #35
```

-----

## Convenções

- Sempre em **português**
- Tipo sempre em **minúsculo**
- Descrição breve no **infinitivo** e sem ponto final
- Descrição completa é obrigatória quando o commit contém mais de uma alteração relevante
- O campo `CARD` é obrigatório — todo commit deve referenciar uma issue do board do GitHub
- Commits de `merge` gerados automaticamente pelo Git não precisam seguir o padrão
- Nunca misturar múltiplos tipos em um único commit — ex: feat + fix devem ser commits separados