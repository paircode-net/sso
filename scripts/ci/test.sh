#!/usr/bin/env bash
# Agnostic CI step (F00010-D2): run tests (required PR gate)
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"
dotnet test SSO.sln -c Release --no-build --logger "trx;LogFileName=test-results.trx" \
  --results-directory "$ROOT/artifacts/test-results"
