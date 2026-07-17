# Agnostic CI step (F00010-D2): run tests
$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "../..")
Set-Location $Root
$out = Join-Path $Root "artifacts/test-results"
New-Item -ItemType Directory -Force -Path $out | Out-Null
dotnet test SSO.sln -c Release --no-build --logger "trx;LogFileName=test-results.trx" --results-directory $out
