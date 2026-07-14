# Playbook — Coding Style

## Propósito

Manter nomenclatura e estilo consistentes com o scaffold BAYSOFT / Forge.

## Princípios

- Preferir clareza e alinhamento ao padrão existente à “elegância” local.
- Nomes revelam intenção HTTP/caso de uso.
- Classes fechadas (`sealed`) por padrão.

## Boas práticas

- Usar namespaces espelhando pastas (`SSO.Core.Application.Default.Samples.Commands`).
- Manter tabs/indentação alinhadas ao arquivo existente.
- Mensagens de usuário via localizer, não literais soltos quando o padrão do arquivo usa localizer.
- Configurar `ConfigKeys` / `ConfigSuppressedProperties` nos requests ModelWrapper.

## Padrões obrigatórios

| Artefato | Padrão | Exemplo |
|----------|--------|---------|
| Command | `{Verb}{Entity}Command` | `PostSampleCommand` |
| Command response | `{Verb}{Entity}CommandResponse` | `PostSampleCommandResponse` |
| Command handler | `{Verb}{Entity}CommandHandler` | herda `ApplicationRequestHandler<...>` |
| Query | `Get{Entity}ByIdQuery` / `Get{Entities}ByFilterQuery` | |
| Notification | `{Verb}{Entity}Notification` | `PostSampleNotification` |
| Domain service | `{Verb}{Entity}ServiceRequest` + Handler | `CreateSampleServiceRequest` |
| Specification | `{Entity}{Rule}Specification` | `SampleDescriptionAlreadyExistsSpecification` |
| Entity validator | `{Entity}Validator` | `SampleValidator` |
| Domain validator | `{Verb}{Entity}SpecificationsValidator` | `CreateSampleSpecificationsValidator` |
| EF map | `{Entity}Map` | `SampleMap` |
| Controller | `{Entities}Controller` | `SamplesController` |
| Test class | `{Operation}Scenarios` | `PostSampleCommandScenarios` |

- Verbos HTTP nos commands: Post, Put, Patch, Delete.
- Entity herda `DomainEntity<TId>` (hoje `Guid`).

## Exemplos

```csharp
public sealed class PostSampleCommand : ApplicationRequest<Sample, PostSampleCommandResponse>
{
    public PostSampleCommand()
    {
        ConfigKeys(x => x.Id);
        ConfigSuppressedProperties(x => x.Id);
    }
}
```

```csharp
[Route("api/default/samples")]
public sealed class SamplesController : ResourceController { ... }
```

## Anti-patterns

- DTOs paralelos quando o padrão usa `ApplicationRequest`/`ApplicationResponse` + ModelWrapper.
- Handlers não-`sealed` sem motivo.
- Nomes genéricos (`Handler1`, `Manager`, `Helper` de domínio).
- Misturar cultura de localização sem necessidade (runtime default: `pt-BR`).
