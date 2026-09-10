# Android configuration — NEXUS DOGS GO

Este documento define a configuração reproduzível do cliente Android.

## Versão base

- Unity Editor: `6000.0.43f1`
- Android: ARM64
- Scripting backend: IL2CPP
- Orientação: Portrait
- Application ID: `com.allyssonestadulho92.nexusdogsgo`
- Versão: `0.2.0`
- Version code: `2`
- Minimum API: 24
- Target API: automático, usando o SDK mais recente instalado no Unity
- Graphics API: OpenGL ES 3

## Configuração automática

No Unity Editor executar:

`NEXUS DOGS GO > Configure > Configure Android Project`

Depois validar com:

`NEXUS DOGS GO > Configure > Validate Android Project`

O configurador aplica identificação, versão, API mínima, IL2CPP, ARM64, orientação, Graphics API e adiciona a cena `Assets/Scenes/Prototype.unity` ao Build Settings quando ela existir.

## Construir o vertical slice

1. Executar `NEXUS DOGS GO > Build Playable Vertical Slice`.
2. Executar `NEXUS DOGS GO > Configure > Configure Android Project`.
3. Para teste local, executar `NEXUS DOGS GO > Build > Development APK`.
4. Para distribuição, executar `NEXUS DOGS GO > Build > Release AAB`.

Os artefactos são criados em `Builds/Android/` e não entram no Git.

## AR Foundation / ARCore

O `Packages/manifest.json` inclui:

- `com.unity.xr.arfoundation` 6.0.6
- `com.unity.xr.arcore` 6.0.6
- `com.unity.xr.management` 4.5.3

No primeiro carregamento do projeto, o Unity Package Manager irá resolver estes pacotes. Depois, em `Edit > Project Settings > XR Plug-in Management > Android`, ativar o ARCore loader. O jogo deve continuar funcional mesmo quando o dispositivo não suporta AR; a exploração GPS não deve depender da disponibilidade de ARCore.

## Serviços externos que não podem ser preenchidos no repositório

### Firebase

Não colocar `google-services.json`, `GoogleService-Info.plist`, chaves privadas nem service-account JSON no Git. Estes ficheiros já estão ignorados pelo `.gitignore`.

Quando existir um projeto Firebase real:

1. registar o package `com.allyssonestadulho92.nexusdogsgo`;
2. descarregar o `google-services.json` para a máquina de desenvolvimento;
3. configurar Authentication e Firestore;
4. criar adaptadores para `IAuthService` e `IDataStore` em vez de acoplar Firebase ao gameplay.

### Mapas

O fornecedor de mapas permanece atrás de `IMapService`. Só deve ser ligado depois de ser escolhido o SDK e criada uma chave própria. Tokens de mapas não devem ser gravados diretamente em C#.

### Backend

O backend ASP.NET Core existente deve ser publicado num endpoint HTTPS antes de substituir serviços locais. URLs de produção e segredos devem vir de configuração de ambiente, não de constantes sensíveis no repositório.

## Assinatura Android

O comando `Release AAB` gera o bundle, mas a publicação na Google Play exige um keystore real. O keystore e passwords não devem ser commitados. Configurar a assinatura localmente em `Project Settings > Player > Publishing Settings` ou por variáveis seguras de CI.

## Critério para merge

Antes de promover para `main`:

- abrir o projeto sem erros de compilação;
- deixar o Package Manager resolver todas as dependências;
- executar os EditMode Tests;
- gerar `Prototype.unity`;
- validar `Configure/Validate Android Project`;
- gerar pelo menos um Development APK;
- testar GPS real num dispositivo Android;
- testar o fallback sem ARCore.
