# External IdPs & hardening (Phase 6)

## IdP order (D7)

1. **Microsoft Entra ID** — OpenID Connect (`Sso:ExternalAuth:Entra`). Homologation catalog row: `entra-homolog`.
2. **Google** — OIDC scaffolding (`Sso:ExternalAuth:Google`); seed `google-stub` disabled until enabled in config.
3. **LDAP** — stub (`ILdapAuthenticationStub`); seed `ldap-stub` disabled.

### Homologation Entra

```json
"Sso": {
  "ExternalAuth": {
    "Entra": {
      "Enabled": true,
      "TenantId": "<tenant-guid-or-common>",
      "ClientId": "<app-registration-client-id>",
      "ClientSecret": "<from-user-secrets-or-kv>",
      "CallbackPath": "/signin-entra"
    }
  }
}
```

Store secrets in User Secrets / env / Key Vault — not in git. Login UI shows external buttons when schemes are registered.

## Hardening

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
