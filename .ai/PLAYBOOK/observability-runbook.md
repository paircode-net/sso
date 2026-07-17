# Runbook — Observabilidade SSO (feature 00010)

## Endpoints

| Path | Uso |
|------|-----|
| `GET /health/live` | Liveness (processo vivo; sem checks pesados) |
| `GET /health/ready` | Readiness (Identity DB + signing configurado) |

## Métricas (meter `SSO.Auth`)

| Nome | Quando |
|------|--------|
| `sso.auth.login.success` / `failure` | Login local e externo |
| `sso.auth.token.issued` | Token endpoint (`grant_type` tag) |
| `sso.auth.switch_context.success` / `failure` | Grant switch_context |
| `sso.auth.rate_limited` | 429 em superfícies Auth |
| `sso.jwt.permissions_count` / `sso.jwt.approximate_size` | Emissão de principal de usuário |

Exporter: `Sso:Observability:Exporter` = `Console` \| `Otlp` \| `AzureMonitor` \| `None`.

## Login failures spike

1. Verificar métrica `sso.auth.login.failure` (tag `reason`) e logs Serilog (sem senha).
2. Conferir lockout (`Sso:Lockout`) e rate limit (`sso.auth.rate_limited`).
3. Se IdP externo: logs `external:*` e disponibilidade Entra/Google/LDAP.
4. Health: `/health/ready` — se Identity DB down, login falha em cascata.

## IdP errors

1. Filtrar logs por `detail` contendo `external:` / provider.
2. Validar secrets no Key Vault (`Sso:ExternalAuth:*`).
3. Traces OTEL em `/Account/ExternalLogin` e `/signin-*`.
4. Confirmar clock skew e redirect URIs do IdP.

## Deploy / migrate

Ordem obrigatória (P-004): **migrate → deploy app** com `Sso:Database:AutoMigrate=false`.

Scripts: `scripts/ci/migrate.sh` (ou `.ps1`). Signing em prod: Key Vault (`Sso:Signing:KeyVaultUri` + `KeyVaultCertificateName`).
