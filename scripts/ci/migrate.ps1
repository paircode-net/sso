# Agnostic CD step (P-004): apply EF migrations. Requires $env:IdentityConnection.
$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "../..")
Set-Location $Root

if (-not $env:IdentityConnection) {
  throw "IdentityConnection env var is required."
}

$env:ConnectionStrings__IdentityConnection = $env:IdentityConnection
if ($env:DefaultConnection) {
  $env:ConnectionStrings__DefaultConnection = $env:DefaultConnection
}

dotnet ef database update `
  --project src/SSO.Infrastructures.Data/SSO.Infrastructures.Data.csproj `
  --startup-project src/SSO.Web.Api/SSO.Web.Api.csproj `
  --context IdentityDbContext

if ($env:DefaultConnection) {
  dotnet ef database update `
    --project src/SSO.Infrastructures.Data/SSO.Infrastructures.Data.csproj `
    --startup-project src/SSO.Web.Api/SSO.Web.Api.csproj `
    --context DefaultDbContext
}
