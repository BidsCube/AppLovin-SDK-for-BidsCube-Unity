# Встановлення: `com.bidscube.applovin.max` + `com.bidscube.sdk`

## 1. Пакети Unity (UPM)

У **`Packages/manifest.json`** додай **core** і **адаптер** (теги мають відповідати релізам на GitHub):

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.8",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.18"
  }
}
```

Або через **Package Manager** → **Add package from git URL** — спочатку core, потім адаптер.

## 2. AppLovin MAX Unity SDK (обов’язково окремо)

Цей UPM-пакет **не** містить плагін з класом **`MaxSdk`**. Встанови **офіційний AppLovin MAX Unity SDK** з дистрибутива AppLovin. Інакше **`AppLovinMaxUnityReflection.IsMaxSdkAvailable`** залишиться `false`.

## 3. Android: core AAR і режим

- За замовчуванням (**LiteNoVideo**): у проєкт копіюється **`bidscube-sdk-lite-no-video-1.2.4.aar`**, без Media3/IMA і без примусового desugaring у launcher. Підходить для банерів / нативу / image; нативне відео через Bidscube в lite обмежене.
- **FullWithVideo**: потрібен **`bidscube-sdk-full-video-1.2.4.aar`** у пакеті (або Maven **`com.bidscube:sdk-full-video`** у налаштуваннях експорту). Gradle додасть Media3/IMA і за потреби desugaring.

Змінити режим: **Tools → Bidscube SDK → Android Build Features** або ScriptableObject **Assets → Create → Bidscube → Android Export Settings** (`featureSet`). Після зміни — знову збери Android.

**Мінімум:** Android **minSdk ≥ 26** (узгодь зі своїм шаблоном). У **Inspector** для **`bidscube-sdk-*.aar`** часто вимикають **Android** (щоб Unity не мерджив двічі) — копію в **`unityLibrary/libs/`** робить postprocessor.

## 4. iOS (MAX mediation)

У **Podfile** додай залежності під свій процес (версії узгодь з **`AdapterPackageInfo`** у коді адаптера), зокрема **`BidscubeSDKAppLovin`** та **`AppLovinSDK`** — як у релізних нотатках нативного iOS-адаптера.

## 5. Ініціалізація (порядок)

- **Android:** якщо задаєш **`AdRequestAuthority`** / конфіг через C# **`BidscubeSDK.Initialize`**, викликай **до** **`MaxSdk.InitializeSdk`**, щоб нативний шар бачив той самий SSP host.
- **Режим MAX mediation:** у **`SDKConfig.Builder`** вкажи **`IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`**, потім ініціалізуй MAX. У цьому режимі **не** використовуй C# API на кшталт **`GetBannerAdView` / `ShowVideoAd`** — лише завантаження/показ через MAX.

Приклад напряму SDK (не MAX) дивись у **`com.bidscube.sdk`** (README / приклади пакета).

## 6. AppLovin MAX dashboard (мінімум)

1. Зареєструй додаток, додай **SDK key** у MAX Unity plugin.
2. У **Mediation → Networks** додай **Bidscube** як **custom SDK network** на потрібних ad units.
3. **Android:** клас адаптера **`com.applovin.mediation.adapters.BidscubeMediationAdapter`** (йде в bundled AAR).
4. **iOS:** клас згідно з документацією нативного адаптера (на кшталт **`ALBidscubeMediationAdapter`**).
5. Поле **App ID** у налаштуванні мережі в MAX — це **Bidscube placement ID**. За потреби **server parameters:** **`request_authority`** або **`ssp_host`**.

## 7. Семпли

У **Package Manager** → **`com.bidscube.applovin.max`** → **Samples** → імпорт **SDK Demo** (потребує **`com.bidscube.sdk`**).

## 8. Типові проблеми

| Симптом | Дія |
|--------|-----|
| Немає **`MaxSdk`** | Встанови офіційний **AppLovin MAX Unity** plugin. |
| **`ClassNotFoundException` `com.bidscube.sdk.BidscubeSDK`** | Переконайся, що в згенерованому **`unityLibrary/build.gradle`** одна лінія core (`files('libs/bidscube-sdk-…')` або Maven **`@aar`**). |
| Дублікат класів Bidscube | Прибери другу залежність core; для AAR у Plugins вимкни Android merge, якщо postprocessor копіює в **`libs/`**. |
| Помилки Gradle / desugaring | **LiteNoVideo** + lite AAR зазвичай без desugaring; **FullWithVideo** може вимагати desugaring — перемкни режим або додай full AAR / Maven. |

Деталі версій AAR і рядків Gradle див. **`Runtime/.../AdapterPackageInfo.cs`** та **`package.json`**.
