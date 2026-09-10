# NEXUS DOGS GO

NEXUS DOGS GO é um projeto mobile em Unity/C# para Android com exploração por GPS, coleção de cães 3D, captura, missões, inventário, batalhas, AR, social e multiplayer. A primeira arquitetura foi construída a partir do protótipo visual do projeto.

## Stack escolhida

- **Motor:** Unity 6 LTS
- **Cliente:** C# / Unity
- **Android:** Unity Android Build Support
- **GPS:** `Input.location` encapsulado por `ILocationProvider`
- **Mapa:** integração por `IMapService` para permitir Mapbox, Google Maps ou outro SDK sem acoplar o gameplay
- **AR:** contrato `IArService`; AR Foundation entra como adaptador de infraestrutura
- **Dados:** persistência local JSON no bootstrap; Firestore/SQL através de `IDataStore`
- **Autenticação:** `IAuthService`, preparado para Firebase Auth ou backend próprio
- **Servidor:** ASP.NET Core (`Server/NexusDogsGo.Api`)
- **Multiplayer:** `INetworkService`, mantendo o cliente independente do fornecedor

## Estado atual

O repositório já contém a base jogável de domínio e serviços para:

- catálogo inicial de cães (Luna, Max, Thor, Mel, Rocky e Shadow);
- cálculo de CP, nível, experiência e raridade;
- GPS e cálculo geográfico;
- geração de spawns em redor do jogador;
- sistema de captura com bola, bónus e qualidade do lançamento;
- inventário;
- missões e progresso;
- batalha por turnos;
- navegação entre os ecrãs principais do protótipo;
- persistência local;
- API backend mínima;
- gerador de cena de protótipo no Unity Editor;
- testes unitários do núcleo de captura e geolocalização.

## Começar no Unity

1. Instalar uma versão compatível do **Unity 6 LTS** com Android Build Support.
2. Abrir a raiz deste repositório no Unity Hub.
3. No Unity, executar **NEXUS DOGS GO > Build Prototype Scene**.
4. Abrir `Assets/Scenes/Prototype.unity`.
5. Premir Play.

O gerador cria uma cena funcional de navegação para validar rapidamente o fluxo: Mapa → Cães → Missões → Inventário → Eventos.

## Próximas integrações

Consultar `docs/ROADMAP.md`, `docs/ARCHITECTURE.md` e `docs/PROTOTYPE_MAPPING.md` antes de ligar mapas, Firebase, AR Foundation e multiplayer real.

> O protótipo define a direção visual. O código foi separado em domínio, gameplay, serviços e UI para impedir que a futura troca de SDK de mapas, backend ou multiplayer obrigue a reescrever o jogo.
