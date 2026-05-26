# AWS Integrations

## Visão Geral

As integrações com serviços AWS seguem os mesmos princípios da Clean Architecture adotados nas integrações com APIs externas. Os contratos são definidos no `[componente].Domain` e as implementações residem no `[componente].Infrastructure`, utilizando o **AWSSDK** oficial como SDK de comunicação.

---

## Localização

### Contratos (Domain)
```
[componente]/[componente].Domain/Integrations/Aws/[ServiceName]/
[componente]/[componente].Domain/Integrations/Aws/[ServiceName]/Interfaces/
[componente]/[componente].Domain/Integrations/Aws/[ServiceName]/Interfaces/I[ServiceName]Client.cs
[componente]/[componente].Domain/Integrations/Aws/[ServiceName]/[ServiceName]Request.cs
[componente]/[componente].Domain/Integrations/Aws/[ServiceName]/[ServiceName]Response.cs
```

### Implementação (Infrastructure)
```
[componente]/[componente].Infrastructure/Integrations/Aws/[ServiceName]/
[componente]/[componente].Infrastructure/Integrations/Aws/[ServiceName]/[ServiceName]Client.cs
[componente]/[componente].Infrastructure/Policies/
[componente]/[componente].Infrastructure/Policies/CircuitBreakerPolicy.cs
```

---

## Contrato da Integração

A interface é declarada no `[componente].Domain` e implementada no `[componente].Infrastructure`, seguindo a regra de inversão de dependência da Clean Architecture.

```csharp
public interface IStorageClient
{
    Task<UploadFileResponse> UploadAsync(UploadFileRequest request, CancellationToken cancellationToken);
}
```

---

## Implementação do Cliente AWS

```csharp
public class StorageClient(IAmazonS3 amazonS3, IMapper mapper) : IStorageClient
{
    public async Task<UploadFileResponse> UploadAsync(UploadFileRequest request, CancellationToken cancellationToken)
    {
        var putRequest = mapper.Map<PutObjectRequest>(request);

        var result = await amazonS3.PutObjectAsync(putRequest, cancellationToken);

        return mapper.Map<UploadFileResponse>(result);
    }
}
```

---

## Validação de Request

As requests de integração são validadas com FluentValidation antes de serem enviadas ao serviço AWS.

```csharp
public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    public UploadFileRequestValidator()
    {
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.Content).NotNull();
    }
}
```

---

## Convenções

- Um cliente por serviço AWS
- Nome da classe e interface seguem o padrão `[ServiceName]Client` e `I[ServiceName]Client`
- Requests e responses da integração nunca são reutilizados como DTOs internos da aplicação — são exclusivos da camada de integração
- O mapeamento entre os modelos internos e os contratos da integração é feito via AutoMapper
- Os clientes AWS (ex: `IAmazonS3`, `IAmazonSQS`) são injetados diretamente via DI e registrados pelo próprio AWSSDK

---

## Injeção de Dependência

O registro dos clientes AWS, políticas de resiliência e validators de integração está centralizado em `[componente]/[componente].Infrastructure/InfrastructureDependency.cs`.