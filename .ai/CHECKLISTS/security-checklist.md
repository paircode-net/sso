# Security Checklist

## Entrada e validação

- [ ] Inputs validados (FluentValidation / specs / ModelWrapper)
- [ ] IDs e filtros não permitem acesso indevido a dados de outros tenants (**modelo de tenancy A definir**)
- [ ] Sem mass assignment perigoso (suppressed properties / keys configuradas)

## Autenticação / autorização

- [ ] Necessidade de auth avaliada para o endpoint
- [ ] Se auth existir: scheme configurado + `UseAuthentication`/`UseAuthorization` ativos
- [ ] `[Authorize]` / políticas aplicadas onde necessário
- [ ] Estado atual do scaffold (sem auth) explicitamente aceito pelo dono do risco

## Segredos e configuração

- [ ] Nenhum secret no código ou appsettings commitado
- [ ] Connection strings de produção fora do repositório
- [ ] Pacotes novos revisados quanto a supply chain básica

## Dados e logs

- [ ] Logs sem senhas/tokens/PII desnecessária
- [ ] Notifications não publicam dados sensíveis em texto claro
- [ ] Erros não vazam stack interna ao cliente em produção

## Persistência

- [ ] Queries parametrizadas (EF por padrão)
- [ ] Migrations DML sem dados sensíveis reais
- [ ] Principio do menor privilégio na connection string de runtime (**A definir** ops)

## Dependências / superfície

- [ ] Swagger apenas onde apropriado
- [ ] HTTPS redirection mantida salvo exceção justificada
- [ ] Integrações externas com credenciais e timeouts pensados
