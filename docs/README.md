# Documentação de continuidade

Base: commit `7cd3aa6`, analisado localmente em 10/09/2026. Documentos distinguem implementação observada, problemas reproduzidos, suspeitas e propostas.

## Ordem de leitura

1. [Estado atual](ESTADO_ATUAL.md): ponto de retomada e evidências.
2. [Produto](PRODUTO.md) e [arquitetura](ARQUITETURA.md): funcionamento e organização.
3. [Desenvolvimento](DESENVOLVIMENTO.md): ambiente e banco.
4. [Pendências](PENDENCIAS.md): selecionar entrega verificável.
5. [Qualidade](QUALIDADE.md): validar a entrega.

Referências: [integrações](INTEGRACOES.md), [rotas](ROTAS.md), [operação](OPERACAO.md), [decisões](DECISOES.md) e [AGENTS.md](../AGENTS.md).

## Manutenção

- Mudança funcional: atualizar produto, rotas e pendências afetadas.
- Persistência: atualizar arquitetura, migrations e operação.
- Integrações: atualizar comportamento, modos de falha e evidências.
- Decisão estrutural: registrar contexto, escolha e consequências em decisões.
- Etapa concluída: atualizar estado atual com referência Git e validações; não registrar segredos ou logs extensos.

Prioridades são sugestões técnicas, não roadmap aprovado. Não marque propostas como decisões tomadas ou inferências como bugs reproduzidos.
