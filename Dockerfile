# Official SSO image (F00010-D3) — host-agnostic (Container Apps / K8s / App Service for Containers).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SSO.sln ./
COPY src/SSO.Shared/SSO.Shared.csproj src/SSO.Shared/
COPY src/SSO.Core.Domain/SSO.Core.Domain.csproj src/SSO.Core.Domain/
COPY src/SSO.Core.Application/SSO.Core.Application.csproj src/SSO.Core.Application/
COPY src/SSO.Infrastructures.Data/SSO.Infrastructures.Data.csproj src/SSO.Infrastructures.Data/
COPY src/SSO.Infrastructures.Services/SSO.Infrastructures.Services.csproj src/SSO.Infrastructures.Services/
COPY src/SSO.Middleware/SSO.Middleware.csproj src/SSO.Middleware/
COPY src/SSO.Web.Api/SSO.Web.Api.csproj src/SSO.Web.Api/
COPY src/SSO.Client/SSO.Client.csproj src/SSO.Client/

RUN dotnet restore src/SSO.Web.Api/SSO.Web.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/SSO.Web.Api/SSO.Web.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Sso__Database__AutoMigrate=false
ENV Sso__Database__SeedOnStartup=false
ENV Sso__Signing__UseDevelopmentCertificates=false
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SSO.Web.Api.dll"]
