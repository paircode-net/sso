# Dependencies

Pacotes e referências observadas nos `.csproj` (versões no momento da descoberta). Transientemente há MediatR/FluentValidation via BAYSOFT e ModelWrapper.

## Projeto → ProjectReferences

| Projeto | Referencia |
|---------|------------|
| `SSO.Web.Api` | Middleware, Shared |
| `SSO.Middleware` | Application, Domain, Data, Services |
| `SSO.Core.Application` | Domain, Shared |
| `SSO.Core.Domain` | Shared |
| `SSO.Infrastructures.Data` | Domain |
| `SSO.Infrastructures.Services` | Domain |
| `SSO.Tests` | Application, Domain, Data, Middleware, Web.Api |
| `SSO.Shared` | (sem project refs de domínio) |

## Pacotes NuGet diretos (por projeto)

### SSO.Web.Api

- `Microsoft.AspNetCore.TestHost` 10.0.2
- `Microsoft.EntityFrameworkCore.Design` 10.0.2
- `Swashbuckle.AspNetCore` 10.1.1

### SSO.Core.Domain

- `BAYSOFT.Abstractions` 10.0.3
- `Microsoft.Extensions.Identity.Stores` 10.0.3

### SSO.Infrastructures.Data

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.3
- `Microsoft.EntityFrameworkCore.Design` 10.0.3
- `Microsoft.EntityFrameworkCore.SqlServer` 10.0.3
- `Microsoft.Extensions.DependencyInjection` 10.0.7
- `OpenIddict.EntityFrameworkCore` 7.5.0

### SSO.Middleware

- `FrameworkReference` → `Microsoft.AspNetCore.App`
- `Microsoft.AspNetCore.Localization.Routing` 2.3.9
- `Microsoft.EntityFrameworkCore.InMemory` 10.0.3
- DI Abstractions/Extensions 10.0.7
- `OpenIddict.AspNetCore` 7.5.0

### SSO.Shared

- DI Extensions/Abstractions 10.0.2

### SSO.Tests

- `coverlet.collector` 8.0.0
- `Microsoft.EntityFrameworkCore.InMemory` 10.0.3
- `Microsoft.NET.Test.Sdk` 18.0.1
- `Moq` 4.20.72
- `MSTest.TestAdapter` / `MSTest.TestFramework` 4.1.0

### SSO.Core.Application / SSO.Infrastructures.Services

- Sem PackageReference direto listado (dependem via projeto)

## Ferramentas de desenvolvimento

| Ferramenta | Uso |
|------------|-----|
| `dotnet-ef` | Migrations (documentado no README) |
| Forge (`.forge`) | Scaffold de entidades/contextos |

## Observações

- Versões EF/Design misturam 10.0.2 e 10.0.3 entre projetos — potencial inconsistência.
- `Microsoft.AspNetCore.Localization.Routing` 2.3.9 é major antigo frente a net10 — validar necessidade.
- Central Package Management (`Directory.Packages.props`) **não** está em uso.

## A definir

- Política de versionamento/atualização de pacotes
- Introduction de package centralization
- Key Vault / rotação de chaves OpenIddict em produção
