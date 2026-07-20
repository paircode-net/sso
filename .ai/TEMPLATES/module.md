# Module — {{Context}} / {{Aggregate}}

Usar o aggregate **Sample** como referência de estrutura.

## Identificação

- Context: {{Default}}
- Entity: {{ }}
- Id type: Guid (padrão atual)
- Table: {{plural}}
- Aggregate root: Sim/Não
- Auditable: Sim/Não (alinhar Forge + código)

## Domain

```text
SSO.Core.Domain/{{Context}}/{{Aggregates}}/
  Entity/
  Services/          Create|Update|Delete (+ operações de domínio: Accept, Cancel…)
  Specifications/    regras de negócio (unicidade, estado, match…)
  Validations/
    EntityValidations/
    DomainValidations/   *SpecificationsValidator por operação
  Resources/
```

Atribuições de valor de domínio e `ValidateEntity`/`ValidateDomain` ficam nos **Services**, não nos Commands.

## Application

```text
SSO.Core.Application/{{Context}}/{{Aggregates}}/
  Commands/          orquestração apenas (IsValid → Service → Commit → Notify)
  Queries/
  Notifications/
```
## Data

```text
SSO.Infrastructures.Data/{{Context}}/
  EntityMappings/{{Entity}}Map.cs
  Migrations/
  {{Context}}DbContext(+Reader/Writer) — se novo contexto
```

## API

```text
SSO.Web.Api/Resources/{{Context}}Db/{{Entities}}Controller.cs
Route: api/{{context-lower}}/{{resource}}
```

## DI / Middleware

- Specs, validators, domain services registrados em `AddServices/*`
- MediatR: assemblies já registrados (novos handlers no mesmo assembly são descobertos)

## Forge

- Atualizar `.forge/project.json` se o scaffold for a fonte de verdade

## Testes

```text
UnitTests/.../{{Aggregate}}/...
IntegrationTests/{{Aggregate}}/...
Helpers/Data/{{Context}}/...
```

## Docs

- [ ] CONTEXT/modules.md
- [ ] CONTEXT/business.md (regras)
- [ ] glossary se termos novos
