#!/usr/bin/env bash
# Optional: pack SSO.Client for NuGet feed
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"
VERSION="${1:-0.0.0-local}"
mkdir -p artifacts/nupkg
dotnet pack src/SSO.Client/SSO.Client.csproj -c Release -o artifacts/nupkg /p:Version="$VERSION"
