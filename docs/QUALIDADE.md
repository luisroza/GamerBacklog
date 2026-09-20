# Qualidade e validação

## Evidências de 10/09/2026

Na análise inicial, antes da criação destes documentos:

- SDK local 10.0.400; dotnet build concluiu com zero erros.
- NuGet emitiu NU1903 para SQLitePCLRaw.lib.e_sqlite3 2.1.11, classificado como alta severidade. É o resultado do restore daquela data, não uma auditoria completa.
- Compilador emitiu CS8602 em SteamClient.cs, no acesso a og.Name.Trim().
- Boot local aplicou migration e seed com sucesso.
- Smoke original passou páginas públicas e POST de login, mas falhou ao procurar Sair/Continuar Jogando em HTML inglês.
- Reexecutado substituindo esses textos somente em memória por Sign out/Continue Playing, passou login persistente, status, biblioteca, configurações, notificações, perfil e busca.
- EF registrou aviso de múltiplas coleções incluídas em uma consulta.
- Steam/RAWG reais, CRUD administrativo, cadastro e fluxos sociais mutáveis não foram testados nessa execução.

## Executar smoke existente

Em um terminal, inicie a aplicação com banco de teste separado, conforme desenvolvimento. Em outro:

```powershell
./scripts/smoke-test.ps1 -Base http://localhost:5099
```

O script versionado ainda contém o erro de idioma P-03. Para reproduzir a verificação da análise sem editar o arquivo:

```powershell
$smokeSource = Get-Content ./scripts/smoke-test.ps1 -Raw
$smokeSource = $smokeSource.Replace('Sair', 'Sign out').Replace('Continuar Jogando', 'Continue Playing')
& ([scriptblock]::Create($smokeSource)) -Base http://localhost:5099
```

O script altera o status de Elden Ring na conta demo para Played; não é somente leitura. O teste de busca imprime o resultado, mas não falha explicitamente quando não encontra o jogo. HTTP 200, isoladamente, também não comprova todos os resultados de negócio.

## Matriz para alterações futuras

| Área alterada | Verificação esperada |
|---|---|
| Contas | Cadastro válido/inválido, login errado/correto, logout, retorno externo rejeitado |
| Autorização | Visitante não altera dados; usuário A não altera biblioteca de B; usuário comum não acessa admin |
| Antiforgery | POST sem token ou token inválido rejeitado |
| Biblioteca | Adicionar, trocar cada status, remover; uma associação por jogo; nota/resenha preservadas |
| Avaliações | Limites 1/5, inválidos, edição, limpeza, média e resenha sem nota |
| Social | Follow/unfollow, self-follow, eventos e destinatário correto |
| Notificações | Isolamento por usuário e marcação de leitura |
| Admin | CRUD, validações, exclusão com dependências e atividades preservadas |
| Catálogo | Busca/filtros/paginação, lista vazia, imagem fallback e importação sem duplicação |
| Integrações | Casos listados em INTEGRACOES.md, incluindo erro parcial e idempotência |
| Banco | Criação do zero e migration sobre cópia de banco existente |
| Interface | Desktop/mobile, navegação por teclado, formulários e mensagens |

Não há projeto de testes unitários/integração nem CI no repositório-base. Ao corrigir regras com risco de regressão, criar testes que comprovem comportamento, preferindo SQLite real de teste para FKs/índices e HTTP controlado para APIs. Não fazer testes dependerem de credenciais pessoais.

## Critério de conclusão

Compilação passa; testes relevantes passam; falhas anteriores são distinguidas das novas; regras de autorização/persistência são preservadas; documentos afetados refletem a mudança. Para documentação isolada, conferir links locais, comandos e git diff --check é suficiente.
