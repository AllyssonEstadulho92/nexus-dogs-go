# Mapeamento do protótipo → implementação

O protótipo fornecido define onze experiências principais. Esta tabela funciona como contrato de UI para as próximas cenas/prefabs.

| Protótipo | ScreenId | Sistema |
|---|---|---|
| Entrada / Jogar | `Home` | bootstrap, sessão, definições |
| Escolher o teu cão | `DogSelect` | catálogo inicial e companheiro ativo |
| Mapa GPS | `Map` | localização, mapa, POIs e spawns |
| Captura | `Capture` | encontro, bola, bónus e chance de captura |
| Perfil / amigos | `Social` | perfil, amigos, feed e presença |
| Coleção | `Dogs` | cães capturados, filtros e detalhes |
| Missões | `Missions` | diárias, semanais e progresso |
| Batalha | `Battle` | ataque, defesa, item e HP |
| Inventário | `Inventory` | bolas, comida, poções, revive |
| Eventos | `Events` | raids, comunidade e temporadas |
| Câmara AR | `Ar` | AR Foundation e colocação do cão |

## Linguagem visual

- fundo azul-marinho profundo;
- cartões com cantos arredondados e superfície azul escura;
- ciano para navegação e ações secundárias;
- verde para ações positivas/Jogar;
- vermelho para ataque/perigo;
- amarelo para recompensas/itens;
- barra inferior persistente em Mapa, Cães, Missões, Inventário e Eventos;
- informação crítica de jogador sempre no topo: avatar, nível, moedas e gemas.

O ficheiro `docs/prototype-reference.svg` consolida o fluxo visual num único mapa de ecrãs para consulta dentro do repositório.
