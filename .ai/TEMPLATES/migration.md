# Migration Plan — {{nome}}

## Objetivo

{{mudança de schema / dados}}

## Contexto EF

- DbContext: `{{DefaultDbContext}}`
- Projeto: `SSO.Infrastructures.Data`
- Startup project: `SSO.Web.Api`
- Output: `Default/Migrations` (ou {{ }})

## Comandos (referência do README)

```bash
cd src/SSO.Infrastructures.Data
dotnet ef --startup-project ../SSO.Web.Api migrations add {{Name}}DefaultDbContext -c DefaultDbContext -o Default/Migrations
```

## Tipo

- [ ] DDL (schema)
- [ ] DML (dados)
- [ ] Ambos (preferir migrations separadas, como o scaffold InitialDDL/InitialDML)

## Mudanças de modelo

| Entidade | Mudança |
|----------|---------|
| | |

## Mappings

- Arquivos `*Map` a atualizar:

## Compatibilidade

- Backward compatible? Sim/Não
- Requer downtime? **A definir**
- Estratégia produção (auto `UseMigrations` vs pipeline): **A definir**

## Rollback

{{como reverter / riscos}}

## Testes

- [ ] App sobe com InMemory/Test host
- [ ] Cenários de dados afetados
- [ ] Validar LocalDB/dev após migrate

## Checklist

- [ ] Snapshot atualizado
- [ ] Sem secrets em DML
- [ ] CONTEXT/modules atualizado se entidade nova
