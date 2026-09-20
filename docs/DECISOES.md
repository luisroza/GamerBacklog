# Decisões e questões abertas

Registro inicial em 10/09/2026. As escolhas abaixo foram observadas no código; não há histórico suficiente para atribuir suas motivações ao autor. Consequências são avaliação técnica.

| ID | Escolha observada | Consequência |
|---|---|---|
| D-01 | Aplicação única MVC/Razor em .NET 10 | Um build/processo; requisições também executam sync |
| D-02 | SQLite com EF Core e migrations | Setup simples; exige cuidado com escrita concorrente e volume persistente |
| D-03 | Identity e cookies | Autenticação integrada; configuração atual é permissiva para demo |
| D-04 | Interface de catálogo seleciona RAWG ou demo por chave | Desenvolvimento sem credenciais; não equivale a integração IGDB |
| D-05 | Importação RAWG sob demanda e preenchimento de lacunas | Catálogo persiste; enriquecimento e refresh são limitados |
| D-06 | SVG local como fallback | Capas disponíveis sem API de imagens |
| D-07 | Tailwind via CDN e Razor compartilhado | Sem pipeline frontend local; dependência de internet |
| D-08 | Status numéricos persistidos e gêneros/plataformas em strings | Mudanças exigem compatibilidade e parsing |
| D-09 | Admin por papel e rota sem links | Autorização efetiva é o papel, não o nome da rota |

## Ainda não decidido

Hospedagem/domínio, banco para produção, autenticação/vinculação Steam, separação de demo, política de atualização RAWG, processamento em background, privacidade/moderação e estratégia de testes/CI. Não importar decisões de outros projetos como se fossem deste repositório.

## Como registrar uma nova decisão

Adicionar ID, data, status (proposta/aceita/substituída), problema, alternativas consideradas, escolha, consequências, migration/impacto operacional e referência de código/PR. Registrar aprovação somente quando de fato ocorrer. Mudanças rotineiras não precisam de uma decisão arquitetural artificial.
