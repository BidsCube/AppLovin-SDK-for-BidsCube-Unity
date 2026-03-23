# Bidscube SDK for Unity

Standalone Unity runtime SDK for **Bidscube** ads in **BidsCube SDK** mode (Unity/C# drives creatives) and **AppLovin MAX mediation** (early native init only; MAX drives load/show).

## Modes

- **BidsCube SDK** — default. Use `GetBannerAdView`, `ShowVideoAd`, `ShowNativeAd`, etc., after `BidscubeSDK.Initialize(config)`.
- **AppLovin MAX** — set `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`. Do **not** call C# APIs that embed creatives; integrate **AppLovin MAX** + the **Bidscube MAX adapter** per `Documentation~/APPLOVIN_MAX.md`.

Legacy stored values `levelPlay` / `level_play` map to **AppLovin MAX mediation** (backward compatible with older configs, same as Flutter).

## Package layout

- Core SDK: `Runtime/BidscubeSDK/`
- Android: bundled **`bidscube-sdk-1.0.0.aar`** + Editor **`BidscubeAndroidGradlePostprocessor`** (Maven deps injection)
- iOS: WebView plugins under `Runtime/Plugins/iOS/` (no bundled Bidscube iOS binary — add **BidscubeSDKAppLovin** / pods per MAX adapter docs)

## Documentation

| Doc | Purpose |
|-----|---------|
| `Documentation~/INTEGRATION.md` | BidsCube SDK usage, configuration, callbacks |
| `Documentation~/APPLOVIN_MAX.md` | MAX mediation, native flow, what not to call from C# |
| `Documentation~/ANDROID_BUNDLED_SDK.md` | AAR + Gradle injection |
| `Documentation~/TEST_PLAN.md` | Minimal QA checklist |

## UPM install

```json
"com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.0"
```

(Replace org/repo/tag with your fork and release tag.)
