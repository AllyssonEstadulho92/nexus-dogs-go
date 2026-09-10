# NEXUS DOGS GO

NEXUS DOGS GO é um projeto mobile em Unity/C# para Android com exploração por GPS, coleção de cães 3D, captura, missões, inventário, batalhas, AR, social e multiplayer. A arquitetura foi construída a partir do protótipo visual do projeto.

## Stack escolhida

- **Motor:** Unity 6 LTS (`6000.0.43f1`)
- **Cliente:** C# / Unity
- **Android:** Unity Android Build Support, IL2CPP e ARM64
- **GPS:** `Input.location` encapsulado por `ILocationProvider`
- **Mapa:** integração por `IMapService` para permitir Mapbox, Google Maps ou outro SDK sem acoplar o gameplay
- **AR:** AR Foundation 6.0.6 + ARCore XR Plugin 6.0.6 + XR Plugin Management 4.5.3
- **Dados:** persistência local JSON no bootstrap; Firestore/SQL através de `IDataStore`
- **Autenticação:** `IAuthService`, preparado para Firebase Auth ou backend próprio
- **Servidor:** ASP.NET Core (`Server/NexusDogsGo.Api`)
- **Multiplayer:** `INetworkService`, mantendo o cliente independente do fornecedor

## Estado atual

O repositório contém a base de domínio e um primeiro vertical slice jogável para:

- catálogo inicial de cães (Luna, Max, Thor, Mel, Rocky e Shadow);
- cálculo de CP, nível, experiência e raridade;
- GPS real no Android e GPS simulado dentro do Unity Editor;
- geração de spawns em redor do jogador;
- seleção de encontro diretamente a partir do mapa;
- sistema de captura com bola, alimento, probabilidade e qualidade do lançamento;
- gesto de lançamento por deslize no ecrã;
- inventário e consumo de itens;
- missões e progresso;
- batalha por turnos;
- companheiro 3D com comportamento de seguimento;
- projeção de coordenadas GPS para posições no mundo Unity;
- navegação entre os ecrãs principais do protótipo;
- persistência local;
- API backend mínima;
- testes EditMode para captura, geolocalização e projeção do mundo;
- CI do backend no GitHub Actions;
- configuração Android reproduzível e comandos de build para APK/AAB.

## Testar o vertical slice no Unity

1. Instalar **Unity 6 LTS 6000.0.43f1** com Android Build Support, SDK, NDK e OpenJDK.
2. Abrir a raiz deste repositório no Unity Hub e aguardar o Package Manager resolver as dependências.
3. Executar **NEXUS DOGS GO > Build Playable Vertical Slice**.
4. Executar **NEXUS DOGS GO > Configure > Configure Android Project**.
5. Executar **NEXUS DOGS GO > Configure > Validate Android Project**.
6. Abrir `Assets/Scenes/Prototype.unity` se não estiver já aberta e premir **Play**.
7. Escolher o cão inicial e abrir o **Mapa**.
8. No Editor, o GPS é simulado automaticamente para permitir testar sem um telemóvel.
9. Premir **ENCONTRO** para selecionar um spawn próximo.
10. No ecrã de captura, usar o botão **CAPTURAR** ou deslizar para cima na zona de lançamento.
11. Testar Poké Bola/Super Bola, Ração, fuga, consumo de inventário e regresso ao mapa.

No Android, o mesmo fluxo usa a localização real do dispositivo e pede a permissão de localização através do `UnityLocationProvider`.

## Build Android

- **Development APK:** `NEXUS DOGS GO > Build > Development APK`
- **Release AAB:** `NEXUS DOGS GO > Build > Release AAB`

A configuração automática aplica o application ID `com.allyssonestadulho92.nexusdogsgo`, versão `0.2.0`, minimum API 24, target API automático, IL2CPP, ARM64, Portrait e OpenGL ES 3. Os builds são colocados em `Builds/Android/`.

Para AR, depois do Package Manager terminar, ativar o **ARCore loader** em `Edit > Project Settings > XR Plug-in Management > Android`. A funcionalidade AR deve permanecer opcional em relação ao loop principal de GPS/captura.

## Componentes do vertical slice

- `MapExplorationController`: ciclo de GPS, atualização da posição, spawns e seleção de encontro.
- `CaptureEncounterController`: estado do encontro, escolha de item, captura, recompensa e persistência.
- `SwipeThrowController`: converte o gesto mobile em `Normal`, `Nice`, `Great` ou `Excellent`.
- `GeoSceneProjection`: converte diferenças GPS em offsets do mundo Unity.
- `WorldSpawnMarkerManager`: prepara marcadores de cães no espaço 3D.
- `CompanionFollower`: movimento base do cão companheiro atrás do jogador.
- `SimulatedLocationProvider`: localização determinística para desenvolvimento no Editor.
- `AndroidProjectConfigurator`: centraliza Player Settings, validação e build Android.

## Serviços externos

Firebase, fornecedor de mapas, assinatura Android e endpoints de produção exigem credenciais/configuração próprias e não são gravados diretamente no repositório. Consultar `docs/ANDROID_CONFIGURATION.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md` e `docs/PROTOTYPE_MAPPING.md`.

A imagem do protótipo define a direção de UX/UI, mas os assets finais, nomes, ícones, bolas e restantes elementos visuais devem ser originais ou devidamente licenciados antes de uma publicação comercial.
