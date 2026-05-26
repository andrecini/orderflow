# API Documentation

## Visão Geral

A documentação das APIs é gerada automaticamente a partir do código-fonte usando **Swashbuckle.AspNetCore**, integrado ao pipeline do ASP.NET Core 8. O Swagger UI fica disponível em `/swagger` nos ambientes de desenvolvimento e homologação. Em produção, o endpoint é desabilitado por padrão.

---

## Geração da Documentação

A documentação é derivada de três fontes combinadas:

- **XML comments** nos controllers e DTOs (`<summary>`, `<remarks>`, `<param>`, `<returns>`)
- **Data Annotations** nos modelos de request e response (`[Required]`, `[MaxLength]`, `[Range]`, etc.)
- **Atributos de resposta** nos endpoints (`[ProducesResponseType]`)

O arquivo `.csproj` de cada projeto de API deve conter:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

---

## Convenções Adotadas

### Rotas
- Padrão: `kebab-case` — ex: `/api/v1/payment-methods`
- Versionamento obrigatório via prefixo de rota: `/api/v1/`, `/api/v2/`
- Sem trailing slash

### Nomenclatura JSON
- Propriedades em `camelCase`
- Datas no formato ISO 8601: `2024-03-15T10:30:00Z`
- Enums serializados como `string`, não inteiro

### Status Codes
Todos os endpoints documentam explicitamente os seguintes códigos quando aplicáveis:

| Código | Uso |
|--------|-----|
| 200 | Sucesso com corpo de resposta |
| 201 | Recurso criado (POST) |
| 204 | Sucesso sem corpo (DELETE, PUT sem retorno) |
| 400 | Falha de validação — retorna `ValidationProblemDetails` |
| 401 | Não autenticado |
| 403 | Sem permissão |
| 404 | Recurso não encontrado |
| 409 | Conflito de estado (ex: recurso já existe) |
| 500 | Erro interno — retorna `ProblemDetails` |

### Modelo de Erro Padrão
O projeto usa `ProblemDetails` (RFC 7807) para todos os erros. O middleware global de exceções garante que erros não tratados também sigam esse formato.

---

## Minimal APIs

Para endpoints implementados com Minimal APIs (adotado em novos módulos a partir da v2), a documentação segue o padrão fluente:

```csharp
app.MapPost("/api/v1/orders", CreateOrder)
   .WithName("CreateOrder")
   .WithSummary("Cria um novo pedido")
   .WithDescription("Processa a criação de um pedido com validação de estoque e cálculo de frete.")
   .WithTags("Orders")
   .WithOpenApi();
```

Controllers MVC ainda são usados em módulos legados e coexistem com Minimal APIs no mesmo projeto.

---

## Autenticação no Swagger

O Swagger UI está configurado com suporte a JWT Bearer. Para testar endpoints autenticados, o token deve ser informado no botão **Authorize** no formato:

```
Bearer {token}
```

A configuração do `SecurityDefinition` e `SecurityRequirement` está centralizada em `SwaggerConfiguration.cs` dentro do projeto de Api.

---

## Versionamento de API

O versionamento é feito via `Asp.Versioning` (pacote oficial Microsoft). Cada versão possui sua própria definição no Swagger, acessível via dropdown no Swagger UI.

- Endpoints deprecated são marcados com `[Obsolete]` e incluem no `<remarks>` a versão de descontinuação e o endpoint substituto.
- Não há remoção de versões sem comunicação prévia e período de deprecação de pelo menos um sprint.

---

## Localização da Configuração

| Artefato | Localização |
|----------|-------------|
| Configuração do Swagger | `[componente]/[componente].Api/Configuration/SwaggerConfiguration.cs` |
| XML docs gerados | `bin/` — incluídos no build automaticamente |
| Modelos de request/response | `[componente]/[componente].Api/Dtos/` |
| Filtros customizados do Swagger | `[componente]/[componente].Api/Swagger/Filters/` |

---

## Ambientes

| Ambiente | Swagger UI | Endpoint OpenAPI JSON |
|----------|------------|----------------------|
| Development | Habilitado | `/swagger/v1/swagger.json` |
| Staging | Habilitado | `/swagger/v1/swagger.json` |
| Production | Desabilitado | Indisponível |