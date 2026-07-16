# External IdPs & hardening

> Feature 00006 + Phase 6. Related: F00001-D7, F00006-D1..D4, ADR order Entra → Google → LDAP.

## IdP order

1. **Microsoft Entra ID** — OIDC (`Sso:ExternalAuth:Entra`). Catalog: `entra-homolog`.
2. **Google** — OIDC real (`Sso:ExternalAuth:Google`). Catalog: `google`.
3. **LDAP / AD** — bind via `System.DirectoryServices.Protocols` (`Sso:ExternalAuth:Ldap`). Catalog: `ldap`. UI: `/Account/LoginWithLdap`.

## Policies (00006)

| Policy | Behavior |
|--------|----------|
| JIT | `ExternalIdentityProviders.AllowJitProvisioning` (default **false**). PATCH `api/identity/external-identity-providers/{id}`. |
| Auto-link | Same e-mail as local user + verified e-mail → link `UserLogin`. Google requires `email_verified=true`. Entra/LDAP treated as verified directory sources. |
| LDAP groups → Roles | `LdapGroupRoleMaps` + sync on login (direct `memberOf` only). API: `api/identity/ldap-group-role-maps`. |

## Homologation — Google

```json
"Sso": {
  "ExternalAuth": {
    "Google": {
      "Enabled": true,
      "ClientId": "<google-oauth-client-id>",
      "ClientSecret": "<from-user-secrets-or-kv>",
      "CallbackPath": "/signin-google"
    }
  }
}
```

Redirect URI: `https://<host>/signin-google`. Enable catalog row `google` (`IsEnabled=true`). Secrets never in git.

## Homologation — LDAP

```json
"Sso": {
  "ExternalAuth": {
    "Ldap": {
      "Enabled": true,
      "Host": "ldap.contoso.local",
      "Port": 636,
      "UseSsl": true,
      "EnforceTlsInProduction": true,
      "BaseDn": "DC=contoso,DC=com",
      "BindDnPattern": "{username}@contoso.com",
      "UserSearchFilter": "(sAMAccountName={username})",
      "DefaultOrganizationId": "11111111-1111-1111-1111-111111111111",
      "TimeoutSeconds": 10
    }
  }
}
```

Production rejects LDAP when `UseSsl=false` and `EnforceTlsInProduction=true`.

## Hardening

| Control | Config | Behavior |
|---------|--------|----------|
| CORS | `Sso:Cors` | Policy `SsoCors` allowlist |
| Rate limit | `Sso:RateLimit` | Fixed window on `/Account/*`, `/connect/*` |
| Lockout | `Sso:Lockout` | Identity lockout |
| Signing | `Sso:Signing` | Dev certs default; Production cert path |
| Migrations (P-004) | `Sso:Database:AutoMigrate` | false in Production by default |

## Checklists

- `.ai/CHECKLISTS/security-checklist.md`
- `.ai/CHECKLISTS/release-checklist.md`
