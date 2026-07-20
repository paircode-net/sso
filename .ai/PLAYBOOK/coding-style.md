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
| Command | `{HttpVerb}{Entity}Command` ou `{HttpVerb}{Action}{Entity}Command` | `PostSampleCommand`, `PatchAcceptOrganizationInviteCommand` |
| Command response | `{HttpVerb}…CommandResponse` | `PostSampleCommandResponse` |
| Command handler | `{HttpVerb}…CommandHandler` | herda `ApplicationRequestHandler<...>` |
| Query | `Get{Entity}ByIdQuery` / `Get{Entities}ByFilterQuery` | |
| Notification | `{HttpVerb}{…}Notification` | `PostSampleNotification`, `PatchCancelOrganizationInviteNotification` |
| Domain service | `{DomainVerb}{Entity}ServiceRequest` + Handler | `CreateSampleServiceRequest`, `AcceptOrganizationInviteServiceRequest` |
| Specification | `{Entity}{Rule}Specification` | `SampleDescriptionAlreadyExistsSpecification` |
| Entity validator | `{Entity}Validator` | `SampleValidator` |
| Domain validator | `{DomainVerb}{Entity}SpecificationsValidator` | `CreateSampleSpecificationsValidator` |
| EF map | `{Entity}Map` | `SampleMap` |
| Controller | `{Entities}Controller` | `SamplesController` |
| Controller action | mesmo verbo HTTP do Command | `[HttpPatch]` ↔ `PatchCancel…Command` |
| Test class | `{Operation}Scenarios` | `PostSampleCommandScenarios` |

- Verbos HTTP nos **Commands** (e actions de Controller): **Post**, **Put**, **Patch**, **Delete** — sempre prefixam o nome.
- Mutações parciais / transições de estado (accept, decline, cancel, enable…): `Patch{Action}{Entity}Command` (ex.: `PatchAcceptOrganizationInviteCommand`, `PatchDeclineOrganizationInviteCommand`, `PatchCancelOrganizationInviteCommand`).
- Domain Services usam verbo de domínio (`Create`/`Update`/`Delete`/`Accept`…), não o verbo HTTP.
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
- Regra ou atribuição de domínio dentro de `*CommandHandler` (usar Domain Service + Specification).
- Command sem verbo HTTP (`AcceptXCommand`, `CancelXCommand`) — usar `PatchAcceptXCommand` / `PatchCancelXCommand`.
- Controller com `[HttpPost]` apontando para um `Patch*Command` (ou o inverso).
