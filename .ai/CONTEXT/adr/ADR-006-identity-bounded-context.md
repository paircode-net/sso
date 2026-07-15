# ADR-006 — Bounded context Identity / IdentityDb

- Status: Aceito
- Data: 2026-07-14
- Decisores: Equipe SSO / feature 00001

## Contexto

O scaffold atual usa o context **Default** (`DefaultDb` / `DefaultDbContext`) com o aggregate Sample. Misturar o núcleo SSO (Identity, OpenIddict, Organization, etc.) no mesmo schema dificulta limites claros e a eventual remoção do Sample.

## Decisão

Criar bounded context **`Identity`**:

| Item | Valor |
|------|-------|
| Context name | Identity |
| Schema EF | `IdentityDb` |
| DbContext | `IdentityDbContext` (+ Reader/Writer) |
| Rotas API de gestão | `api/identity/{resource}` |
| Controllers | `SSO.Web.Api/Resources/IdentityDb/` |

O context **Default** permanece para Sample até a Fase 2 estável (D8). Stores ASP.NET Identity e OpenIddict ficam em `IdentityDbContext`.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| Tudo em DefaultDb | Menos DbContexts | Mistura scaffold e núcleo SSO |
| Microserviço Identity separado | Isolamento forte | Fora do escopo; quebra monólito CQRS atual sem ADR de plataforma |

## Consequências

### Positivas

- Separação clara Sample vs SSO.
- Migrations e seeds independentes.
- Alinha ao playbook de multi-context (`{Name}DbContext` + schema `{Name}Db`).

### Negativas / trade-offs

- Dois DbContexts e connection strings/migrations a gerenciar.
- Composition root Middleware cresce (registro duplo).

## Impacto no código

- Novas pastas `Domain/Identity`, `Application/Identity`, `Data/Identity`.
- Middleware: registrar `IdentityDbContext` e migrations.
- Tests: helpers Data para Identity + isolamento.

## Referências

- Feature 00001 (D1, D8)
- `.ai/PLAYBOOK/architecture.md`
- ADR-001, ADR-002
)
