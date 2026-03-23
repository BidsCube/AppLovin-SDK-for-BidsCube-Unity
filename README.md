# Bidscube SDK for Unity

Standalone Unity runtime SDK for **Bidscube** ads in **BidsCube SDK** mode (Unity/C# drives creatives) and **AppLovin MAX mediation** (early native init only; MAX drives load/show).

## Modes

- **BidsCube SDK** — default. Use `GetBannerAdView`, `ShowVideoAd`, `ShowNativeAd`, etc., after `BidscubeSDK.Initialize(config)`.
- **AppLovin MAX** — set `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`. Do **not** call C# APIs that embed creatives; integrate **AppLovin MAX** + the **Bidscube MAX adapter** per `Documentation~/APPLOVIN_MAX.md`.

Legacy stored values `levelPlay` / `level_play` map to **AppLovin MAX mediation** (backward compatible with older configs, same as Flutter).

## Configuring AppLovin MAX

Do this in the **AppLovin MAX** dashboard (and in your Bidscube / adapter docs) before or alongside the Unity steps in `Documentation~/APPLOVIN_MAX.md`:

1. **App registration** — Add your game in MAX with the correct **platform**, **package name** (Android) / **bundle ID** (iOS), and store URLs if required. Copy the **SDK key** into your MAX Unity plugin settings.
2. **Bidscube as a mediated network** — Under **MAX → Mediation** (or **Networks**), configure **Bidscube** as a **custom SDK network** (or the network type your adapter documentation specifies). Enter **API keys, account IDs, and placement identifiers** exactly as Bidscube’s MAX adapter guide describes — field names differ by adapter version.


AppLovin’s overview: [Integrating custom SDK networks](https://support.axon.ai/en/max/mediated-network-guides/integrating-custom-sdk-networks/). Unity code, native adapters, and `BidscubeSDK.Initialize` → **`Documentation~/APPLOVIN_MAX.md`**.

## Package layout

- Core SDK: `Runtime/BidscubeSDK/`
- Android: bundled **`bidscube-sdk-1.0.0.aar`** + Editor **`BidscubeAndroidGradlePostprocessor`** (Maven deps injection)
- iOS: WebView plugins under `Runtime/Plugins/iOS/` (no bundled Bidscube iOS binary — add **BidscubeSDKAppLovin** / pods per MAX adapter docs)



## UPM install

```json
"com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.0"
```

(Replace org/repo/tag with your fork and release tag.)
