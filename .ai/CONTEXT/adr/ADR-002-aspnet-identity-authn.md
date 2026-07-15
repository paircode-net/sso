# ADR-002 — ASP.NET Identity para autenticação

- Status: Aceito
- Data: 2026-07-14
- Decisores: Equipe SSO / feature 00001

## Contexto

É necessário gerenciar usuários, credenciais, confirmação de e-mail, reset de senha, 2FA e lockout de forma familiar ao ecossistema .NET, sem acoplar o modelo de autorização multi-produto ao papel nativo simples do Identity.

## Decisão

Usar **ASP.NET Identity** (EF Core) para **AuthN**: usuários, senhas, tokens de conta, 2FA e mecanismos padrão de segurança de conta.

**Roles/claims de produto e autorização contextual** ficam no **domínio SSO** (assignments por Organization / Branch / Product), não como único mecanismo de authz via `IdentityRole` global (D5).

Identity store no `IdentityDbContext` (ADR-006).

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| Somente tabelas custom de usuário | Controle fino | Reinventar hash, 2FA, lockout, recovery |
| Roles Identity como único modelo authz | Simples | Insuficiente para Org×Branch×Product |
| Auth 100% externa (sem store local) | Menos persistence | Não atende cadastro/credenciais locais da definição |

## Consequências

### Positivas

- Fluxos de conta maduros e bem suportados.
- Separação clara AuthN (Identity) vs AuthZ contextual (domínio).
- Compatibilidade com OpenIddict (padrões conhecidos).

### Negativas / trade-offs

- Dois “mundos” de roles (Identity opcional vs domínio SSO) — documentar e evitar duplicação sem propósito.
- Entidade de domínio nomeada **`User`** (estende `IdentityUser`); evitar o nome `ApplicationUser`.
- Extensão de `IdentityUser`/`IdentityRole` precisa de disciplina de mapping.

## Impacto no código

- Projetos: Domain (portas), Data (Identity maps), Middleware (`AddIdentity`), Application (comandos de conta).
- Migrações: schema Identity em `IdentityDb`.
- UI: fluxos de conta integrados com Razor login (D6) nas fases 2/4.

## Referências

- Feature 00001 (D5, D12)
- ADR-001, ADR-004, ADR-006
- `CONTEXT/business.md`
)
