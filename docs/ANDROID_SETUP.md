# Android — preparação

1. Instalar no Unity Hub: Android Build Support, Android SDK & NDK Tools e OpenJDK.
2. Em Player Settings usar IL2CPP e ARM64 para builds de produção.
3. Definir package id próprio, por exemplo `com.nexusdogsgame.nexusdogsgo`.
4. Pedir localização apenas quando o jogador entra no mapa.
5. Explicar na UI porque a localização é necessária antes do prompt do Android.
6. Não bloquear o jogo inteiro se GPS estiver indisponível: permitir coleção, inventário e treino offline.

## Permissões previstas

- localização precisa/aproximada para exploração;
- câmara para AR;
- notificações para eventos (opt-in);
- Internet para conta, sincronização e multiplayer.

Localização em background não é necessária para o primeiro vertical slice e deve ficar desligada até existir uma função clara que a justifique.
