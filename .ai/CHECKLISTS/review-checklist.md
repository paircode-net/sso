# Review Checklist

## Arquitetura

- [ ] Dependências apontam na direção correta
- [ ] Controller sem regra de negócio
- [ ] **Command Handler só orquestra** (sem `if` de domínio, sem atribuição de Status/hash/ExpiresAt/normalize)
- [ ] Regras em Specifications + `*SpecificationsValidator`; atribuições no Domain Service
- [ ] Application não referencia Infrastructure.Data concreta
- [ ] Persistência via Reader/Writer / interfaces de Domain
- [ ] Sem padrão novo injustificado

## Código

- [ ] Naming alinhado ao playbook
- [ ] `sealed` onde é o padrão local
- [ ] Sem duplicação evitável
- [ ] Validações no lugar certo (Entity vs Domain specs) — **não no Command**
- [ ] Novas Specs/validators registradas em `AddValidationsConfigurations`
- [ ] Tratamento de erro consistente com handlers existentes

## API e contratos

- [ ] Rotas e verbos corretos
- [ ] Status codes adequados
- [ ] ModelWrapper keys/suppressed properties configurados
- [ ] Breaking changes documentados

## Testes

- [ ] Cenários cobrem happy path e regras críticas
- [ ] Integração usa rota do controller
- [ ] Testes determinísticos

## Segurança / ops

- [ ] Sem secrets
- [ ] Auth impact avaliado
- [ ] Migrations revisadas
- [ ] Logs adequados

## Docs

- [ ] `.ai/CONTEXT` sincronizado se o conhecimento mudou
- [ ] Decision/ADR se mudança arquitetural
