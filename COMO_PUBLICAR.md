# Como publicar o AlePulse na Google Play

Pré-requisitos: Visual Studio 2022 (17.14+) ou .NET 10 SDK com workload `maui-android` instalado (você já compila o app hoje, então já tem isso).

## Passo 1 — Criar sua chave de assinatura (só uma vez na vida)

No terminal (Windows: PowerShell; Mac/Linux: Terminal), rode:

```
keytool -genkeypair -v -keystore alepulse.keystore -alias alepulse -keyalg RSA -keysize 2048 -validity 10000
```

- O `keytool` vem com o JDK usado pelo Android (no Windows: `C:\Program Files\dotnet\pkgs\...` ou o JDK do Visual Studio; se não achar, instale o "Java JDK 17+").
- Ele vai pedir uma **senha** e seus dados (nome, organização…). **Anote a senha e guarde o arquivo `alepulse.keystore` em local seguro com backup** — sem ele a Google não deixa atualizar o app.

## Passo 2 — Gerar o pacote (.aab) já assinado

Defina as 4 variáveis de ambiente apontando para o arquivo e as senhas (exemplo Windows PowerShell):

```powershell
$env:AlePulseKeystorePath = "C:\caminho\alepulse.keystore"
$env:AlePulseKeyAlias     = "alepulse"
$env:AlePulseKeyPass      = "senhaDaChave"
$env:AlePulseStorePass    = "senhaDoArquivo"
```

Gere o pacote:

```
dotnet publish src/AlePulse.Mobile/AlePulse.Mobile.csproj -f net10.0-android -c Release -p:AlePulseSigning=true
```

O arquivo sai em:
`src/AlePulse.Mobile/bin/Release/net10.0-android/publish/` — procuro por `com.alepulse.app.aab` (alguns sufixos de versão podem aparecer no nome).

Dica: para testar no celular antes de publicar, gere um APK de teste com `-p:AndroidPackageFormat=apk`.

## Passo 3 — Subir na Google Play

1. Crie a conta de desenvolvedor em play.google.com/console (taxa única de ~US$ 25).
2. "Criar app" → nome **AlePulse** → categoria Saúde e Fitness.
3. Preencha as fichas obrigatórias: descrição, capturas de tela (use os prints do app no seu celular), ícone 512×512, Política de Privacidade (obrigatória — o app coleta e-mail e dados de treino; um texto simples hospedado em qualquer site resolve), Declaração de Conteúdo e de Dados.
4. Em "Teste interno/fechado", crie uma versão, faça upload do `.aab` e teste no seu celular (seu e-mail entra como testador).
5. Depois de validado, promova para **Produção**.
6. O app usa internet (API hospedada) e vibração — na Declaração de Dados declare coleta de e-mail (conta) e dados de treino (conteúdo do usuário). Nada de anúncios.

## Observações importantes

- **ID do pacote novo (`com.alepulse.app`)**: na Play ele é um app novo. No seu celular, a versão antiga (com.companyname.alepulse.mobile1) continua instalada — desinstale-a se quiser evitar duplicidade. Seus dados estão no servidor, então é só logar no app novo.
- **Versão**: para cada atualização futura, aumente `ApplicationVersion` (1 → 2 → 3…) no `AlePulse.Mobile.csproj` antes de gerar o `.aab`.
- **`usesCleartextTraffic=true`** está habilitado no AndroidManifest (sobra da fase Ngrok). A API hoje é HTTPS, então é seguro remover depois; enquanto estiver presente, a Google não bloqueia, apenas recomenda HTTPS.
- A assinatura só é aplicada quando as variáveis `AlePulse*` existirem; sem elas o build Release sai sem assinar (serve para testes locais).
