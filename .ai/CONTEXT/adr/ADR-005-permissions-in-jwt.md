# ADR-005 — Permissões efetivas no JWT

- Status: Aceito
- Data: 2026-07-14
- Decisores: Equipe SSO / feature 00001
- Supersedes: revisão da decisão anterior “JWT mínimo + API effective-permissions” (mesmo ADR-005)

## Contexto

Clients de produto precisam autorizar localmente com baixa latência. Uma API separada de `effective-permissions` adiciona round-trips e complexidade de cache. A equipe prefere que o access token carregue o conjunto de permissões efetivas do contexto ativo, aceitando o trade-off de tamanho e frescor do token.

## Decisão

1. O access token inclui as **permissões efetivas** do contexto ativo (User × Organization × Branch × Product), além das claims OIDC padrão e de contexto (`organization_id`, `branch_id`, identificação de product/client).
2. A resolução (motor de authz / Role→Permission) ocorre **na emissão** do token (login, refresh e **switch-context**).
3. Products autorizam preferencialmente com base nas claims de permission do JWT (sem round-trip obrigatório ao SSO por request).
4. Mitigações obrigatórias ao desenho:
   - **TTL curto** do access token + refresh bem definido;
   - claim de versão/etag de policy (ex.: `perm_ver`) quando útil para invalidação/cache;
   - revogação de refresh/sessão quando roles/permissions críticas mudarem;
   - monitorar tamanho do token; se exceder limites práticos de header, reabrir ADR.
5. API de consulta de permissões pode existir para **administração/diagnóstico**, mas **não** é o mecanismo primário de authz dos products no MVP.

Formato da claim (nome final na implementação): lista compacta de códigos de permission (ex.: claim `permissions` ou `perm`), estável e documentada para os times de produto.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| JWT mínimo + API effective-permissions | Token enxuto; frescor sob demanda | Round-trip/cache em todo client; mais complexidade operacional |
| Só introspection a cada request | Sempre fresco | Latência e carga no Authorization Server |
| Permissions só em cookies server-side | Controle central | Ruim para APIs multi-client / SPAs |

## Consequências

### Positivas

- Authz local nos products sem dependência de API de policy por request.
- Contrato simples para times de produto (ler claims do token).
- Consistente com switch-context: novo token = novo conjunto de permissions.

### Negativas / trade-offs

- Tokens maiores; risco de limites de header em ambientes restritivos.
- Permissions podem ficar **stale** até expirar o access token ou novo switch-context/refresh com re-resolução.
- Mudanças de role/permission exigem disciplina de TTL, revogação e reemissão.

## Impacto no código

- OpenIddict: customização de claims na emissão (incluir permissions efetivas + contexto).
- Domain/Application: motor de resolução invocado no pipeline de token/switch-context.
- Products: policies/handlers baseados em claims de permission.
- Testes: tamanho do token, matriz de permissions por contexto, comportamento pós-switch-context.
- Documentação de integração: claim names, TTL e expectativa de stale window.

## Referências

- Feature 00001 (F00001-D3 revisado)
- ADR-001, ADR-003, ADR-004
- Plano: `.ai/WORK/2026-07-14-00001-plataforma-sso.md`
