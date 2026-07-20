# Playbook — Engineering

## Propósito

Definir como a equipe engaja mudanças no código: qualidade, escopo e disciplina de entrega.

## Princípios

- Compreender o contexto antes de alterar.
- Preferir alterações pequenas e revisáveis.
- Qualidade acima de velocidade.
- Reutilizar antes de abstrair.
- Documentar decisões e aprendizado.

## Boas práticas

- Seguir o fluxo em `.ai/WORK/workflow.md`.
- Localizar o aggregate/módulo afetado antes de criar arquivos.
- Manter dependências apontando para dentro (API → Middleware → Application/Domain ← Infrastructure).
- Extrair só quando houver duplicação real ou clareza clara de responsabilidade.
- Manter localização (`resx` / `IStringLocalizer`) quando a mensagem for voltada ao usuário.

## Padrões obrigatórios

- Não inventar regras de negócio.
- Não alterar comportamento sem intenção explícita.
- Novos fluxos de aplicação usam MediatR (commands/queries/notifications/domain services).
- Controllers apenas delegam via `Send` / Mediator — sem regra de negócio.
- **Commands orquestram; Domain Services + Specs possuem regras e atribuições de domínio** (ver `architecture.md` § Command vs Domain Service).
- Persistência via interfaces do Domain (`I*DbContextReader` / `I*DbContextWriter`), não via DbContext concreto na Application.
- Classes de domínio/application preferencialmente `sealed` (convenção Forge).
- Novas Specs/validators: registrar em `SSO.Middleware/AddServices/AddValidationsConfigurations.cs`.

## Exemplos

```text
OK: PostSampleCommandHandler valida request → chama CreateSampleService → CommitAsync → Publish notification
OK: CreateOrganizationInviteService atribui TokenHash/Status/ExpiresAt → ValidateDomain → AddAsync
OK: AcceptOrganizationInviteService + Specs (NotPending, Expired, EmailMismatch)
OK: PatchAcceptOrganizationInviteCommand → AcceptOrganizationInviteService (Command = HTTP Patch; Service = verbo de domínio)
OK: SamplesController recebe HTTP e chama Send(request)
NÃO: Controller acessa DbContext e altera entidade
NÃO: Application referencia SSO.Infrastructures.Data diretamente
NÃO: CommandHandler faz if (invite.Status != Pending) ou data.ExpiresAt = UtcNow.AddDays(7)
NÃO: CommandHandler cria Membership / altera Status sem passar por Domain Service
```

## Anti-patterns

- “Quick fix” que pula camada de Domain Services.
- Colocar validação/atribuição de domínio no Command “só desta vez”.
- Duplicar validação sem reutilizar Specifications / FluentValidation.
- Criar serviços genéricos “Utils” sem dono claro.
- Expandir escopo além do pedido (“enquanto estou aqui...”).
