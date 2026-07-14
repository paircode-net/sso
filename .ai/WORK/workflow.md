# Workflow padrão

Fluxo obrigatório para tarefas de desenvolvimento assistido por IA ou humano.

```text
Descoberta
    ↓
Planejamento
    ↓
Validação
    ↓
Implementação
    ↓
Testes
    ↓
Revisão
    ↓
Documentação
    ↓
Merge
```

## 1. Descoberta

- Compreender o pedido e o resultado esperado.
- Consultar `AI_MANIFEST.md`, PLAYBOOK e CONTEXT (nesta ordem após o manifest).
- Localizar módulos, rotas, handlers e testes afetados.
- Identificar ambiguidades de negócio — **parar e perguntar** se necessário.

## 2. Planejamento

- Tarefa pequena: confirmar abordagem em bullets.
- Tarefa média/grande: criar arquivo em `.ai/WORK/` a partir de `.ai/TEMPLATES/`.
- Incluir arquivos impactados, riscos e testes.

## 3. Validação

- Conferir alinhamento com architecture/coding-style/security.
- Confirmar se migration/auth/breaking change estão cobertos.
- Obter alinhamento do usuário quando houver opções equivalentes.

## 4. Implementação

- Seguir camadas e naming do repositório.
- Preferir o aggregate Sample como referência estrutural.
- Não expandir escopo.
- Não modificar código de produção quando a tarefa for só documentação (como o bootstrap AI).

## 5. Testes

- Unitários e/ou integração conforme o playbook.
- Garantir rotas HTTP corretas.
- Executar a suíte relevante quando o ambiente permitir.

## 6. Revisão

- Usar `.ai/CHECKLISTS/review-checklist.md` (+ security/performance se aplicável).
- Corrigir problemas no escopo.

## 7. Documentação

- Atualizar CONTEXT quando o fato do sistema mudar.
- Registrar decisões relevantes.
- Manter SYSTEM_PROMPT livre de domínio.

## 8. Merge

- Diff revisável; mensagem clara.
- PR com resumo e plano de teste (quando aplicável).
- Não mergear com riscos de segurança não aceitos (ex.: expor API sensível sem auth).

## Artefatos em `.ai/WORK/`

| Tipo | Destino |
|------|---------|
| Planos ativos | `.ai/WORK/yyyy-mm-dd-slug.md` |
| Workflow (este arquivo) | permanente |
| Planos concluídos | manter ou arquivar conforme a equipe decidir (**A definir** política de retenção) |
