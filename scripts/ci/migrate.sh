#!/usr/bin/env bash
# Agnostic CD step (P-004 / F00010): apply EF migrations before deploy.
# Requires IdentityConnection (and optionally DefaultConnection) in the environment.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

: "${IdentityConnection:?IdentityConnection secret/env is required}"

export ConnectionStrings__IdentityConnection="$IdentityConnection"
if [[ -n "${DefaultConnection:-}" ]]; then
  export ConnectionStrings__DefaultConnection="$DefaultConnection"
fi

dotnet tool restore 2>/dev/null || true
dotnet ef database update \
  --project src/SSO.Infrastructures.Data/SSO.Infrastructures.Data.csproj \
  --startup-project src/SSO.Web.Api/SSO.Web.Api.csproj \
  --context IdentityDbContext

if [[ -n "${DefaultConnection:-}" ]]; then
  dotnet ef database update \
    --project src/SSO.Infrastructures.Data/SSO.Infrastructures.Data.csproj \
    --startup-project src/SSO.Web.Api/SSO.Web.Api.csproj \
    --context DefaultDbContext || true
fi
