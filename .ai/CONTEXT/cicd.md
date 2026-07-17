# Deploy e CI/CD (feature 00010)

## Artefato oficial

- **Dockerfile** na raiz (F00010-D3): imagem host-agnóstica.
- Runtime: `ASPNETCORE_URLS=http://+:8080`, `Sso__Database__AutoMigrate=false`.

## Scripts agnósticos (`scripts/ci/`)

| Script | Função |
|--------|--------|
| `build.sh` / `build.ps1` | restore + build Release |
| `test.sh` / `test.ps1` | `dotnet test` (gate de PR) |
| `migrate.sh` / `migrate.ps1` | `dotnet ef database update` Identity (+ Default opcional) |
| `pack-sdk.sh` | pack `SSO.Client` |

YAML de CI/CD só orquestra esses scripts (F00010-D2).

## Ambientes (F00010-D4)

1. **CI** em todo PR — testes obrigatórios.
2. **Staging** — após CI verde em `main` (workflow CD automático).
3. **Production** — GitHub Environment com **aprovação manual** de ops (`workflow_dispatch` ou promoção).

## Secrets / Key Vault (F00010-D5)

| Segredo | Uso |
|---------|-----|
| `IdentityConnection` | Migrate + app |
| `DefaultConnection` | Opcional |
| `SSO_KEY_VAULT_URI` | `Sso:Signing:KeyVaultUri` |
| `SSO_SIGNING_CERT_NAME` | `Sso:Signing:KeyVaultCertificateName` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Exporter AzureMonitor |
| Registry + Azure OIDC | Push/deploy da imagem |

A app, em Production, também carrega configuração do Key Vault quando `Sso:Signing:KeyVaultUri` (ou `Sso:KeyVault:Uri`) está definido (managed identity / `DefaultAzureCredential`).

## Checklist pós-deploy

1. `GET /health/live` → 200  
2. `GET /health/ready` → 200 (DB + signing)  
3. Login de smoke + métrica `sso.auth.login.success`  
4. Confirmar `AutoMigrate=false` no ambiente
