# Встановлення: `com.bidscube.applovin.max` + `com.bidscube.sdk`

## 1. Пакети Unity (UPM)

У **`Packages/manifest.json`** додайте **ядро** та **адаптер** (версії збігайте з релізними тегами на GitHub):

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.9",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.20"
  }
}
```

Або **Package Manager** → **+** → **Add package from git URL** — спочатку core, потім адаптер.

Пакет адаптера оголошує залежність лише на **`com.bidscube.sdk`**. **Офіційний MAX** (`com.applovin.mediation.ads` тощо) додаєте **ви** у свій проєкт — див. документацію AppLovin.

## 2. AppLovin MAX Unity SDK (обов’язково)

Цей UPM-пакет **не** містить клас **`MaxSdk`**. Підключіть **офіційний** AppLovin MAX Unity Plugin (UPM або `.unitypackage`). Інакше **`AppLovinMaxUnityReflection`** не зможе ініціалізувати MAX.

## 3. Android

- **Мінімум:** API **26+** для зібраних AAR; узгодьте **compileSdk** / Gradle з шаблоном Unity.
- **Режим за замовчуванням — LiteNoVideo:** у проєкт копіюється **`bidscube-sdk-lite-no-video-1.2.4.aar`**, без Media3/Google IMA і без примусового **coreLibraryDesugaring** у launcher.
- **FullWithVideo** (нативне VAST/IMA): потрібен **`bidscube-sdk-full-video-1.2.4.aar`** у пакеті (або Maven **`com.bidscube:sdk-full-video`** за налаштуваннями постпроцесора). У редакторі: **Tools → Bidscube SDK → Android Build Features** або asset **Bidscube → Android Export Settings** → **FullWithVideo**.
- **Дублікат AAR:** у Inspector для **`bidscube-sdk-*.aar`** зазвичай вимикають **Android** plugin import, якщо постпроцесор сам кладе AAR у **`unityLibrary/libs/`** — щоб не злити двічі.

## 4. iOS (MAX)

У **Podfile** потрібні **`AppLovinSDK`** (13.x) та **`BidscubeSDKAppLovin`** (**1.0.4** — як у релізах native). Налаштування під ваш CI / post-build.

## 5. Ініціалізація (порядок)

- **Android:** якщо задаєте **`AdRequestAuthority` / SSP** через C# **`BidscubeSDK.Initialize`**, викликайте **`BidscubeSDK.Initialize`** **до** **`MaxSdk.InitializeSdk`**, щоб нативна конфігурація збігалася з адаптером.
- **Режим медіації:** у **`SDKConfig.Builder`** вкажіть **`IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`**, потім ініціалізація MAX. У цьому режимі **не** використовуйте C# API показу креативів ядра (`GetBannerAdView`, `ShowVideoAd`, …) — лише MAX.

Приклад мінімальної C# ініціалізації під MAX:

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .AdRequestAuthority("your-ssp-host.example.com")
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
// далі MaxSdk.InitializeSdk(...) за документацією AppLovin
```

Деталі API ядра (банери, колбеки, тестовий режим) — у репозиторії **`com.bidscube.sdk`**.

## 6. Кабінет AppLovin MAX (мінімум)

1. Додайте **Bidscube** як **custom SDK network** у медіації.
2. **Android:** клас адаптера **`com.applovin.mediation.adapters.BidscubeMediationAdapter`** (йде в bundled AAR).
3. **iOS:** **`ALBidscubeMediationAdapter`** (точна назва згідно з вашим iOS адаптером).
4. Поле **App ID** у налаштуваннях мережі для Bidscube — це **placement ID** Bidscube. За потреби server parameters: **`request_authority`** / **`ssp_host`**.

## 7. Типові проблеми

| Симптом | Дія |
|--------|-----|
| Немає **`MaxSdk`** | Встановіть офіційний MAX Unity SDK. |
| **`ClassNotFoundException` `com.bidscube.sdk.BidscubeSDK`** | Перевірте **`unityLibrary/build.gradle`**: одна залежність на core (файл з **`libs/`** або Maven **`@aar`**). |
| **Duplicate class / DEX** | Приберіть дубль core; не імпортуйте той самий AAR і через Unity Plugin, і через `unityLibrary/libs/`. |
| **Gradle / desugaring** | **LiteNoVideo** зазвичай без desugaring; **FullWithVideo** може потребувати **desugar_jdk_libs** на launcher — див. згенерований Gradle після експорту. |

SSP-хост задається в коді через **`SDKConfig.Builder.AdRequestAuthority(...)`** (деталі реалізації — у пакеті **`com.bidscube.sdk`**).
