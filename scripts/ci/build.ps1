# Agnostic CI step (F00010-D2): restore + build
$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "../..")
Set-Location $Root
dotnet restore SSO.sln
dotnet build SSO.sln -c Release --no-restore
