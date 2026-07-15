# ADR-003 — Isolamento multi-tenant e switch-context

- Status: Aceito
- Data: 2026-07-14
- Decisores: Equipe SSO / feature 00001

## Contexto

Usuários podem pertencer a múltiplas Organizations e Branches. É necessário isolamento lógico de dados e um contrato explícito de **contexto ativo** para autorização, sem confiar em headers mutáveis como fonte de verdade.

## Decisão

1. **Organization** é o tenant: isolamento lógico obrigatório em queries/comandos sensíveis (`OrganizationId`).
2. Contexto ativo (Organization / Branch) é estabelecido via endpoint **`switch-context`**, que emite novo token (ou equivalente OpenIddict) contendo claims `organization_id` e `branch_id`.
3. **Headers não são fonte de verdade** para tenant/branch; APIs leem o contexto das claims do access token.
4. Product no runtime deriva principalmente de `client_id` / audience / scopes do cliente OAuth.

```text
Login → token (contexto mínimo)
  → switch-context → token com organization_id / branch_id
  → APIs autorizadas nesse contexto
```

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| Header `X-Organization-Id` | Simples para APIs | Fácil spoof se não houver binding forte ao token |
| Uma org fixa por usuário | Simples | Viola multi-org da definição |
| Só cookie de sessão server-side | Controle central | Pior para APIs/SPA e clients M2M |

## Consequências

### Positivas

- Contexto auditável e amarrado ao token.
- Modelo alinhado a OAuth (claims) e a multi-membership.
- Testes de isolamento claros (token com org A não acessa org B).

### Negativas / trade-offs

- Clientes precisam chamar switch-context (ou fluxo equivalente) ao mudar org/branch.
- Tokens curtos + refresh precisam ser desenhados com rotação de contexto em mente.

## Impacto no código

- Application: comando/handler switch-context; validação de Membership.
- OpenIddict: claims custom no token.
- Data: filtros por `OrganizationId` nas queries Identity.
- Tests: matriz multi-org / switch-context.

## Referências

- Feature 00001 (D2)
- ADR-004, ADR-005
- `CONTEXT/business.md`
)
