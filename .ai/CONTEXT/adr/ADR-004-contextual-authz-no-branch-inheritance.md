# ADR-004 — Autorização contextual sem herança Branch no MVP

- Status: Aceito — **superseded parcialmente** por [ADR-008](ADR-008-branch-authz-inheritance-opt-in.md) quando `BranchAuthzInheritance=InheritFromAncestors` (default continua = este ADR)
- Data: 2026-07-14
- Decisores: Equipe SSO / feature 00001

## Contexto

A autorização deve considerar Usuário → Organização → Branch → Produto → Role → Claims → Permissões. Branches podem ter hierarquia estrutural (ex.: Matriz → Filial), mas herança automática de permissões na árvore aumenta ambiguidade e risco no MVP.

## Decisão

1. Autorização é sempre resolvida no **contexto ativo** (org + branch + product do token/cliente).
2. Assignments de Role/Claim/Permission são explícitos nesse contexto.
3. **MVP / default: sem herança automática** de permissões de Branch pai → filho (D4). Hierarquia de Branch pode existir como estrutura organizacional, sem gerar authz implícita.
4. **Herança via Role → Permission** permanece (papel concede conjunto de permissões).
5. Roles/claims de produto vivem no domínio SSO (não só Identity roles) — ver ADR-002.
6. **Evolução (00009 / ADR-008):** herança pai→filho é **opt-in** (flag Organization + `Inheritable` no assignment). Com flag Off, este ADR continua a regra efetiva.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| Herança pai→filho automática | Menos assignments manuais | Ambiguidade, surpresa de acesso, testes complexos |
| Flat org sem Branch | Simples | Viola requisito multi-branch |
| Permissions só via API (sem JWT) | Token enxuto | Round-trip; ver alternativa rejeitada no ADR-005 |

## Consequências

### Positivas

- Matriz de autorização previsível e testável.
- Evolução futura de herança Branch: **ADR-008** (opt-in; default Off).
- Clareza operacional: “o que está assigned na branch ativa” (default).

### Negativas / trade-offs

- Mais assignments explícitos em organizações com muitas filiais (mitigado por ADR-008 opt-in).
- Hierarquia Branch no MVP é principalmente organizacional, não de authz (default).

## Impacto no código

- Domain: motor de effective permissions filtrado por org/branch/product ativos.
- Sem walker de árvore de Branch na resolução **default**.
- Tests: garantir que permissão na Matriz **não** flui sozinha para Filial (flag Off).

## Referências

- Feature 00001 (D4, D5); feature 00009 / ADR-008
- ADR-002, ADR-003, ADR-005
- `CONTEXT/business.md`
)
