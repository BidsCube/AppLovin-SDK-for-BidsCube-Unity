# Minimal test plan (Unity)

## Android: custom SSP host (`AdRequestAuthority`)

1. Build an Android player with `SDKConfig.Builder().AdRequestAuthority("your-staging-host.com").Build()` and `BidscubeSDK.Initialize(config)` (or `BaseURL("https://your-staging-host/sdk")` — same normalization).
2. Capture traffic (proxy / logcat network tags per your setup).
3. Confirm ad requests target the **configured host**, not only the default production URL.

## AppLovin MAX: shared native instance (smoke)

1. **Android:** use `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` and C# `Initialize` **before** MAX SDK init (same order as Flutter). **iOS:** either the same C# order or adapter-only init per [AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS).
2. **Android:** Gradle export includes injected **AppLovin SDK 13+** and bundled adapter AAR. **iOS:** **Podfile** lists **`BidscubeSDKAppLovin`** `1.0.3` and **`AppLovinSDK`** **13.x** (post-processor or manual `pod install`).
3. Load/show a **test** banner or interstitial through MAX.
4. Expect: no `InvalidOperationException` from Bidscube C# creative APIs (you should not call them); mediated loads succeed (**Android:** shared native instance when C# init is used; **iOS:** per adapter / native docs).

## Direct SDK mode (regression)

1. `IntegrationMode(BidscubeIntegrationMode.DirectSdk)` (or default).
2. Exercise banner / image / video / native from sample scene; confirm callbacks and visuals.

## Legacy config

1. `IntegrationModeFromWire("levelPlay")` resolves to **AppLovin MAX mediation** and behaves like MAX mode (creative APIs throw).
