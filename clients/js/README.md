# @sso/client

JavaScript/TypeScript helpers for SSO JWT claims (feature 00004). **Does not verify signatures** — use the BFF (`samples/sso-bff`) or your API for validation.

```bash
cd clients/js
npm install
npm run build
npm test
```

```ts
import { parseJwtPayload, requirePermission, getPermissionVersion } from "@sso/client";

const payload = parseJwtPayload(accessToken);
if (!requirePermission(payload, "hq.reports")) throw new Error("forbidden");
const cacheKey = getPermissionVersion(payload);
```

Claim names match `SSO.Shared` / `product-integration.md`.
