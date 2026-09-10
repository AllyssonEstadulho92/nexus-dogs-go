# Roadmap de implementação

## Fase 1 — Núcleo jogável

- [x] arquitetura Unity/C#;
- [x] catálogo base de cães;
- [x] localização e distância geográfica;
- [x] spawns locais;
- [x] captura;
- [x] inventário;
- [x] missões;
- [x] batalha;
- [x] navegação de protótipo;
- [x] API backend mínima;
- [ ] modelos 3D e animações;
- [ ] mapa real;
- [ ] save cloud.

## Fase 2 — Vertical slice Android

- integrar SDK de mapas;
- permissões de localização Android;
- criar mapa com POIs, jogador e cão companheiro;
- criar 6 modelos 3D iniciais;
- ecrã de encontro/captura completo;
- áudio, VFX e haptics;
- autenticação Firebase;
- Firestore para perfil/coleção/inventário;
- telemetria e crash reporting.

## Fase 3 — AR e social

- AR Foundation + ARCore;
- colocação, escala e fotografia do cão no mundo real;
- lista de amigos;
- presença e convites;
- trocas com regras anti-fraude.

## Fase 4 — Multiplayer e eventos

- raids cooperativas;
- PvP assíncrono e/ou tempo real;
- temporadas;
- eventos geográficos;
- backend autoritativo;
- anti-cheat de localização e economia.

## Regra de qualidade

Nenhuma integração externa deve ser feita diretamente dentro de UI ou modelos de domínio. Cada SDK entra por um adaptador de `Services/`.
