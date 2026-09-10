# Arquitetura técnica

## Princípio

O jogo é dividido em quatro camadas para que GPS, mapas, Firebase, AR e multiplayer possam evoluir sem contaminar as regras do jogo.

```text
UI / Presentation
      ↓
Gameplay / Use Cases
      ↓
Domain
      ↑
Infrastructure / SDK adapters
```

## Cliente Unity

```text
Assets/NexusDogsGo/
├── Runtime/
│   ├── Core/          bootstrap e estado global
│   ├── Domain/        modelos puros
│   ├── Gameplay/      captura, batalha, missões, mundo, progressão
│   ├── Services/      contratos e implementações locais
│   ├── UI/            navegação e apresentação
│   └── Config/        configuração e catálogo inicial
├── Editor/            ferramentas para gerar a cena
└── Tests/             testes EditMode
```

## Serviços externos

- `IAuthService`: Firebase Auth, Play Games ou backend próprio.
- `IDataStore`: Firestore, REST/SQL ou armazenamento local.
- `IMapService`: Mapbox, Google Maps, Cesium ou solução própria.
- `IArService`: AR Foundation.
- `INetworkService`: Netcode, Photon, Nakama ou backend WebSocket.

A regra é simples: **nenhum sistema de gameplay deve depender diretamente de um SDK externo**.

## Fluxo principal

1. `GameBootstrap` cria os serviços e carrega o perfil.
2. `UnityLocationProvider` obtém a posição do jogador.
3. `DogSpawnService` gera encontros próximos.
4. O jogador abre o encontro e o `CaptureCalculator` calcula a probabilidade.
5. Em sucesso, o cão é adicionado à coleção e os itens são consumidos.
6. `MissionTracker` recebe eventos de caminhada/captura/batalha.
7. `IDataStore` persiste o estado.

## Backend

`Server/NexusDogsGo.Api` é uma API ASP.NET Core mínima. Nesta fase fornece health check, catálogo e um endpoint determinístico de spawn. Em produção, deve ser responsável pela autoridade de economia, eventos, raids, matchmaking e validação anti-cheat.

## Segurança

Nunca guardar API keys privadas, service-account JSON ou segredos no cliente. Configurações públicas de SDK mobile podem existir no build, mas qualquer segredo capaz de autorizar operações administrativas fica apenas no servidor/secret manager.
