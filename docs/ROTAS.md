# Rotas e acesso

Rotas de interface MVC; não são um contrato de API JSON. A tabela mostra URLs usuais. A rota convencional é {controller=Home}/{action=Index}/{id?}. Ações convencionais de leitura sem HttpGet explícito não têm restrição de verbo pelo atributo.

| Verbo usual | URL | Acesso | Entrada/efeito |
|---|---|---|---|
| GET | / | Público | Landing ou home personalizada |
| GET | /Home/Error | Público | Tela de erro |
| GET | /Games | Público | q, genre, platform, page; busca pode importar dados |
| GET | /Games/{id} | Público | Detalhes; pode hidratar via RAWG |
| POST | /Games/{id}/SetStatus | Autenticado | status, returnUrl; valida enum e jogo |
| POST | /Games/{id}/SaveReview | Autenticado | rating, review; nota 1–5 |
| GET | /Library | Autenticado | status |
| POST | /Library/UpdateStatus | Proprietário | id do UserGame, status, back |
| POST | /Library/Remove | Proprietário | id do UserGame, back |
| GET | /u/{username} | Público | Perfil |
| POST | /u/{username}/Follow | Autenticado | Alterna relacionamento; returnUrl local |
| GET | /Notifications | Autenticado | Até 30 notificações próprias |
| POST | /Notifications/MarkAllRead | Autenticado | Marca todas as próprias |
| GET | /Settings/Integrations | Autenticado | Configuração/status das integrações |
| POST | /Settings/SaveSteam | Autenticado | steamId; vazio remove |
| POST | /Settings/SyncSteam | Autenticado | Executa sync real/simulado |
| GET, POST | /Identity/Account/Login | Público | Login, Password, RememberMe, ReturnUrl |
| GET, POST | /Identity/Account/Register | Público | Username, Email, Password, ConfirmPassword, DisplayName |
| POST | /Identity/Account/Logout | Sessão atual | Encerra sessão; sem Authorize explícito |
| GET | /covers/{slug} | Público | SVG; slug aceita segmentos |
| GET | /avatar/{username} | Público | SVG |
| GET | /admin_panel | Admin | search, page e métricas |
| GET, POST | /admin_panel/create | Admin | Formulário de jogo |
| GET, POST | /admin_panel/edit/{id} | Admin | Edição por ID da rota |
| GET | /admin_panel/delete/{id} | Admin | Confirmação e impacto |
| POST | /admin_panel/delete/{id} | Admin | Exclusão em cascata |

Todos os POSTs listados usam ValidateAntiForgeryToken. Formulários Razor geram o token. LibraryController verifica proprietário, e os outros fluxos pessoais usam o usuário autenticado. Cookies redirecionam login/acesso negado para /Identity/Account/Login.

AdminGameFormViewModel valida nome, limites de texto, ano, Metacritic e URL de imagem; o controller limita ano ao ano corrente + 2. Consultar o modelo para limites exatos antes de alterar formulários.

Pontos a considerar: GETs do catálogo têm efeitos de persistência; busca pública pode consumir API; não há rate limiting explícito; redirecionamento de acesso negado pode ser confuso para usuário já autenticado.
