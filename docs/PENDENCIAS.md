# Pendências priorizadas

Todas abertas na base 7cd3aa6. Prioridades são proposta técnica: P0 antes de exposição pública, P1 para confiança funcional, P2 para evolução. Não autorizam executar migração, publicação ou novas funcionalidades por conta própria.

## P-01 — Seed e contas em produção (P0)

**Evidência de código:** DbSeeder cria demos e admin com senha padrão em qualquer ambiente, e promove usuário existente com o nome configurado. Program aceita senha simples de 6 caracteres, email não exclusivo; login não ativa lockoutOnFailure.

**Entrega sugerida:** separar seed demo do provisionamento administrativo, eliminar credenciais padrão de produção e revisar autenticação.

**Aceite:** boot de produção não cria demos/admin previsível nem promove conta arbitrária; provisionamento explícito funciona; login por email tem regra de unicidade consistente; testes cobrem acesso negado e tentativas inválidas. Tratar dados existentes antes de impor unicidade.

## P-02 — Dependência SQLite sinalizada (P0)

**Reproduzido:** build/restore em 10/09/2026 emitiu NU1903 para SQLitePCLRaw.lib.e_sqlite3 2.1.11. Referência emitida: GHSA-2m69-gcr7-jv3q.

**Entrega sugerida:** investigar advisory e grafo transitivo, escolher atualização compatível de pacotes e validar persistência. Não suprimir alerta sem resolução documentada.

**Aceite:** restore deixa de reportar essa vulnerabilidade; build, boot, migrations e smoke passam. Não presumir que isso elimina todas as vulnerabilidades.

## P-03 — Smoke desatualizado (P1)

**Reproduzido:** teste procura textos portugueses na interface inglesa e acusa login não persistido indevidamente.

**Aceite:** script original passa sem substituição em memória; busca sem resultado esperado falha; comprova sessão autenticada e resultados de negócio. Usa banco de teste e documenta efeitos.

## P-04 — Limpeza de avaliação/resenha (P1)

**Evidência de código:** LibraryService ignora rating null; UI envia vazio ao desmarcar estrelas. String vazia de resenha pode chegar como null pelo model binding; essa parte requer reprodução.

**Aceite:** limpar nota/resenha persiste; trocar somente status preserva ambos; média atualiza. Distinguir explicitamente não alterar de limpar; teste HTTP do formulário, não só chamada direta ao serviço.

## P-05 — Vínculo Game/UserGame na Steam (P1)

**Suspeita de leitura, não reproduzida com API:** usa game.Id antes do primeiro SaveChanges para Game novo, sem atribuir navegação Game ao UserGame.

**Aceite:** resposta controlada com vários jogos inéditos importa cada associação com FK válida; segunda sync não duplica; biblioteca anterior preservada. Corrigir tracking/relacionamento se a reprodução confirmar falha.

## P-06 — Contrato de conquistas Steam (P1)

**Requer validação externa:** DTO trata availableGameStats como lista direta. Conferir contrato oficial e envelope real antes de afirmar compatibilidade.

**Aceite:** fixtures fiéis de schema com/sem conquistas desserializam; conquistas/desbloqueios persistem corretamente; erro individual não mascara sync inteira. Raridade não é apresentada como estatística real quando for placeholder.

## P-07 — Simulação após falha real (P1)

**Evidência de código:** exceção na sync real cai em SimulateAsync, gravando desbloqueios fictícios nas tabelas normais.

**Aceite:** falha real retorna erro sem simular; demo é explícita/isolada; sync parcial pode ser retomada; UI mostra o modo correto. Definir tratamento dos dados simulados já existentes.

## P-08 — Importação RAWG e enriquecimento (P2)

**Evidência de código:** busca com resultado local não consulta RAWG; jogo Steam sem RawgId não hidrata; descrições vazias e buscas vazias podem repetir chamadas.

**Aceite:** política explícita de enriquecimento/retry/cache; fluxo que enriquece jogo local quando solicitado; nenhuma promessa de chamada única sem mecanismo que a garanta.

## P-09 — Concorrência e escala (P2)

**Evidência de código:** consultas carregam coleções completas; faltam índices únicos externos; sync usa saves/consultas em loops.

**Aceite:** cenários concorrentes não duplicam IDs externos/conquistas; paginar/filtrar no banco quando adequado; medir consultas; migration considera duplicatas existentes.

## P-10 — Base operacional e apresentação (P2; necessária antes de produção)

Sem CI/implantação versionada, backup automatizado/restauração comprovada ou gestão de Data Protection documentada no código. Tailwind/fontes dependem de CDN e rodapé informa sempre demo.

**Aceite:** definir alvo de hospedagem; build/teste automatizados; assets adequados ao ambiente; configuração de HTTPS/proxy/cookies; chaves persistentes de Data Protection; migração, backup e restore ensaiados. Nenhuma arquitetura de hospedagem foi escolhida ainda.

## Ordem sugerida

P-03 primeiro para recuperar um teste confiável; P-01/P-02 antes de publicar; P-04 a P-07 antes de considerar integrações e avaliações confiáveis; demais conforme objetivo e volume. Para produção, P-10 também é pré-requisito.
