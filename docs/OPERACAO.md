# Operação e preparação para produção

## Estado atual

Somente execução local foi verificada. Não há Dockerfile, compose, workflow CI ou configuração de hospedagem versionados na base analisada. Não existe decisão de provedor/domínio neste projeto.

## Inicialização e diagnóstico

O processo inicia SQLite, aplica migrations e executa seed antes de atender. Falhas de migration/seed impedem a inicialização. HTTP é configurado pelo launch profile ou --urls; a aplicação não habilita explicitamente redirecionamento HTTPS/HSTS no Program.cs atual.

Em erro, verificar saída do processo, ambiente, connection string e permissões de escrita. Não publicar logs de requests externos sem remover chaves: URLs dos clientes podem conter API keys.

| Sintoma | Verificar |
|---|---|
| Porta 5099 ocupada | Processo existente ou usar outra porta |
| Catálogo demo sem RAWG | Chave, ambiente carregado e se o banco já tinha jogos |
| Steam simulado inesperadamente | Chave, SteamID e logs da falha real |
| Banco bloqueado | Outros processos/escritas e localização do SQLite |
| Admin sem acesso | Papel da conta e configuração; senha não muda no seed de conta existente |
| Visual incompleto offline | Dependência de CDN Tailwind/fontes |
| Smoke acusa login | Erro de idioma P-03 |

Não há health endpoint dedicado. GET / é somente uma checagem básica de resposta, não prova integridade das integrações.

## Banco e backup

Banco relativo ao diretório de execução; mantenha dados e backups fora de locais compartilhados publicamente. A cópia local atual está sob OneDrive: não considerar sincronização de arquivos um backup consistente de SQLite ativo.

Para backup local simples, encerrar todas as instâncias e usar ferramenta de backup SQLite consistente ou confirmar checkpoint/fechamento limpo antes de copiar o banco. Não copiar apenas .db enquanto pode haver transações no WAL. Não remover arquivos -wal/-shm para tentar resolver bloqueios.

Validar restauração em caminho separado: iniciar com connection string da cópia, conferir contas/jogos/bibliotecas e migration, sem sobrescrever a base original. Registrar data e resultado. Nenhum restore foi ensaiado nesta análise.

Rollback de código pode não ser compatível com schema novo. Avaliar migration reversa e backup antes da mudança; não presumir que checkout resolve alteração de dados.

## Requisitos a definir antes de publicar

- Corrigir contas/seed, dependência sinalizada e modos de simulação.
- Definir provedor, volume persistente, domínio, HTTPS e confiança em proxy conforme ambiente real.
- Persistir/proteger chaves Data Protection para estabilidade de cookies entre reinícios/réplicas.
- Administrar segredos fora do Git e sanitizar logs.
- Executar migrations de forma controlada, com backup e recuperação ensaiada; evitar múltiplas instâncias disputando bootstrap.
- Definir política de backup, retenção e objetivos de recuperação.
- Definir monitoramento, limites de sincronização e tratamento de abuso de login/busca pública.
- Revisar privacidade dos perfis e titularidade da vinculação Steam.

Esta lista é planejamento de implementação, não evidência de controles existentes ou autorização de implantação.
