# Bidscube AppLovin MAX adapter (`com.bidscube.applovin.max`)

**Package (UPM):** `com.bidscube.applovin.max` **1.0.16** · **Git tag:** `v1.0.16` · **Core UPM peer:** `com.bidscube.sdk` **1.2.6** (from `package.json` → `dependencies`) · **Android bundled:** **MAX** adapter **1.0.4** · **lite** core AAR **1.2.3** · **iOS:** **`BidscubeSDKAppLovin`** **1.0.4** · **AppLovin** native **13.x**

**Companion** package for **Bidscube** + **AppLovin MAX**: Android **MAX** adapter AAR, **lite** core AAR (reference), and **`AppLovinMaxUnityReflection`** so C# can call **`MaxSdk`** when the official AppLovin plugin is present. **You also add** **`com.bidscube.sdk`** (core Unity SDK) — this repo does not ship the full C# runtime anymore.

### AppLovin MAX Unity SDK — потрібен окремо

Цей UPM-пакет **не містить** AppLovin MAX Unity Plugin (нема збірки з класом **`MaxSdk`**, яку шукає reflection). Його потрібно підключити **окремо** з дистрибутива AppLovin (Unity Package Manager або `.unitypackage` з їхнього сайту / Git integration). Тоді **`AppLovinMaxUnityReflection.IsMaxSdkAvailable`** стане `true`, і викличеться **`MaxSdk.InitializeSdk`**.

- Залежність у `manifest`: лиш **`com.bidscube.sdk`** + **`com.bidscube.applovin.max`**; пакета **`com.applovin.mediation.ads`** (офіційний MAX) немає в `package.json` цього адаптера — його додає видавець у свій проєкт вручну.

Докладніше про порядок ініціалізації (**Android:** `BidscubeSDK.Initialize` перед `MaxSdk.InitializeSdk`): [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md).

## Android modes (`LiteNoVideo` / `FullWithVideo`)

**Default (1.0.16+):** **`BidscubeAndroidFeatureSet.LiteNoVideo`** — the postprocessor copies only **`bidscube-sdk-lite-1.2.3.aar`** to **`unityLibrary/libs/`** and injects **no** **`com.bidscube:bidscube-sdk`**, **no** Media3, **no** Google IMA. Sets **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`** on Android; **banner / native / image** work; direct video APIs fail gracefully.

**`FullWithVideo`:** requires **`Runtime/Plugins/Android/bidscube-sdk-1.2.3.aar`** (vendor from native SDK build) **or** **`BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar`** with a reachable **`com.bidscube:bidscube-sdk:1.2.3@aar`**. Then Gradle adds **Media3** + **Google IMA** and does **not** define **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`**. Without the full AAR / Maven repo, Android export logs an **error** — use **LiteNoVideo** for demo/CI without those artifacts.

**`com.bidscube.sdk`** should use **`#if BIDSCUBE_ANDROID_LITE_NO_VIDEO`** (or runtime checks) for **`ShowVideoAd`**, **`GetVideoAdView`**, etc., matching this package’s define and Gradle wiring.

### How to change mode in the Editor

1. **Assets → Create → Bidscube → Android Export Settings** (ScriptableObject) and set **`featureSet`**, **or** open **Tools → Bidscube SDK → Android Build Features** and toggle **FullWithVideo** vs **LiteNoVideo**.
2. Re-export / rebuild Android — defines and **`IPostGenerateGradleAndroidProject`** follow the selected mode.

### What’s new in **1.0.16**

| Topic | Summary |
|--------|---------|
| **Android / docs** | **`enableDesugaring`** control + Gradle strip with **warning** when off (bundled AAR metadata usually requires desugaring); peer **`com.bidscube.sdk` 1.2.6** — see [CHANGELOG](CHANGELOG.md). |

### What’s new in **1.0.15**

| Topic | Summary |
|--------|---------|
| **Hotfix** | Editor **`.meta`** files for scripts/tools; **LiteNoVideo** default; **FullWithVideo** no longer injects unresolved Maven core — see [CHANGELOG](CHANGELOG.md). |

### What’s new in **1.0.14**

| Topic | Summary |
|--------|---------|
| **Release** | UPM / tag **1.0.14** — [CHANGELOG](CHANGELOG.md). |
| **Android default** | **`FullWithVideo`** was default in 1.0.14 (superseded by **1.0.15**). |

### What’s new in **1.0.13**

| Topic | Summary |
|--------|---------|
| **Release** | UPM / tag **1.0.13** — [CHANGELOG](CHANGELOG.md). |
| **Android Full / Lite** | **`BidscubeAndroidFeatureSet`**, **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`**, **Tools** menu + **`BidscubeAndroidExportSettings`**, **`BidscubeAndroidGradlePostprocessor`** (one core AAR + conditional IMA/Media3). |

### What’s new in **1.0.12**

| Topic | Summary |
|--------|---------|
| **Scope / docs** | Companion **`AppLovinMaxUnityReflection`** + AARs; **`AdapterPackageInfo`** / version matrix — see [CHANGELOG](CHANGELOG.md) **[1.0.12]**. |

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
| **Android Gradle** | **BidscubeAndroidExportSettings** → **Enable Desugaring** stays **on** for bundled lite/full core (AAR metadata); uncheck only if your core does not require desugaring — post-processor strips **`coreLibraryDesugaring`** when off. Default **`BundledUnityLibraryLibsAar`** + **`LiteNoVideo`**. |
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
- [Android modes](#android-modes-fullwithvideo--litenovideo)
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

**Рекомендовано для продакшену:** обидва пакети з **GitHub** (UPM git), з **закріпленими тегами** — без локальних `file:` шляхів.

Додайте в **`Packages/manifest.json`** проєкту **core** і **адаптер** так:

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.6",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.16"
  }
}
```

Версії **`#v…`** мають збігатися з релізними тегами на GitHub і з очікуваним peer (**`com.bidscube.sdk`** у [`package.json`](package.json) → `dependencies`). Чистий semver на кшталт `"com.bidscube.sdk": "1.2.6"` **без** scoped registry зазвичай **не** підтягне core з публічного Unity Registry — тому для інтеграторів документований шлях саме **git URL**.

Через **Package Manager** (`+` → **Add package from git URL…**) можна додати спочатку core, потім адаптер — або одразу правити **`manifest.json`** як вище.

**Короткий чеклист MAX + Bidscube (англ.):** [`Documentation~/APPLOVIN_MEDIATION_STEPS.md`](Documentation~/APPLOVIN_MEDIATION_STEPS.md) — пакети, Lite/Full, порядок ініціалізації, dashboard.

**Окремо** установіть **офіційний AppLovin MAX Unity SDK** за інструкцією AppLovin (UPM або `.unitypackage`). Без нього в рантаймі не зʼявиться **`MaxSdk`**, і `AppLovinMaxUnityReflection` лишиться «порожнім».

**Unity UI** (for **Samples** or your UI): add **`com.unity.ugui`** / **`com.unity.textmeshpro`** in the **host** project if needed — this adapter **`package.json`** only declares **`com.bidscube.sdk`**.

**Лише для розробки в монорепо (мейнтейнери):** можна тимчасово вказати адаптер через `file:…` відносно вашого клону — не використовуйте це в шаблонах для видавців або CI, щоб залежності завжди збігалися з тегами на GitHub.

Package metadata: [`package.json`](package.json) (`displayName`: **Bidscube AppLovin MAX Adapter**).

---

## Features

| Area | Behavior |
|------|----------|
| **BidsCube SDK** (from **`com.bidscube.sdk`**) | C# APIs: `GetBannerAdView`, `GetVideoAdView`, init, etc. — see that package. |
| **AppLovin MAX** | `BidscubeIntegrationMode.AppLovinMaxMediation` — `BidscubeSDK.Initialize` then MAX; creatives through MAX + Bidscube adapter. |
| **Android** | **`BidscubeAndroidGradlePostprocessor`**: **default LiteNoVideo** → bundled **`bidscube-sdk-lite-*.aar`**, no Maven core / no Media3/IMA; **FullWithVideo** → **`bidscube-sdk-*.aar`** (or **MavenBidscubeSdkAar**) + **Media3/IMA**. Menu: **Tools → Bidscube SDK → Android Build Features**. |
| **iOS** | MAX: CocoaPods **`BidscubeSDKAppLovin` `1.0.4`** + **`AppLovinSDK` `13.x`** — add to **Podfile** manually or via your own post-build script. |
| **Legacy configs** | Wire values `levelPlay` / `level_play` → **AppLovin MAX mediation** (same idea as Flutter) |

---

## Requirements

| Requirement | Details |
|-------------|---------|
| **Unity** | **2020.3+** (see [`package.json`](package.json) `unity`) |
| **UPM dependencies** | `com.unity.ugui`, `com.unity.textmeshpro` (declared in `package.json`) |
| **Android** | **Minimum API Level ≥ 26**; align **compileSdk** / Gradle with your templates and `ANDROID_BUNDLED_SDK.md` |
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
| `Runtime/BidscubeSDK/Android/` | **`BidscubeAndroidFeatureSet`**, **`BidscubeAndroidExportSettings`**, **`BidscubeLiteVideoGuard`**, **`AndroidBuildDefines`** (`BIDSCUBE_ANDROID_LITE_NO_VIDEO`) |
| `Runtime/BidscubeSDK/Mediation/` | **`AppLovinMaxUnityReflection`** (MAX via reflection) |
| `Runtime/BidscubeSDK/Properties/` | **`AdapterPackageInfo`** (UPM + native version strings for checks) |
| `Runtime/Plugins/Android/` | **`applovin-bidscube-max-adapter-1.0.4.aar`**, **`bidscube-sdk-lite-1.2.3.aar`**; optional **`bidscube-sdk-1.2.3.aar`** for offline **FullWithVideo** |
| `Editor/` | Scripting defines sync, **Tools** menu, **`BidscubeAndroidGradlePostprocessor`** |
| `Documentation~/` | Markdown (MAX + Android layout; C# API lives with **`com.bidscube.sdk`**) |
| `Samples~/SDK Demo/` | Demo scenes (require **`com.bidscube.sdk`**) — import via **Samples**; video UI gated by **`#if !BIDSCUBE_ANDROID_LITE_NO_VIDEO`** |

---

## Documentation

| Doc | Purpose |
|-----|---------|
| [`Documentation~/APPLOVIN_MEDIATION_STEPS.md`](Documentation~/APPLOVIN_MEDIATION_STEPS.md) | **Start here:** packages, Lite/Full, init order, dashboard, optional desugaring off |
| [`Documentation~/VIDEO_PLAYBACK.md`](Documentation~/VIDEO_PLAYBACK.md) | **Direct vs MAX:** default vs custom player (Unity `IVideoSurfacePlayback` vs native Java provider) |
| [`Documentation~/INTEGRATION.md`](Documentation~/INTEGRATION.md) | BidsCube SDK usage, configuration, callbacks |
| [`Documentation~/AD_REQUEST_ENDPOINT.md`](Documentation~/AD_REQUEST_ENDPOINT.md) | **`AdRequestAuthority`**, HTTPS `/sdk` base URL, query params by ad type, SSP response |
| [`Documentation~/APPLOVIN_MAX.md`](Documentation~/APPLOVIN_MAX.md) | MAX mediation, native flow, what not to call from C# |
| [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md) | AAR, Gradle injection, `CheckAarMetadata` / compileSdk |
| [`Documentation~/TEST_PLAN.md`](Documentation~/TEST_PLAN.md) | Minimal QA checklist |

---

## Samples

In **Package Manager**, open **`com.bidscube.applovin.max`** → **Samples** → import **SDK Demo** (scenes/scripts; they reference **`com.bidscube.sdk`**).

---

## Troubleshooting

| Issue | What to check |
|-------|----------------|
| **Android Gradle / AAR metadata errors** | **minSdk ≥ 26**, single **`com.bidscube:bidscube-sdk`** / **`files('libs/…')`** line — see [`Documentation~/ANDROID_BUNDLED_SDK.md`](Documentation~/ANDROID_BUNDLED_SDK.md). Bundled **`bidscube-sdk-lite` / `bidscube-sdk`** AARs typically **require** launcher desugaring — keep **Enable Desugaring** on **`BidscubeAndroidExportSettings`** or **`:launcher:checkReleaseAarMetadata`** fails. |
| **Duplicate class / DEX `com.bidscube.sdk`** | Remove any extra core line (second Maven **`@aar`**, duplicate **`files(...)`**, Unity auto-merge + **`libs/`** copy, etc.). For **`BidscubeAndroidGradlePostprocessor`**, turn **Android** import **off** on **`bidscube-sdk-*.aar`** in the Inspector so Unity does not merge them twice (postprocessor copies into **`unityLibrary/libs/`**). |
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
