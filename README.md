# Bidscube SDK for Unity (`com.bidscube.sdk`)

UPM package for **Bidscube** ads in Unity: **BidsCube SDK** mode (Unity/C# drives banners, video, native, interstitials) and **AppLovin MAX mediation** (MAX drives load/show via the Bidscube adapter; **Android:** early C# **`BidscubeSDK.Initialize`** recommended; **iOS:** optional — adapter can init native BidCube — see [`APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md)).

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
    "com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.3.1"
  }
}
```

Or **Package Manager → Add package from git URL** with the same URL (replace **org / repo / tag** with your fork and release).

**Unity UI dependencies:** `com.unity.ugui` and `com.unity.textmeshpro` are listed in [`package.json`](package.json) and are pulled in automatically when you add this package by Git URL. If they are missing (e.g. odd manifest state), the Editor runs **`BidscubeUpmDependencyInstaller`** once per session and issues `PackageManager.Client.Add` for the same versions — same idea as the [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) README (“no manual setup”).

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
| **Android** | Bundled **`applovin-bidscube-max-adapter-1.0.3.aar`**; core **`com.bidscube:bidscube-sdk`** resolved from Maven on export; Editor **`BidscubeAndroidGradlePostprocessor`** (AppLovin SDK 13.0+, Maven deps, compileSdk / minSdk / desugar). **Do not** duplicate `implementation 'com.bidscube:bidscube-sdk:…'` in Custom Gradle. |
| **iOS** | WebView plugins under `Runtime/Plugins/iOS/`; MAX: CocoaPods **`BidscubeSDKAppLovin`** `1.0.3` + **`AppLovinSDK`** `13.x` ([iOS repo](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS)); **`BidscubeIosPodfilePostprocessor`** appends missing pods to the exported **Podfile** |
| **Legacy configs** | Wire values `levelPlay` / `level_play` → **AppLovin MAX mediation** (same idea as Flutter) |

---

## Requirements

| Requirement | Details |
|-------------|---------|
| **Unity** | **2020.3+** (see [`package.json`](package.json) `unity`) |
| **UPM dependencies** | `com.unity.ugui`, `com.unity.textmeshpro` (declared in `package.json`) |
| **Android** | **Minimum API Level ≥ 26** recommended with the injected dependency set; **compileSdk** raised by post-processor as needed — [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md) |
| **MAX mediation** | **AppLovin MAX** (Unity plugin or native). **Android:** adapter AAR is bundled. **iOS:** **`BidscubeSDKAppLovin`** + **`AppLovinSDK`** **13.x** (see [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md)) |

---

## Integration modes

| Mode | `SDKConfig.Builder` | Ads from Unity C# |
|------|---------------------|-------------------|
| **BidsCube SDK** | `IntegrationMode(BidscubeIntegrationMode.DirectSdk)` (default) | **Yes** — `GetBannerAdView`, `ShowVideoAd`, etc. |
| **AppLovin MAX** | `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` | **No** — use MAX only; C# creative APIs throw |

Wire strings (JSON / `PlayerPrefs`): `direct`, `directSdk`, `appLovinMax`, and legacy `levelPlay` / `level_play` → MAX (see `BidscubeIntegrationModeWire` in code).

---

## AppLovin MAX (mediation)

Mediation is implemented in **native** Bidscube adapters for MAX, not via Unity `UnitySendMessage`. **Android:** call **`BidscubeSDK.Initialize`** early with **`AppLovinMaxMediation`** so the Java SDK shares **`AdRequestAuthority`** and options with the bundled adapter. **iOS:** the **`BidscubeSDKAppLovin`** adapter can initialize BidCube internally ([native guide](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS)); use C# **`Initialize`** when you want the same Unity-driven **`SDKConfig`** before MAX, or set **`request_authority` / `ssp_host`** in MAX server parameters.

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
2. **Bidscube as a mediated network** — Under **Mediation / Networks**, add **Bidscube** as a **custom SDK network**. Enable it on each ad unit. **Android:** adapter class **`com.applovin.mediation.adapters.BidscubeMediationAdapter`** (bundled AAR). **iOS:** **`ALBidscubeMediationAdapter`** (exact name). **App ID** in MAX must be the **BidCube placement ID** (MAX still calls this field “App ID”). **Placement ID** is optional unless your setup needs a second value. Optional server parameters: **`request_authority`** or **`ssp_host`** for SSP host / `host:port`. **Android:** call **`BidscubeSDK.Initialize` before `MaxSdk.InitializeSdk`** so native config matches Unity when you use C# **`AdRequestAuthority`**. **iOS:** adapter-only init is supported per [AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS).

| Item | Where | Note |
|------|--------|------|
| **SDK Key** | AppLovin MAX | MAX SDK initialization |
| **Ad unit IDs** | MAX dashboard | Load/show APIs |
| **Custom network** | MAX mediation | Adapter class (platform-specific) |
| **Android** | Network setup | `com.applovin.mediation.adapters.BidscubeMediationAdapter` |
| **iOS** | Network setup | `ALBidscubeMediationAdapter` |
| **App ID** (custom network) | MAX UI | **BidCube placement ID** |
| **Server params** | Network / instance | `request_authority`, `ssp_host` (optional) |

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
| `Runtime/Plugins/Android/` | **`applovin-bidscube-max-adapter-1.0.3.aar`** (+ WebView templates); core SDK via injected Maven coordinate |
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
| **Duplicate class / DEX `com.bidscube.sdk`** | Remove any extra `implementation 'com.bidscube:bidscube-sdk:…'` or local AAR — the post-processor already injects one Maven line. |
| **Ads not loading** | `BaseURL`, placement IDs, device logs, network |
| **MAX mode** | `IntegrationMode` is **AppLovinMaxMediation**, adapter + MAX SDK versions, dashboard network setup |
| **Wrong mode behavior** | Direct APIs used in MAX mode → `InvalidOperationException`; use MAX only |

---

## Releasing (maintainers)

Tags **`v*`** must match [`package.json`](package.json) `version`. See [`RELEASE.md`](RELEASE.md) and run [`tools/verify-release-ready.sh`](tools/verify-release-ready.sh) before tagging.

**GitHub:** workflows live under [`.github/workflows/`](.github/workflows/) — push tag **`v*`** or run **[Release (GitHub)](.github/workflows/release.yml)** manually from the **Actions** tab (requires `.github` on the default branch). CI: [`ci.yml`](.github/workflows/ci.yml).

---

## Changelog & license

- **Changelog:** [`CHANGELOG.md`](CHANGELOG.md)  
- **License:** [`LICENSE.md`](LICENSE.md)
