# Bidscube SDK for Unity (`com.bidscube.sdk`)

**Package (UPM):** `1.0.11` · **Git tag:** `v1.0.11` · **Android core (default lite: `libs/bidscube-sdk-lite-1.2.3.aar`; full: `bidscube-sdk-1.2.3.aar`; optional Maven `…@aar`):** `1.2.3` · **Android MAX adapter AAR:** `1.0.4` · **iOS CocoaPods `BidscubeSDKAppLovin`:** `1.0.4`

UPM package for **Bidscube** ads in Unity: **BidsCube SDK** mode (Unity/C# drives banners, video, native, interstitials) and **AppLovin MAX mediation** (MAX drives load/show via the Bidscube adapter; **Android:** early C# **`BidscubeSDK.Initialize`** recommended; **iOS:** optional — adapter can init native BidCube — see [`APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md)).

### What’s new in **1.0.11**

| Topic | Summary |
|--------|---------|
| **Release** | UPM / tag **1.0.11** — see **[CHANGELOG.md](CHANGELOG.md)** (**1.0.10** packaging and Editor fixes remain in that section). |

### What’s new in **1.0.8**

| Topic | Summary |
|--------|---------|
| **Android Lite vs Full** | **`BidscubeAndroidFeatureSet`**, dual bundled core AARs, conditional Media3/IMA Gradle lines; **`BidscubeAndroidExportSettings`** asset for teams / Git; Direct SDK video gated on lite builds — **`ANDROID_BUNDLED_SDK.md`**, **`VIDEO_PLAYBACK.md`**. |

### What’s new in **1.0.7**

| Topic | Summary |
|--------|---------|
| **Android default core** | **`BundledUnityLibraryLibsAar`** — export copies **`bidscube-sdk-*.aar`** to **`unityLibrary/libs/`** and injects **`files('libs/…')`**; opt back to Maven with **`MavenBidscubeSdkAar`** — **`ANDROID_BUNDLED_SDK.md`**. |
| **Release ZIP** | **`release.yml`** excludes **`__MACOSX/*`**, **`*.DS_Store`**, **`.git/*`**. |
| **Tooling** | **`verify-release-ready.sh`** requires the bundled core AAR next to **`NativeAndroidBidscubeSdkVersion`**. |

### What’s new in **1.0.6**

| Topic | Summary |
|--------|---------|
| **Android Gradle** | **Launcher** **`coreLibraryDesugaring`** / **`coreLibraryDesugaringEnabled`** injected by default (**`NoDesugarMode = true`** skips). **`BidscubeAndroidCoreDependencyMode`** (default then: **`MavenBidscubeSdkAar`** + **`…@aar`**). |
| **Docs / MAX** | Direct vs MAX playback clarified (**`APPLOVIN_MAX.md`**, **`INTEGRATION.md`**). |
| **Diagnostics** | Init publisher row + **`ClassNotFoundException`** guidance (AAR vs POM). |

### What’s new in **1.0.5**

| Topic | Summary |
|--------|---------|
| **UPM `1.0.5`** | Semver **`1.0.5`** in [`package.json`](package.json); release tag **`v1.0.5`** — see [`RELEASE.md`](RELEASE.md). |
| **1.0.4 baseline** | Init publisher row, VideoAdView logging, Direct SDK video fixes — see **`[1.0.4]`** in [`CHANGELOG.md`](CHANGELOG.md). |

Full list: [`CHANGELOG.md`](CHANGELOG.md).

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
    "com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.11"
  }
}
```

Or **Package Manager → Add package from git URL** with the same URL (replace **org / repo / tag** with your fork and release; tag example **`v1.0.8`**).

**Unity UI dependencies:** `com.unity.ugui` and `com.unity.textmeshpro` are listed in [`package.json`](package.json) and resolve with the package from Git URL. The Editor may run **`BidscubeUpmDependencyInstaller`** once per session to align those dependencies — same idea as the [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) README (“no manual setup”).

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
| **Android** | Bundled **`applovin-bidscube-max-adapter-1.0.4.aar`**; **two** core AARs (lite + full). Export mode: commit **`BidscubeAndroidExportSettings`** (*Assets → Create → Bidscube → Android Export Settings*) or set **`BidscubeAndroidGradlePostprocessor.FeatureSet`**. See **`ANDROID_BUNDLED_SDK.md`**, **`VIDEO_PLAYBACK.md`**. |
| **iOS** | WebView plugins under `Runtime/Plugins/iOS/`; MAX: CocoaPods **`BidscubeSDKAppLovin` `1.0.4`** + **`AppLovinSDK` `13.x`** ([iOS repo](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS)); **`BidscubeIosPodfilePostprocessor`** can append pods to the exported **Podfile** |
| **Legacy configs** | Wire values `levelPlay` / `level_play` → **AppLovin MAX mediation** (same idea as Flutter) |

---

## Requirements

| Requirement | Details |
|-------------|---------|
| **Unity** | **2020.3+** (see [`package.json`](package.json) `unity`) |
| **UPM dependencies** | `com.unity.ugui`, `com.unity.textmeshpro` (declared in `package.json`) |
| **Android** | **Minimum API Level ≥ 26** recommended with the injected dependency set; **compileSdk** raised by post-processor as needed — [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md) |
| **MAX mediation** | **AppLovin MAX** (Unity plugin or native). **Android:** adapter AAR **1.0.4** bundled. **iOS:** **`BidscubeSDKAppLovin` `1.0.4`** + **`AppLovinSDK` `13.x`** (see [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md)) |

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
| `Runtime/Plugins/Android/` | **`applovin-bidscube-max-adapter-1.0.4.aar`** (Android on); **`bidscube-sdk-1.2.3.aar`** + **`bidscube-sdk-lite-1.2.3.aar`** (Android off in **`.meta`** — post-processor copies the chosen flavor to **`unityLibrary/libs/`**); WebView templates |
| `Runtime/Plugins/iOS/` | WebView native plugins |
| `Editor/Android/` | **`BidscubeAndroidGradlePostprocessor`** |
| `Documentation~/` | Markdown docs (this package) |
| `Samples~/SDK Demo/` | Example scenes (import via **Samples** in Package Manager) |

---

## Documentation

| Doc | Purpose |
|-----|---------|
| [`Documentation~/VIDEO_PLAYBACK.md`](Documentation~/VIDEO_PLAYBACK.md) | **Direct vs MAX:** default vs custom player (Unity `IVideoSurfacePlayback` vs native Java provider) |
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
| **Android Gradle / AAR metadata errors** | **minSdk ≥ 26**, clean Gradle export; see [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md). **UPM ≥ 1.0.8:** bundled lite/full core + **launcher** desugaring from the post-processor. If you duplicate lines, set **`BidscubeAndroidGradlePostprocessor.NoDesugarMode = true`** and own desugaring in Custom Launcher Gradle. |
| **Duplicate class / DEX `com.bidscube.sdk`** | Remove any extra core line (second Maven **`@aar`**, duplicate **`files(...)`**, etc.) — keep a single core source; see **`CoreDependencyMode`** in **`ANDROID_BUNDLED_SDK.md`**. |
| **`ClassNotFoundException` / `com.bidscube.sdk.BidscubeSDK`** | Ensure **`unityLibrary/build.gradle`** declares core once (default **`files('libs/bidscube-sdk-<ver>.aar')`** after export, or **`MavenBidscubeSdkAar`** with **`…@aar`**). Do not add a conflicting second line in Custom Gradle. |
| **Ads not loading** | `BaseURL`, placement IDs, device logs, network |
| **MAX mode** | `IntegrationMode` is **AppLovinMaxMediation**, adapter + MAX SDK versions, dashboard network setup |
| **MAX vs Direct SDK** | In MAX mode, load and show ads through MAX only; use **`BidscubeIntegrationMode.DirectSdk`** for C# `ShowVideoAd` / banner APIs. |

---

## Releasing (maintainers)

Tags **`v*`** match [`package.json`](package.json) `version`. Process and checklist: [`RELEASE.md`](RELEASE.md). Version alignment helper: [`tools/verify-release-ready.sh`](tools/verify-release-ready.sh).

**GitHub:** workflows live under [`.github/workflows/`](.github/workflows/) — push tag **`v*`** or run **[Release (GitHub)](.github/workflows/release.yml)** manually from the **Actions** tab (requires `.github` on the default branch). CI: [`ci.yml`](.github/workflows/ci.yml).

---

## Changelog & license

- **Changelog:** [`CHANGELOG.md`](CHANGELOG.md)  
- **License:** [`LICENSE.md`](LICENSE.md)
