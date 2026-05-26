# Authentication & Authorization

## Visão Geral

A autenticação é baseada em **Basic Auth** para obtenção do token e **Bearer JWT** para acesso aos endpoints protegidos. O fluxo de autenticação é gerenciado pela própria aplicação, sem dependência de serviços externos de identidade. As permissões de acesso são embutidas no JWT como claims e validadas via policies do ASP.NET Core.

---

## Fluxo de Autenticação

1. O cliente envia credenciais via **Basic Auth** para `POST /api/v1/authenticate`
2. A aplicação valida o usuário na collection `user-services` do MongoDB, comparando a senha com o hash armazenado
3. Em caso de sucesso, a aplicação retorna um **Bearer JWT** com as claims do usuário
4. O cliente utiliza o Bearer JWT para acessar os endpoints protegidos
5. As claims do JWT são validadas automaticamente pelo pipeline do ASP.NET Core

---

## Endpoint de Autenticação

**Rota:** `POST /api/v1/authenticate`

**Request (Basic Auth):**
```
Authorization: Basic {base64(username:password)}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": "service-user",
  "expiresIn": 3600,
  "expiresAt": "2024-03-15T11:30:00Z"
}
```

---

## Usuários de Serviço

Os usuários de serviço estão armazenados na collection `user-services` do MongoDB. A senha é armazenada como hash — nunca em texto plano.

```json
{
  "_id": "...",
  "username": "service-user",
  "passwordHash": "...",
  "roles": ["orders:read", "orders:write"],
  "isActive": true
}
```

---

## Claims e Roles

As roles do usuário são embutidas no JWT como claims durante o login e utilizadas pelo ASP.NET Core para validar o acesso aos endpoints. Mudanças nas roles de um usuário só terão efeito após a expiração do token atual.

```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.Name, user.Username),
    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
};

foreach (var role in user.Roles)
    claims.Add(new Claim(ClaimTypes.Role, role));
```

### Uso nos Endpoints

```csharp
app.MapPost("/api/v1/orders", CreateOrder)
   .WithName("CreateOrder")
   .RequireAuthorization("orders:write")
   .WithTags("Orders")
   .WithOpenApi();
```

### Registro de Policies

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("orders:read", policy => policy.RequireRole("orders:read"));
    options.AddPolicy("orders:write", policy => policy.RequireRole("orders:write"));
});
```

---

## Cache de Autenticação

Os dados do usuário autenticado são armazenados em cache via `IMemoryCache` por **10 minutos** para evitar consultas repetidas ao MongoDB a cada validação de token.

```csharp
public class UserCacheService(IMemoryCache cache, IUserRepository userRepository)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<UserService?> GetAsync(string username, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(username, out UserService? cached))
            return cached;

        var user = await userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user is not null)
            cache.Set(username, user, CacheDuration);

        return user;
    }
}
```

---

## Localização

```
[componente]/[componente].Api/Endpoints/Auth/
[componente]/[componente].Api/Endpoints/Auth/AuthenticateEndpoint.cs
[componente]/[componente].Application/Middlewares/
[componente]/[componente].Application/Services/UserCacheService.cs
[componente]/[componente].Domain/Integrations/MongoDb/UserServices/
[componente]/[componente].Domain/Integrations/MongoDb/UserServices/UserService.cs
[componente]/[componente].Infrastructure/Integrations/MongoDb/UserServices/UserServiceRepository.cs
```

---

## Convenções

- A senha nunca trafega ou é armazenada em texto plano — sempre comparada via hash
- O Bearer JWT é o único mecanismo de autenticação após o login — Basic Auth é exclusivo do endpoint `/authenticate`
- Roles seguem o padrão `[recurso]:[ação]` — ex: `orders:read`, `orders:write`, `payments:read`
- Todo endpoint protegido declara explicitamente `.RequireAuthorization("policy-name")`
- O cache de usuário é invalidado automaticamente após 10 minutos — não há invalidação manual
- A configuração do JWT (secret, issuer, audience, expiração) é gerenciada via `appsettings` e nunca hardcoded
- O registro das policies e da autenticação JWT está centralizado em `[componente]/[componente].Api/ApiDependency.cs`