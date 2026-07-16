# External IdPs & hardening (Phase 6)

> See **[external-idps.md](external-idps.md)** for Google/LDAP (feature 00006) homologation, JIT, auto-link, and LDAP group→role maps.

## Hardening (summary)

| Control | Config | Behavior |
|---------|--------|----------|
| CORS | `Sso:Cors` | Policy `SsoCors` with allowlist |
| Rate limit | `Sso:RateLimit` | Fixed window on `/Account/*`, `/connect/token`, `/connect/authorize` |
| Lockout | `Sso:Lockout` | Identity lockout (default 5 / 5 min) |
| Signing | `Sso:Signing` | Dev certs by default; Production requires cert path (Key Vault rotation = D9 ops) |
| Migrations (P-004) | `Sso:Database:AutoMigrate` | **false** by default in Production; apply via `dotnet ef database update` in pipeline |

## P-004 decision

**Accepted:** Production does **not** auto-migrate on startup unless `Sso:Database:AutoMigrate=true` is explicitly set. Pipeline owns schema upgrades.

## Checklists

- `.ai/CHECKLISTS/security-checklist.md`
- `.ai/CHECKLISTS/release-checklist.md`
