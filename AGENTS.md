# Orientações para desenvolvimento

Aplicam-se a todo o repositório. Instruções explícitas do usuário prevalecem. Não exigem confirmação adicional para trabalho já autorizado.

## Contexto inicial

1. Leia `docs/ESTADO_ATUAL.md` e `docs/PENDENCIAS.md`.
2. Consulte `docs/ARQUITETURA.md` e o documento da área afetada.
3. Verifique `git status` e o código atual. Documentação é referência datada, não substitui a implementação.

## Convenções

- Preserve MVC/Razor; não introduza outro framework sem necessidade do pedido.
- Identificadores e interface em inglês; documentação/comunicação em português, salvo orientação diferente.
- Use async em banco/rede e a injeção de dependências existente.
- Regras compartilhadas em Services; view models em Models; persistência em Data; entidades em Domain; APIs externas em Integrations.
- Siga o visual de `_Layout.cshtml`, `site.css` e componentes Razor compartilhados.
- Não altere números dos enums persistidos sem tratar compatibilidade dos dados.
- Mudanças no modelo exigem avaliar migration e snapshot. Não substitua migrations por EnsureCreated.
- Preserve alterações locais do usuário. Commit, push e implantação somente quando solicitados.

## Segurança e integridade

- Não versione chaves, tokens, bancos locais ou credenciais reais.
- Preserve autenticação, antiforgery nos POSTs e verificação de proprietário. Obtenha o usuário da sessão autenticada.
- Valide redirecionamentos como locais; mantenha o papel Admin no painel.
- Sincronizações devem preservar avaliações/resenhas/status manuais, tratar idempotência, falhas parciais e vínculos entre entidades.
- Não trate simulação como evidência de integração real. Informe o modo validado.

## Validação e entrega

- Para código: `dotnet build` e verificações proporcionais; consulte `docs/QUALIDADE.md`.
- Para documentação: conferir referências, comandos e `git diff --check`; não repetir testes de aplicação sem mudança de código.
- Testes mutáveis devem usar banco local descartável ou dados identificáveis. O smoke test altera a biblioteca demo.
- Atualize documentação afetada e pendências ao concluir mudanças relevantes.
- Relate resultado, validações, limitações e trabalho restante. Compilação não comprova integração real ou prontidão de produção.
