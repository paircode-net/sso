# Decisions

Registro leve de decisões observáveis ou pendentes. Para decisões novas formais, usar também `.ai/TEMPLATES/adr.md` / `decision.md` e ADRs em `.ai/CONTEXT/adr/`.

## ADRs Aceitos (feature 00001)

| ADR | Título | Arquivo |
|-----|--------|---------|
| ADR-001 | OpenIddict como Authorization Server | [adr/ADR-001-openiddict-authorization-server.md](adr/ADR-001-openiddict-authorization-server.md) |
| ADR-002 | ASP.NET Identity para AuthN | [adr/ADR-002-aspnet-identity-authn.md](adr/ADR-002-aspnet-identity-authn.md) |
| ADR-003 | Multi-tenant + switch-context | [adr/ADR-003-multi-tenant-switch-context.md](adr/ADR-003-multi-tenant-switch-context.md) |
| ADR-004 | AuthZ contextual sem herança Branch no MVP | [adr/ADR-004-contextual-authz-no-branch-inheritance.md](adr/ADR-004-contextual-authz-no-branch-inheritance.md) |
| ADR-005 | Permissões efetivas no JWT | [adr/ADR-005-permissions-in-jwt.md](adr/ADR-005-permissions-in-jwt.md) |
| ADR-006 | Bounded context Identity / IdentityDb | [adr/ADR-006-identity-bounded-context.md](adr/ADR-006-identity-bounded-context.md) |
| ADR-007 | Claim `sid` + deny-list SQL (hot revoke) | [adr/ADR-007-sid-hot-revocation.md](adr/ADR-007-sid-hot-revocation.md) |
| ADR-008 | Herança opt-in Branch (pai→filhos) | [adr/ADR-008-branch-authz-inheritance-opt-in.md](adr/ADR-008-branch-authz-inheritance-opt-in.md) |

Plano: `.ai/WORK/2026-07-14-00001-plataforma-sso.md`.

## Decisões feature 00002 (AuthZ APIs admin)

| ID | Decisão | Status |
|----|---------|--------|
| F00002-D1 | AuthZ via `IAuthorizationHandler` + `[RequiresPermission]` lendo claim `permissions` (ADR-005) | **Aceito** |
| F00002-D2 | Platform permissions com `UserRoleAssignment.OrganizationId` null; rotas platform sem exigir `organization_id` no token; OrgAdmin amarrado ao token; PlatformAdmin pode cross-org | **Aceito** |
| F00002-D3 | `menus/effective` permanece admin/diagnóstico (`sso.admin.menus`); products usam JWT | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00002-authz-apis-admin.md`.

## Decisões feature 00003 (Portal admin)

| ID | Decisão | Status |
|----|---------|--------|
| F00003-D1 | Portal v1 = Razor Area `/Admin` no `SSO.Web.Api` (não SPA/Blazor Server) | **Aceito** |
| F00003-D2 | Vínculo org↔user só via **convite + aceite** do usuário (sem membership silenciosa) | **Aceito** |
| F00003-D3 | Contexto org/branch no portal via grant **`switch_context`** (ADR-003); cookie não é fonte de verdade | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00003-portal-admin-por-papel.md`.

## Decisões feature 00004 (SDK integração)

| ID | Decisão | Status |
|----|---------|--------|
| F00004-D1 | Pacote `SSO.Client` no monorepo (`src/SSO.Client/`) | **Aceito** |
| F00004-D2 | Mesmo épico: .NET + JS/TS (`requirePermission` + claims) | **Aceito** |
| F00004-D3 | Sample BFF entregável em `samples/sso-bff` (+ `samples/product-api`) | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00004-sdk-integracao-produtos.md` (**implementado**).

## Decisões feature 00005 (Revogação quente + sessão)

| ID | Decisão | Status |
|----|---------|--------|
| F00005-D1 | Deny-list em **SQL** + claim `sid` no access token (cache curto no SDK); Redis depois se escala exigir | **Aceito** |
| F00005-D2 | Hot check no SDK **ligado por padrão**, com opt-out explícito (`SsoClient:RevocationCheck`) | **Aceito** |
| F00005-D3 | SLA de revoke **≤ 60 s** (cache SDK 30–60s; configurável p/ 15s em high-security) | **Aceito** |
| F00005-D4 | Incluir MVP de notificações: outbox + webhook HMAC `session.revoked` por AuthClient | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00005-revogacao-quente-sessao.md` (**implementado**).

## Decisões feature 00006 (Federação Google + LDAP)

| ID | Decisão | Status |
|----|---------|--------|
| F00006-D1 | JIT provisioning **por org/IdP** (default off = pre-provision) | **Aceito** |
| F00006-D2 | LDAP via **System.DirectoryServices.Protocols** | **Aceito** |
| F00006-D3 | Auto-link por e-mail se `email_verified` (IdP confiável) | **Aceito** |
| F00006-D4 | MVP mapeamento **grupo LDAP → Role** no login (config por org) | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00006-federacao-google-ldap.md` (**implementado**).

## Decisões feature 00007 (Consent + AuthClients)

| ID | Decisão | Status |
|----|---------|--------|
| F00007-D1 | Consent “Remember” **por client**: first-party permanente; Always com TTL configurável (default 180d) | **Aceito** |
| F00007-D2 | Metadados **híbridos**: OpenIddict = protocolo; tabela leve admin (`IsSystem`, TTL, policy, `IsFirstParty`) | **Aceito** |
| F00007-D3 | Scopes custom `{product_code}.{feature}` | **Aceito** |
| F00007-D4 | First-party via flag no registro (`IsFirstParty`); `RequireConsent=Never` só se first-party | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00007-consent-authclients.md` (**implementado**).

## Decisões feature 00008 (Claims tipadas)

| ID | Decisão | Status |
|----|---------|--------|
| F00008-D1 | Claim tipada no JWT com prefixo **`sso_c_{code}`** (namespacing; evita colisão OIDC/SSO) | **Aceito** |
| F00008-D2 | Novo claim **`claim_ver`** separado do `perm_ver` (semântica limpa permissões vs claims) | **Aceito** |
| F00008-D3 | Origem **`UserClaimAssignment` + `RoleClaim`** (usuário e papel) | **Aceito** |
| F00008-D4 | Value types no MVP = **escalares** (`string`, `int`, `bool`); sem JSON/datetime | **Aceito** |
| F00008-D5 | Precedência no merge: **`UserClaimAssignment` vence `RoleClaim`** (mesmo ClaimDefinition/contexto) | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00008-claims-tipadas.md` (**implementado**).

## Decisões feature 00009 (Herança Branch opt-in)

| ID | Decisão | Status |
|----|---------|--------|
| F00009-D1 | Opt-in **em duas camadas**: flag Organization (`Off`\|`InheritFromAncestors`) **+** `Inheritable` no assignment; herança só se ambos permitirem | **Aceito** |
| F00009-D2 | Claims tipadas (00008) usam a **mesma política** de herança que permissions | **Aceito** |
| F00009-D3 | Claims tipadas: herança clássica — valor no branch ativo vence; senão sobe ancestrais `Inheritable` | **Aceito** |
| F00009-D4 | **ADR-008 aditivo**; ADR-004 permanece como default (superseded parcialmente quando flag On) | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00009-heranca-branch-opt-in.md` (**implementado**). ADR: [ADR-008](adr/ADR-008-branch-authz-inheritance-opt-in.md).

## Decisões feature 00010 (Observabilidade + CI/CD)

| ID | Decisão | Status |
|----|---------|--------|
| F00010-D1 | Observabilidade via **OpenTelemetry** com exporter **plugável por config** (Console em Dev; App Insights ou OTLP em Prod) — sem travar vendor | **Aceito** |
| F00010-D2 | CI/CD com **steps agnósticos** (scripts `dotnet` build/test/migrate) + YAML fino conforme host do repo | **Aceito** |
| F00010-D3 | **Dockerfile como artefato oficial host-agnóstico** (Container Apps / K8s / App Service for Containers) | **Aceito** |
| F00010-D4 | **Staging automático** após CI verde; **Production com aprovação manual** de grupo de ops | **Aceito** |
| F00010-D5 | **Key Vault reference no CD** (fecha D9): signing keys via KV + managed identity | **Aceito** |

Plano: `.ai/WORK/2026-07-16-00010-observabilidade-cicd.md` (**implementado**).

## Decisões feature 00001 (D1–D12)

| ID | Decisão | Status |
|----|---------|--------|
| F00001-D1 | Context `Identity` / `IdentityDb` separado do Sample | Aceito (ADR-006) |
| F00001-D2 | Switch-context + claims `organization_id`/`branch_id` (não header como fonte de verdade) | Aceito (ADR-003) |
| F00001-D3 | Permissões efetivas embutidas no JWT (TTL curto + reemissão no switch-context) | Aceito (ADR-005 revisado) |
| F00001-D4 | Sem herança automática Branch pai→filho no MVP | Aceito (ADR-004) |
| F00001-D5 | Identity para AuthN; roles/claims de produto no domínio SSO | Aceito (ADR-002, ADR-004) |
| F00001-D6 | Login/consent Razor em `SSO.Web.Api` no MVP | Aceito — consent **explícito** por client (00007); seed first-party permanece implícito/`Never` |
| F00001-D7 | IdPs: Entra ID → Google → LDAP | Aceito — Google/LDAP **implementados** (00006) |
| F00001-D8 | Manter Sample até Fase 2 estável | Aceito |
| F00001-D9 | Dev: key/cert local; Prod: Key Vault + rotação | Aceito (ADR-001) — **implementado** via F00010-D5 (`SigningCertificateResolver` + CD) |
| F00001-D10 | `Product` ≠ `AuthClient` | Aceito |
| F00001-D11 | Admin MVP = API-only | **Superado parcialmente** por 00003 (portal Razor `/Admin`) |
| F00001-D12 | Soft-delete + auditoria nas entidades Identity | Aceito (P-008 parcial) |

## Decisões observadas no código (implícitas)

| ID | Decisão | Evidência | Status |
|----|---------|-----------|--------|
| D-001 | Usar Clean Architecture + CQRS Full com BAYSOFT.Abstractions | Estrutura de soluções + handlers | Ativa |
| D-002 | Id de entidades: Guid | Forge + DomainEntity\<Guid\> | Ativa |
| D-003 | Classes preferencialmente sealed | Forge `UseSealedClasses` + código | Ativa |
| D-004 | SQL Server + EF Core como persistência | csproj Data + connection string | Ativa |
| D-005 | Composition root no projeto Middleware | `Configurations.cs` | Ativa |
| D-006 | Localização default pt-BR na API | `UseMiddleware` | Ativa |
| D-007 | Migrations aplicadas no startup | `UseMigrations()` | Ativa (reavaliar p/ produção) |
| D-008 | Testes com MSTest + InMemory + TestServer | `SSO.Tests` | Ativa |
| D-009 | AuthN/AuthZ habilitados (Fase 0) | `AddIdentityFoundation` + pipeline Auth | Ativa — supersede scaffold comentado |
| D-010 | Mail service stub, DI desligado | `MailService` + AddDomainServices | Temporária |

## Decisões pendentes (produto / plataforma)

| ID | Tema | Status |
|----|------|--------|
| P-001 | Modelo de autenticação/autorização do SSO | **Definido** (ADRs 001–005); implementação pendente (Fases 0+) |
| P-002 | Stack de observabilidade | **Aceito** — OpenTelemetry + exporter plugável (F00010-D1). Ver [00010](../WORK/2026-07-16-00010-observabilidade-cicd.md) |
| P-003 | CI/CD e estratégia de deploy | **Aceito** — steps agnósticos + Dockerfile host-agnóstico + gate Prod (F00010-D2..D5). Ver [00010](../WORK/2026-07-16-00010-observabilidade-cicd.md) |
| P-004 | Estratégia de migrations em produção (auto vs pipeline) | **Aceito** — Production: `Sso:Database:AutoMigrate=false` por default; aplicar via pipeline (`dotnet ef database update`). Ver `phase6-hardening.md` |
| P-005 | Analyzers / EditorConfig / Nullable uniforme | Pendente de definição |
| P-006 | Domínio de negócio real além de Sample | **Definido (planejamento)** — Identity context + aggregates 00001; código pendente |
| P-007 | Papel e conteúdo de `SSO.Shared` | Pendente de definição |
| P-008 | Tratamento de auditoria (Forge auditable vs código) | **Parcial** — Identity: soft-delete + auditoria (F00001-D12); restante a definir |

## Como evoluir este arquivo

- Registrar decisões novas no topo da tabela ou via ADR linkado.
- Marcar decisões temporárias quando forem propositalmente incompletas.
- Não registrar preferências pessoais sem impacto arquitetural.
