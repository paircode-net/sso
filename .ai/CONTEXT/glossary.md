# Glossary

| Termo | Significado neste repositório |
|-------|------------------------------|
| **CQRS Full** | Separação de Commands, Queries e Notifications na Application, com Domain Services via MediatR |
| **BAYSOFT.Abstractions** | Pacote NuGet com bases (`DomainEntity`, `ApplicationRequestHandler`, Reader/Writer, etc.) |
| **Forge** | Metadados/codegen em `.forge/project.json` para entidades e convenções |
| **Context / Default** | Bounded context do scaffold Sample; schema EF `DefaultDb` |
| **Context / Identity** | Bounded context SSO; schema EF `IdentityDb` (ADR-006) — Fase 0 ativa |
| **Sample** | Aggregate de exemplo (não confundir com domínio SSO de produto); mantido até Fase 2 estável |
| **Application Command** | Caso de uso de escrita orquestrado na Application |
| **Application Query** | Caso de uso de leitura |
| **Notification** | Evento in-process pós-operação (`INotification`) |
| **Domain Service** | Caso de uso de domínio via MediatR (`*ServiceRequest`) |
| **Specification** | Regra de domínio consultável (ex.: unicidade) |
| **Reader / Writer** | Abstrações de persistência read/write sobre o DbContext |
| **ModelWrapper** | Biblioteca de wrap de request/response, filtros e tamanhos de coleção |
| **Middleware (projeto)** | Composition root de DI — não necessariamente middleware HTTP custom |
| **ResourceController** | Controller base da API que expõe `Send` para MediatR |
| **Scenarios** | Convenção de classes de teste MSTest |
| **SSO** | Plataforma de Single Sign-On deste repositório (épico 00001) |
| **Organization** | Tenant; isolamento lógico de identidade e autorização |
| **Branch** | Filial / unidade dentro de uma Organization |
| **Product** | Sistema do ecossistema que consome o SSO (conceito de negócio) |
| **AuthClient** | Cliente OAuth/OIDC registrado, ligado a um Product (≠ Product) |
| **Membership** | Vínculo usuário ↔ organização (e opcionalmente branch) |
| **Permission** | Capacidade autorizável dinâmica |
| **Claim tipada** | Atributo de domínio (`ClaimDefinition` → JWT `sso_c_{code}`); não substitui Permission para gate de rota |
| **Effective permissions** | Conjunto resolvido no contexto User×Org×Branch×Product; embutido no access token (ADR-005) |
| **claim_ver** | Etag opaco do catálogo/atribuições de claims tipadas (00008); separado de `perm_ver` |
| **User** | Entidade de identidade do domínio SSO (estende ASP.NET Identity; não usar o nome `ApplicationUser`) |
| **switch-context** | Fluxo que define org/branch ativos e emite token com `organization_id`/`branch_id` e permissions efetivas |
| **organization_id** | Claim JWT do tenant ativo (não usar `org_id`) |
| **Authorization Server** | Este serviço (OpenIddict) emissor de tokens |
