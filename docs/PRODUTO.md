# Produto e regras atuais

## Objetivo e jornadas

Biblioteca social para organizar jogos, compartilhar avaliações e acompanhar amigos. A experiência atual é de demonstração, com interface em inglês e dados fictícios associados a jogos reais.

| Área | Comportamento |
|---|---|
| Visitante | Apresentação, catálogo, detalhes e perfis públicos |
| Conta | Cadastro com username/email/senha/nome; login por username ou email; logout |
| Catálogo | Busca por nome, filtros de gênero/plataforma, 20 jogos por página |
| Jogo | Metadados, Metacritic, média comunitária, resenhas e conquistas |
| Biblioteca | Adiciona por status ou avaliação; filtra, atualiza e remove |
| Perfil | Bio/avatar quando presentes, prateleiras e contagens sociais |
| Social | Seguir/deixar de seguir; impede seguir a si mesmo |
| Feed | Até 20 atividades próprias e de pessoas seguidas |
| Notificações | Menu com até 8; página com até 30; marcar todas como lidas |
| Administração | Métricas, busca e CRUD de jogos; 25 registros por página |

## Biblioteca e avaliações

Valores persistidos de PlayStatus: Backlog=1, Playing=2, Played=3, Abandoned=4, Wishlist=5. Backlog aparece como Want to Play. Played significa jogado, não necessariamente concluído.

Existe um único UserGame por usuário/jogo. Uma primeira avaliação cria essa associação em Backlog. Notas são inteiros de 1 a 5; a média considera somente notas não nulas. Resenhas aparecem por UpdatedAt decrescente.

LibraryService.SaveAsync aplica somente campos não nulos. Isso preserva nota/resenha ao trocar status, mas impede limpar nota com null. Limpar resenha pelo formulário também precisa de teste porque o model binding pode converter string vazia em null.

Salvar resenha não vazia gera atividade e notifica outros usuários que possuem o jogo. Trocar status gera atividade. Remover da biblioteca não apaga automaticamente atividades ou conquistas pessoais.

Trending seleciona até 10 jogos pela média local, com desempate por ID. Não mede tendência temporal.

## Integração e administração

SteamID é informado manualmente, sem autenticação Steam para comprovar titularidade. Sincronização ocorre dentro da requisição POST; veja [integrações](INTEGRACOES.md).

O painel gerencia jogos, não contas ou moderação. Excluir jogo remove entradas de bibliotecas e conquistas em cascata; atividades mantêm o texto com GameId nulo.

## Lacunas

- Sem edição de perfil, upload de avatar, recuperação de senha ou fluxo de confirmação de email.
- Favorite existe na entidade, sem fluxo de gerenciamento na interface.
- PSN/Xbox não sincronizam.
- Sem mensagens privadas, listas personalizadas, moderação ou configuração de privacidade.
- Sem distinção persistida entre conquistas simuladas e reais.

Lacunas descrevem o estado; não representam novas funcionalidades aprovadas.
