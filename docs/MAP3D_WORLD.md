# NEXUS WORLD 3D

## Objetivo

O mapa do NEXUS DOGS GO passa a ser tratado como um mundo 3D jogável, e não apenas como uma imagem de mapa. A localização GPS continua a ser a fonte de posição do jogador, enquanto a representação visual é construída no Unity.

## Elementos implementados

- ruas 3D com marcações de faixa e passadeiras;
- passeios e quarteirões;
- edifícios com alturas variadas, telhados, entradas e bandas de janelas;
- parques com caminhos, árvores, bancos e fonte;
- praças, monumentos e zonas de água;
- postes de iluminação e elementos urbanos;
- Nexus Hubs e arenas de desafio;
- avatar 3D do jogador;
- cão companheiro 3D;
- cães selvagens 3D animados;
- aura visual distinta por raridade;
- câmara com rotação, zoom, follow e look-ahead;
- ciclo dia/noite sincronizado com a hora local;
- origem geográfica persistente para impedir que os spawns deslizem em relação ao cenário.

## Fluxo de teste

1. Abrir o projeto em Unity 6000.0.43f1.
2. Executar `NEXUS DOGS GO > Build Playable Vertical Slice`.
3. Premir Play.
4. Escolher um cão inicial e entrar no mapa.
5. Rodar a câmara por arrasto e aproximar/afastar por pinça ou roda do rato.
6. Selecionar um cão selvagem para iniciar um encontro.

## Limites atuais

O cenário é procedural e serve como mundo jogável independente. Ainda não usa geometria de edifícios/estradas proveniente de um fornecedor cartográfico real. A arquitetura mantém `IMapService` para permitir ligar um fornecedor licenciado sem reescrever o loop de gameplay.

Os modelos atuais do jogador e dos cães são construídos com primitivas Unity para que o projeto funcione sem assets externos. Devem ser substituídos por modelos 3D originais/licenciados com rig, animações e LODs antes de uma versão de produção.

## Próxima camada de produção

A evolução natural é ligar dados cartográficos reais, substituir os modelos procedurais por personagens finais, introduzir animações mecanim, LOD/object pooling, navegação mais refinada do companheiro, partículas e efeitos sonoros próprios, mantendo as regras de performance mobile.
