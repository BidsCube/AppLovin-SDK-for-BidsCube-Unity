# Bidscube SDK for Unity (`com.bidscube.sdk`)

UPM package for **Bidscube** ads in Unity: **BidsCube SDK** mode (Unity/C# drives banners, video, native, interstitials) and **AppLovin MAX mediation** (early native `BidscubeSDK` init only; MAX drives load/show via the Bidscube adapter).

## Contents

- [Install (UPM)](#install-upm)
- [Features](#features)
- [Requirements](#requirements)
- [Integration modes](#integration-modes)
- [AppLovin MAX (mediation)](#applovin-max-mediation)
- [Configuring AppLovin MAX (dashboard)](#configuring-applovin-max-dashboard)
- [Quick start (BidsCube SDK)](#quick-start-bidscube-sdk)
- [Ad request endpoint & SSP URLs](#ad-request-endpoint--ssp-urls)
- [Package layout](#package-layout)
- [Documentation](#documentation)
- [Samples](#samples)
- [Troubleshooting](#troubleshooting)
- [Releasing (maintainers)](#releasing-maintainers)
- [Changelog & license](#changelog--license)

---

## Install (UPM)

Add to the Unity project **`Packages/manifest.json`**:

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.1"
  }
}
```

Or **Package Manager → Add package from git URL** with the same URL (replace **org / repo / tag** with your fork and release).

**Local development** (this repo next to your project):

```json
"com.bidscube.sdk": "file:../AppLovin-SDK-Unity"
```

Package metadata: [`package.json`](package.json) (`displayName`: **Bidscube SDK**).

---

## Features

| Area | Behavior |
|------|----------|
| **BidsCube SDK** (default) | C# APIs: `GetBannerAdView`, `GetVideoAdView`, `GetNativeAdView`, `ShowImageAd`, `ShowVideoAd`, `ShowNativeAd`, consent helpers, etc. |
| **AppLovin MAX** | `BidscubeIntegrationMode.AppLovinMaxMediation` — early `BidscubeSDK.Initialize`; creatives **only** through MAX + Bidscube adapter (C# creative APIs throw) |
| **Android** | Bundled **`bidscube-sdk-1.0.0.aar`** + Editor **`BidscubeAndroidGradlePostprocessor`** (Maven / compileSdk / minSdk fixes) |
| **iOS** | WebView-related plugins under `Runtime/Plugins/iOS/`; Bidscube native + MAX adapter per your integration guide |
| **Legacy configs** | Wire values `levelPlay` / `level_play` → **AppLovin MAX mediation** (same idea as Flutter) |

---

## Requirements

| Requirement | Details |
|-------------|---------|
| **Unity** | **2020.3+** (see [`package.json`](package.json) `unity`) |
| **UPM dependencies** | `com.unity.ugui`, `com.unity.textmeshpro` (declared in `package.json`) |
| **Android** | **Minimum API Level ≥ 26** recommended with the injected dependency set; **compileSdk** raised by post-processor as needed — [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md) |
| **MAX mediation** | AppLovin MAX Unity plugin (or native MAX) + **Bidscube MAX adapter** artifacts — [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md) |

---

## Integration modes

| Mode | `SDKConfig.Builder` | Ads from Unity C# |
|------|---------------------|-------------------|
| **BidsCube SDK** | `IntegrationMode(BidscubeIntegrationMode.DirectSdk)` (default) | **Yes** — `GetBannerAdView`, `ShowVideoAd`, etc. |
| **AppLovin MAX** | `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` | **No** — use MAX only; C# creative APIs throw |

Wire strings (JSON / `PlayerPrefs`): `direct`, `directSdk`, `appLovinMax`, and legacy `levelPlay` / `level_play` → MAX (see `BidscubeIntegrationModeWire` in code).

---

## AppLovin MAX (mediation)

Mediation is implemented in **native** Bidscube adapters for MAX, not via Unity `UnitySendMessage`. Call **`BidscubeSDK.Initialize`** early with **`AppLovinMaxMediation`** so the adapter shares the **same** native SDK instance.

### Native flow (formats)

| Format | Native Bidscube SDK | In MAX |
|--------|------------------------|--------|
| **Banner** | `getImageAdView` → `View` | Passed to MAX; AdDisplayManager / WebView with rendered ADM |
| **Interstitial** | `showImageAd` | Full-screen image from rendered ADM |
| **Video / rewarded** | `showVideoAd` | IMA + fullscreen container |
| **Native** | `getNativeAdView` → payload | Adapter builds **MaxNativeAd** — not the SDK view as the mediated creative |

### C# initialization (mediation)

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .AdRequestAuthority("your-ssp-host.example.com")
    .EnableTestMode(false)
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
// Then: AppLovin MAX SDK init, ad units, load/show via MAX only.
```

**Do not** call `GetBannerAdView`, `GetNativeAdView`, `GetVideoAdView`, `ShowImageAd`, `ShowVideoAd`, `ShowNativeAd`, etc. in MAX mode — see [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md).

---

## Configuring AppLovin MAX (dashboard)

Do this in the **AppLovin MAX** UI alongside Unity setup:

1. **App registration** — Platform, **package name** (Android) / **bundle ID** (iOS), store links if needed. Copy the **SDK key** into the MAX Unity plugin.
2. **Bidscube as a mediated network** — Under **Mediation / Networks**, add **Bidscube** as a **custom SDK network** (per your adapter doc). Enter **keys, account IDs, placement IDs** exactly as the adapter specifies.

| Item | Where | Note |
|------|--------|------|
| **SDK Key** | AppLovin MAX | MAX SDK initialization |
| **Ad unit IDs** | MAX dashboard | Load/show APIs |
| **Custom network** | MAX mediation | Bidscube adapter class / package |
| **Android** | Network setup | e.g. `BidscubeMediationAdapter` (match your artifact) |
| **iOS** | Network setup | e.g. `ALBidscubeMediationAdapter` |
| **Instance / custom string** | Network instance | Bidscube **placement ID** |

3. **Ad units** — Create MAX ad units per format; **enable Bidscube** and map **network placement IDs**.
4. **Waterfall / bidding** — Order lines or enable bidding per your strategy.
5. **Testing** — MAX test mode / test devices; align native **test mode** with `SDKConfig` if used.

Overview: [Integrating custom SDK networks](https://support.axon.ai/en/max/mediated-network-guides/integrating-custom-sdk-networks/).

Full Unity steps: [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md).

---

## Quick start (BidsCube SDK)

Default mode is **direct** integration — Unity drives ads after init.

### 1. Initialize

```csharp
using BidscubeSDK;

var config = new SDKConfig.Builder()
    .AdRequestAuthority("ssp-bcc-ads.com")
    .EnableLogging(true)
    .EnableDebugMode(true)
    .DefaultAdTimeout(30000)
    .DefaultAdPosition(AdPosition.Header)
    .EnableTestMode(true)
    .Build();

BidscubeSDK.BidscubeSDK.Initialize(config);
```

### 2. Show or attach ads (examples)

```csharp
// Full-screen image
BidscubeSDK.BidscubeSDK.ShowImageAd("your_placement_id", myCallback);

// Banner / image as GameObject in your scene hierarchy
var bannerGo = BidscubeSDK.BidscubeSDK.GetBannerAdView("banner_placement", myCallback);

// Video widget
var videoGo = BidscubeSDK.BidscubeSDK.GetVideoAdView("video_placement", myCallback);

// Native
var nativeGo = BidscubeSDK.BidscubeSDK.GetNativeAdView("native_placement", myCallback);
```

Implement **`IAdCallback`** (or your project’s callback type) for load / fail / click / close events. See [`Documentation~/INTEGRATION.md`](Documentation~/INTEGRATION.md) for the full API surface, consent, and callbacks.

---

## Ad request endpoint & SSP URLs

- Set the SSP **host** (and optional **port**) with **`SDKConfig.Builder.AdRequestAuthority(string)`**. Default: **`ssp-bcc-ads.com`** (same as Android `DEFAULT_AD_REQUEST_AUTHORITY`).
- **`SDKConfig.BaseURL`** is a read-only **`https://<authority>/sdk`** (do not pass full URLs with query — the SDK appends `/sdk` and query parameters per ad type).
- Full reference (normalization, IPv6, query tables, GET + JSON `adm` / `position`): [`Documentation~/AD_REQUEST_ENDPOINT.md`](Documentation~/AD_REQUEST_ENDPOINT.md).

---

## Package layout

| Path | Role |
|------|------|
| `Runtime/BidscubeSDK/` | Core C# SDK, `BidscubeSDK`, `SDKConfig`, Android interop |
| `Runtime/Plugins/Android/` | **`bidscube-sdk-1.0.0.aar`**, WebView templates |
| `Runtime/Plugins/iOS/` | WebView native plugins |
| `Editor/Android/` | **`BidscubeAndroidGradlePostprocessor`** |
| `Documentation~/` | Markdown docs (this package) |
| `Samples~/SDK Demo/` | Example scenes (import via **Samples** in Package Manager) |

---

## Documentation

| Doc | Purpose |
|-----|---------|
| [`Documentation~/INTEGRATION.md`](Documentation~/INTEGRATION.md) | BidsCube SDK usage, configuration, callbacks |
| [`Documentation~/AD_REQUEST_ENDPOINT.md`](Documentation~/AD_REQUEST_ENDPOINT.md) | **`AdRequestAuthority`**, HTTPS `/sdk` base URL, query params by ad type, SSP response |
| [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md) | MAX mediation, native flow, what not to call from C# |
| [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md) | AAR, Gradle injection, `CheckAarMetadata` / compileSdk |
| [`Documentation~/TEST_PLAN.md`](Documentation~/TEST_PLAN.md) | Minimal QA checklist |

---

## Samples

In **Package Manager**, select **Bidscube SDK** → **Samples** → import **SDK Demo** (`Samples~/SDK Demo`: scenes and scripts).

---

## Troubleshooting

| Issue | What to check |
|-------|----------------|
| **Android Gradle / AAR metadata errors** | **minSdk ≥ 26**, clean Gradle export; see [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md) |
| **Ads not loading** | `BaseURL`, placement IDs, device logs, network |
| **MAX mode** | `IntegrationMode` is **AppLovinMaxMediation**, adapter + MAX SDK versions, dashboard network setup |
| **Wrong mode behavior** | Direct APIs used in MAX mode → `InvalidOperationException`; use MAX only |

---

## Releasing (maintainers)

Tags **`v*`** must match [`package.json`](package.json) `version`. See [`RELEASE.md`](RELEASE.md) and run [`tools/verify-release-ready.sh`](tools/verify-release-ready.sh) before tagging. CI/CD: [`.github/workflows/ci.yml`](.github/workflows/ci.yml), [`.github/workflows/release.yml`](.github/workflows/release.yml).

---

## Changelog & license

- **Changelog:** [`CHANGELOG.md`](CHANGELOG.md)  
- **License:** [`LICENSE.md`](LICENSE.md)
