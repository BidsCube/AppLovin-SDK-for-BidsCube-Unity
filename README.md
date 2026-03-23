# Bidscube SDK for Unity

Standalone Unity runtime SDK for **Bidscube** ads in **direct** mode (Unity/C# drives creatives) and **AppLovin MAX mediation** (early native init only; MAX drives load/show).

## Modes

- **Direct / SDK** — default. Use `GetBannerAdView`, `ShowVideoAd`, `ShowNativeAd`, etc., after `BidscubeSDK.Initialize(config)`.
- **AppLovin MAX** — set `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`. Do **not** call C# APIs that embed creatives; integrate **AppLovin MAX** + the **Bidscube MAX adapter** per `Documentation~/APPLOVIN_MAX.md`.

Legacy stored values `levelPlay` / `level_play` map to **AppLovin MAX mediation** (backward compatible with older configs, same as Flutter).

## Package layout

- Core SDK: `Runtime/BidscubeSDK/`
- Android: bundled **`bidscube-sdk-1.0.0.aar`** + Editor **`BidscubeAndroidGradlePostprocessor`** (Maven deps injection)
- iOS: WebView plugins under `Runtime/Plugins/iOS/` (no bundled Bidscube iOS binary — add **BidscubeSDKAppLovin** / pods per MAX adapter docs)

## Documentation

| Doc | Purpose |
|-----|---------|
| `Documentation~/INTEGRATION.md` | Direct SDK usage, configuration, callbacks |
| `Documentation~/APPLOVIN_MAX.md` | MAX mediation, native flow, what not to call from C# |
| `Documentation~/ANDROID_BUNDLED_SDK.md` | AAR + Gradle injection |
| `Documentation~/TEST_PLAN.md` | Minimal QA checklist |

## UPM install

```json
"com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.0"
```

(Replace org/repo/tag with your fork and release tag.)

## GitHub releases

Теги **`vMAJOR.MINOR.PATCH`** мають збігатися з **`package.json` → `version`** (без `v`). Деталі, ZIP-імена та міграція зі старого репозиторію LevelPlay — у **[`RELEASE.md`](RELEASE.md)**. Перед тегом можна виконати `./tools/verify-release-ready.sh`.

## Requirements

- Unity **2020.3**+ (see `package.json`)
- **Android**: minSdk **26+** (Gradle / AAR metadata; див. `Documentation~/ANDROID_BUNDLED_SDK.md`), Unity Android Build Support (for the Gradle post-processor)
- **MAX mediation**: AppLovin MAX SDK + Bidscube MAX adapter versions from your integration guide; align native **Bidscube** versions with the Flutter plugin’s Android `com.bidscube:bidscube-sdk` / iOS pod when standardizing releases.

## Changelog

See `CHANGELOG.md` for breaking changes and migration from removed Level Play / IronSource Unity bridge artifacts.
