#!/usr/bin/env bash
# Agnostic CI step (F00010-D2): restore + build
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"
dotnet restore SSO.sln
dotnet build SSO.sln -c Release --no-restore
