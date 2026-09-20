# GamerBacklog

Catálogo social de jogos e biblioteca pessoal inspirado no Backloggd: organize jogos por status, avalie, escreva resenhas, siga pessoas e acompanhe atividades e conquistas.

MVP em C#/.NET 10, ASP.NET Core MVC/Razor, Identity, EF Core e SQLite. Interface em inglês e documentação de desenvolvimento em português.

## Estado atual do projeto

Revisão da documentação em **20/09/2026**, baseada no código local em `main`, commit `7cd3aa6`. O projeto está em **MVP para demonstração local**, com documentação de continuidade criada e pendências abertas antes de produção.

| Área | Estado |
|---|---|
| Aplicação local | Build, inicialização do SQLite e fluxos básicos verificados em 10/09/2026 |
| Testes | Smoke passou com correção de textos somente em memória; o script versionado ainda procura rótulos em português na interface inglesa |
| RAWG | Cliente implementado, mas sem chave na configuração versionada; chamadas reais ainda não validadas |
| Steam | Importação implementada e modo simulado disponível; fluxo real e possíveis problemas de vínculo/contrato ainda precisam de validação |
| PSN e Xbox | Placeholders, sem sincronização funcional |
| Documentação | Guias de produto, arquitetura, desenvolvimento, integrações, testes e operação disponíveis em `docs/` |
| Implantação | Nenhuma publicação concluída nesta sessão; hospedagem de produção ainda não definida |

A validação local de 10/09 cobriu páginas públicas, login persistente, alteração de status, biblioteca, configurações, notificações, perfil e busca. Não comprova chamadas externas, CRUD administrativo, carga ou prontidão de produção. Os testes não foram reexecutados nesta revisão documental.

**Próximas prioridades:** corrigir o smoke test, separar contas/seed demo de produção, resolver o alerta de dependência SQLite registrado no build, permitir limpar avaliações e corrigir/validar a sincronização Steam, incluindo a simulação após falha real. Consulte [pendências e critérios de aceite](docs/PENDENCIAS.md) e [evidências de qualidade](docs/QUALIDADE.md).

A publicação no endereço Sites solicitado não foi realizada: a aplicação ASP.NET Core/SQLite atual não é diretamente compatível com o ambiente Sites verificado na análise de implantação. Não foi escolhida nem executada uma migração de arquitetura.

## Executar

Na pasta que contém `GamerBacklog.csproj`, com SDK .NET 10 instalado:

```powershell
dotnet restore
dotnet build
dotnet run
```

Abra [localhost:5099](http://localhost:5099). O perfil local usa Development. O boot aplica migrations e cria dados demo automaticamente.

Conta demo: `demo@gamerbacklog.dev` / `Demo123!`. Há também `ana@gamerbacklog.dev` com a mesma senha. Use essas contas somente para demonstração local.

## Funcionalidades

- Cadastro, login, logout e perfis públicos em `/u/{username}`.
- Catálogo com busca, filtros, paginação, detalhes e avaliações da comunidade.
- Biblioteca: Want to Play, Playing, Played, Abandoned e Wishlist.
- Notas de 1–5 estrelas, resenhas, seguidores, feed e notificações internas.
- Importação RAWG sob demanda quando configurada.
- Código de importação Steam de biblioteca, horas e conquistas, com modo simulado.
- CRUD administrativo de jogos em `/admin_panel`, restrito ao papel Admin.
- Capas e avatares SVG gerados por endpoints locais.

PSN e Xbox são placeholders. `DemoIgdbClient` contém 24 jogos locais; não consulta IGDB. A integração Steam real precisa de correções e validação, descritas nas pendências.

## Documentação

Comece por [AGENTS.md](AGENTS.md) e pelo [índice de continuidade](docs/README.md).

| Documento | Conteúdo |
|---|---|
| [Estado atual](docs/ESTADO_ATUAL.md) | Histórico, evidências e ponto de retomada |
| [Produto](docs/PRODUTO.md) | Funcionalidades, regras e lacunas |
| [Arquitetura](docs/ARQUITETURA.md) | Componentes, fluxos e dados |
| [Desenvolvimento](docs/DESENVOLVIMENTO.md) | Setup, configuração e migrations |
| [Integrações](docs/INTEGRACOES.md) | RAWG, Steam e modos de falha |
| [Rotas](docs/ROTAS.md) | Endpoints e autorização |
| [Qualidade](docs/QUALIDADE.md) | Testes e critérios de validação |
| [Pendências](docs/PENDENCIAS.md) | Priorização e critérios de aceite |
| [Operação](docs/OPERACAO.md) | Diagnóstico, banco e preparação para produção |
| [Decisões](docs/DECISOES.md) | Escolhas observadas e questões abertas |

## Configuração opcional

As chaves `Rawg:ApiKey` e `Steam:ApiKey` estão vazias no arquivo versionado. Use variáveis de ambiente ou `appsettings.Development.json`, ignorado pelo Git; veja desenvolvimento.

Sem chaves, o backend usa catálogo demo e sincronização Steam simulada. Tailwind e fontes vêm de CDN, portanto o visual não é inteiramente independente de internet.

Não há implantação de produção documentada ou automatizada neste repositório. O seed atual cria contas demo/admin também fora de Development; isso deve ser corrigido antes de publicar.
