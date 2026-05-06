# AppLovin MAX + Bidscube — short checklist

Use this page first. Details: [`APPLOVIN_MAX.md`](APPLOVIN_MAX.md), Android Gradle: [`ANDROID_BUNDLED_SDK.md`](ANDROID_BUNDLED_SDK.md).

## 1. Packages in `manifest.json`

- **`com.bidscube.sdk`** — Bidscube Unity SDK (C# + native wiring).
- **`com.bidscube.applovin.max`** — this adapter (MAX adapter AAR, Gradle post-processor, lite core AAR).
- **`com.applovin.mediation.ads`** — official AppLovin MAX Unity plugin (provides **`MaxSdk`**). Not a dependency of the adapter; you add it yourself.
- **Scoped registry** for AppLovin (see [`README.md`](../README.md) Install section) so `com.applovin.*` resolves.

Run **External Dependency Manager → Android Resolver** (or your usual resolve) after adding MAX.

## 2. Android: Lite vs full core

| Goal | Setting |
|------|--------|
| Smaller graph: banner / native / image, **no** in-app video stack | **`LiteNoVideo`** (default) — **`bidscube-sdk-lite-*.aar`** only, no Media3/IMA |
| MAX **rewarded / video** from native Bidscube stack | **`FullWithVideo`** + full **`bidscube-sdk-*.aar`** (bundled or Maven) — see [`ANDROID_BUNDLED_SDK.md`](ANDROID_BUNDLED_SDK.md) |

Change mode: **Tools → Bidscube SDK → Android Build Features**, or create **Assets → Create → Bidscube → Android Export Settings** and commit the **`.asset`** for CI.

### `coreLibraryDesugaring` (launcher)

**Default: leave Enable Desugaring on.** Bundled **`bidscube-sdk-lite-*.aar`** and full **`bidscube-sdk-*.aar`** usually ship AAR metadata that **requires** core library desugaring on **`:launcher`**; disabling it causes **`:launcher:checkReleaseAarMetadata`** to fail.

**Advanced:** If you use a Bidscube core dependency that does **not** require desugaring (custom build or future artifact), create **Bidscube Android Export Settings**, uncheck **Enable Desugaring**, and rebuild — the post-processor strips **`coreLibraryDesugaring`** and sets **`coreLibraryDesugaringEnabled false`** on the **generated** `launcher` and `unityLibrary` Gradle files.

**Note:** With **no** export settings asset, desugaring is **not** changed (whatever your Unity **Custom Launcher Template** / defaults export stays).

## 3. C# init order (Android)

1. Build **`SDKConfig`** with **`IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`** and your **`AdRequestAuthority`** / options.
2. **`BidscubeSDK.Initialize(config)`** — **before** MAX.
3. **`MaxSdk.InitializeSdk(...)`** (and optional SDK key / test device setup per AppLovin docs).

Do **not** use direct creative APIs (`GetBannerAdView`, `ShowVideoAd`, …) in MAX mode — load/show through MAX only.

## 4. MAX dashboard

Add **Bidscube** as a **custom SDK network** with the exact class names from [`APPLOVIN_MAX.md`](APPLOVIN_MAX.md). **App ID** = Bidscube **placement ID**. Enable the network on each ad unit.

## 5. iOS (if applicable)

Pods: **`AppLovinSDK`** 13.x + **`BidscubeSDKAppLovin`** (version pinned in docs). **`pod install`**, open **`.xcworkspace`**. See [`APPLOVIN_MAX.md`](APPLOVIN_MAX.md).

## 6. Verify

- Android **logcat**: Bidscube init, no **`ClassNotFoundException`** for **`BidscubeMediationAdapter`**.
- MAX **Mediation Debugger**: network loads; test ads per AppLovin.

Longer API / config reference: [`INTEGRATION.md`](INTEGRATION.md) (direct SDK); MAX-specific tables: [`APPLOVIN_MAX.md`](APPLOVIN_MAX.md).
