# Ambiente de desenvolvimento

## Pré-requisitos e execução

SDK .NET 10, Git, acesso ao NuGet e PowerShell para o smoke test. Não há global.json fixando SDK, nem pipeline npm/Tailwind local.

Na raiz do repositório:

```powershell
dotnet restore
dotnet build
dotnet run
```

O launch profile usa Development e http://localhost:5099. Para ambiente/porta explícitos:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --no-launch-profile --urls http://127.0.0.1:5099
```

Finalize com Ctrl+C. Variáveis de ambiente permanecem naquela sessão até serem removidas.

## Configuração

| Chave | Uso atual |
|---|---|
| ConnectionStrings:DefaultConnection | SQLite; padrão Data Source=gamerbacklog.db |
| Rawg:ApiKey | Seleciona RAWG como catálogo; vazio usa demo |
| Steam:ApiKey | Habilita tentativa de sync real |
| Steam:DefaultSteamId | Presente no JSON, sem uso no fluxo atual |
| Admin:Username | Nome da conta criada/promovida pelo seed; padrão admin |
| Admin:Password | Senha usada somente ao criar admin inexistente |

Use appsettings.Development.json local (ignorado pelo Git) ou variáveis Rawg__ApiKey, Steam__ApiKey e ConnectionStrings__DefaultConnection. Não copie segredos para documentação ou logs.

Alterar Admin:Password não troca a senha de conta existente. O seed pode promover conta existente com o username configurado; não use isso como administração de produção.

## Banco local

A primeira execução cria/popula o SQLite. O diretório de trabalho determina o caminho se a connection string for relativa. Arquivos .db, .db-wal e .db-shm são ignorados pelo Git.

Prefira banco separado para testes:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Data Source=gamerbacklog-test.db'
dotnet run
```

Após encerrar a aplicação:

```powershell
Remove-Item Env:ConnectionStrings__DefaultConnection
```

Não apague o banco de trabalho para contornar migration. Para recriar dados demo, use caminho novo e preserve o anterior até não ser necessário.

## EF e migrations

O manifesto dotnet-tools.json está na raiz e declara dotnet-ef 10.0.11. Restauração e resolução do comando foram verificadas em 10/09/2026:

```powershell
dotnet tool restore --tool-manifest ./dotnet-tools.json
dotnet tool run dotnet-ef -- --version
dotnet tool run dotnet-ef -- migrations list --project ./GamerBacklog.csproj
```

Para mudança de modelo, substitua NomeDaAlteracao:

```powershell
dotnet tool run dotnet-ef -- migrations add NomeDaAlteracao --project ./GamerBacklog.csproj
```

Revise migration/snapshot, principalmente exclusões e conversões. Valide banco vazio e atualização de banco existente. O próximo boot aplica migrations automaticamente. A ferramenta 10.0.11 difere dos pacotes EF 10.0.0; reavalie compatibilidade ao atualizar dependências.

## Fluxo de entrega

1. Inspecionar Git e definir comportamento esperado.
2. Consultar pendências; usar branch codex/descricao quando necessário.
3. Implementar mudança coerente, incluindo migration quando aplicável.
4. Validar e atualizar os documentos afetados.
5. Revisar diff e registrar limitações. Commit/push somente quando solicitados.
