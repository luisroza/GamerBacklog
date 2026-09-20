# Integrações

Descrição baseada no código, não em chamadas reais validadas. Contratos externos devem ser conferidos na documentação oficial e em respostas representativas ao implementar correções. Não presumir quotas ou disponibilidade comercial a partir de comentários antigos.

## RAWG

Arquivos: Integrations/RawgClient.cs, ICatalogClient.cs, Controllers/GamesController.cs e Data/DbSeeder.cs.

- Configuração: Rawg:ApiKey. HttpClient nomeado rawg, base https://api.rawg.io/api/, timeout de 30 segundos.
- Boot: solicita até 40 jogos populares, ordenados por Metacritic, se o banco estiver vazio. Falha/lista vazia usa catálogo demo.
- Busca: somente quando nenhum nome local corresponde ao termo, antes de aplicar filtros de gênero/plataforma, importa até 6 resultados.
- Deduplificação: RawgId, depois nome com trim/lowercase. Preenche lacunas de metadados, não atualiza todos os campos existentes.
- Detalhes: jogo com RawgId e Summary vazia pode receber descrição/desenvolvedor ao abrir a página. Descrição é limitada a aproximadamente 1200 caracteres.
- Falhas: capturadas e registradas; retornam resultado vazio/false.

A promessa antiga de nunca repetir uma chamada não é garantida: buscas sem resultados podem repetir, descrição vazia pode ser consultada novamente e requisições concorrentes não têm coordenação. Não existe cache negativo nem marcador explícito de hidratação concluída.

Jogos importados da Steam sem RawgId não são enriquecidos automaticamente apenas ao abrir detalhes. Uma busca que já encontra o jogo local também não aciona RAWG. Não existe botão/ação atual Sync catalog now.

## Steam

Arquivo principal: Integrations/SteamClient.cs; configuração e acionamento em SettingsController.

- Requer Steam:ApiKey e SteamId salvo no usuário para tentar modo real.
- Consulta GetOwnedGames; associa por SteamAppId, depois nome normalizado; tenta criar Game e UserGame para títulos desconhecidos.
- Horas são minutos/60 em divisão inteira. Em registros existentes, só aumenta/preenche horas; preserva status, nota e resenha.
- Novos registros: sem minutos -> Backlog; atividade nos últimos 14 dias -> Playing; demais -> Played.
- Conquistas: considera até 15 jogos da biblioteca com SteamAppId por horas; consulta GetSchemaForGame e GetPlayerAchievements.
- ExternalId de conquista combina appid e nome da API. Desbloqueios são adicionados se ausentes; o código não reconcilia remoções.
- Raridade importada é fixa em 50, não estatística real. Eventos de conquistas ficam limitados a até 3 jogos por sync real.
- LastSteamSyncAt é atualizado ao concluir; não há job agendado.

Se não houver chave/SteamID, ou se a tentativa real lançar exceção capturada pelo wrapper, executa SimulateAsync. A simulação escolhe alvo de 30–90% de conquistas por jogo e grava desbloqueios no mesmo banco. Ela também atualiza LastSteamSyncAt e gera atividades via Steam. O resultado informa modo simulado, mas os registros não guardam proveniência.

## Pontos de revisão da Steam

1. Game novo é adicionado e seu ID CLR usado no UserGame antes de SaveChanges, sem atribuir navegação Game. Suspeita de vínculo com ID 0/erro de FK; reproduzir com catálogo vazio e resposta controlada.
2. SchemaGame.AvailableGameStats está tipado diretamente como lista. Conferir o envelope real de availableGameStats e suas conquistas contra contrato oficial; ainda não validado com credenciais.
3. Falha real pode produzir conquistas fictícias; separar demo de erro operacional.
4. Saves múltiplos permitem estado parcial; testar retomada e reexecução sem duplicatas.
5. SteamID manual não prova titularidade; validar formato e decidir fluxo de vinculação antes de uso público.

## Outros clientes

DemoIgdbClient é catálogo embutido; os IgdbIds são locais, não IDs verificados no provedor. PSN e Xbox lançam NotImplementedException; botões estão desabilitados.

## Critérios para considerar integração validada

- Resposta real ou fixture fiel ao contrato oficial; sem chaves em logs/fixtures.
- Caminhos vazio, privado/indisponível, timeout, resposta inválida e falha parcial cobertos.
- Segunda execução não duplica jogos ou desbloqueios.
- Status/nota/resenha manuais preservados.
- Modo demo inequívoco e sem contaminar resultados reais.
- Resultado informa corretamente importados, atualizados, ignorados e falhas relevantes.
