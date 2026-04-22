# Video playback: Direct SDK (Unity) vs AppLovin MAX (Android native)

Two integration surfaces decide **where** video runs and **how** you plug a custom player.

---

## 1. Direct SDK — Unity / C# creatives

**When:** `BidscubeIntegrationMode.DirectSdk` (default). You use `ShowVideoAd`, `GetVideoAdView`, banners, native, etc. from C#.

### Default (no custom Unity player)

1. Build `SDKConfig` and call `BidscubeSDK.Initialize(config)` (at least `AdRequestAuthority` / logging flags as needed).
2. **Do not** set `SDKConfig.Builder.VideoPlaybackFactory` and **do not** assign `VideoAdView.VideoPlaybackFactory`.

**Behaviour:** For linear VAST / direct MP4, `VideoAdView` uses **Google IMA** when available for that path, and/or **`UnityEngine.Video.VideoPlayer`** on the SDK `RawImage`, depending on the resolved path. No extra “player” wiring.

**Smaller builds:** Scripting define **`BIDSCUBE_DISABLE_UNITY_VIDEO`** drops `UnityEngine.VideoModule`; you **must** then supply `VideoPlaybackFactory` returning `IVideoSurfacePlayback` for that linear path. See **`INTEGRATION.md`** (log verification) and **`IVideoSurfacePlayback`**.

### Custom Unity player

1. Implement **`IVideoSurfacePlayback`** (`Prepare` / `Play` / `Stop`, `SourceUrl`, `BindToRawImage`, `Prepared` / `Started` / `Completed`) — e.g. AVPro, your own texture, etc.
2. Register the factory **before** `Initialize` (recommended):

   `new SDKConfig.Builder().VideoPlaybackFactory((go, rawImage) => { … return playback; })`

   Inside the lambda: `AddComponent<MyPlayback>()`, call **`BindToRawImage(rawImage)`** on your instance, then return it.

3. Fallback: static **`VideoAdView.VideoPlaybackFactory`** (tests or pre-`Initialize` wiring). Resolution order: **`BidscubeSDK.ActiveConfiguration.VideoPlaybackFactory`** → static → built-in Unity player (unless `BIDSCUBE_DISABLE_UNITY_VIDEO`).

**Scope:** Unity only. This does **not** replace Java `VideoView` inside native `com.bidscube.sdk` unless you call that stack yourself.

---

## 2. AppLovin MAX — native Android SDK

**When:** `BidscubeIntegrationMode.AppLovinMaxMediation`. Ads load/show through **MAX**; C# Bidscube creative APIs **throw**.

1. On **Android**, call **`BidscubeSDK.Initialize(config)`** **before** **`MaxSdk.InitializeSdk`** so Java `BidscubeSDK` shares `AdRequestAuthority`, test mode, and logging with the bundled adapter (see **`APPLOVIN_MAX.md`**).

**Behaviour:** Rewarded / interstitial **video** on the native path uses the **published** `com.bidscube.sdk` stack (IMA + fullscreen video surface — e.g. `VideoView` / adapters as shipped in that AAR version). Unity **`VideoAdView`** / **`IVideoSurfacePlayback`** are **not** used for MAX-mediated video.

### Default (no custom native `VideoView` factory)

Do **not** set a native `VideoPlayerProvider` (or equivalent) on the Java `SDKConfig.Builder`. Rely on **C# `Initialize`** (and/or adapter + MAX server params per **`APPLOVIN_MAX.md`**) so the SDK uses its default surface.

### Custom native `VideoView` (Java / Kotlin)

When your pinned **`bidscube-sdk-*.aar`** documents a **`VideoPlayerProvider`** (or factory that supplies a **`VideoView`** for the IMA path), pass it on the **Java** `SDKConfig.Builder`, e.g. **`videoPlayerProvider(context -> new MyImmersiveVideoView(context))`**, on the **first** successful **`BidscubeSDK.initialize(applicationContext, config)`**. A second init is often ignored — treat **first** init as authoritative.

**Practical pattern:** Own **`Application`** or main Android module calls Java **`BidscubeSDK.initialize(...)`** with that builder **before** MAX / adapter “win” with an empty default; afterwards Unity may still call C# **`Initialize`** to sync **activity** when the Java SDK is already initialized.

**Unity UPM:** This package **does not** expose `videoPlayerProvider` on **`SDKConfig.Builder`** in C#. Custom native playback is wired **only** in Android (Gradle export, `mainTemplate`, or an Android library). Confirm exact setter names on **`com.bidscube.sdk`** for your AAR version.

### Android export: `LiteNoVideo` vs `FullWithVideo` (Gradle)

**Resolution order:** `BidscubeAndroidExportSettings` asset in the project (if any) → else `BidscubeAndroidGradlePostprocessor.FeatureSet` (`BidscubeAndroidFeatureSet`). Prefer committing an export settings asset for GitHub / CI parity.

| `FeatureSet` | Core AAR copied to `unityLibrary/libs/` | Injected Maven (besides AppLovin / UMP / Glide / Material / …) |
|----------------|----------------------------------------|------------------------------------------------------------------|
| **`LiteNoVideo`** (default) | `bidscube-sdk-lite-<version>.aar` | **No** `androidx.media3:*`, **no** `com.google.ads.interactivemedia.v3:interactivemedia` |
| **`FullWithVideo`** | `bidscube-sdk-<version>.aar` | **Yes** Media3 **1.4.1** + Google IMA **3.33.0** |

Default **core** resolution remains **`BundledUnityLibraryLibsAar`** (`implementation files('libs/…')`) — **no Maven Central required for the core artifact.**  
Android player builds set **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`** when `FeatureSet == LiteNoVideo` so **Direct SDK** C# APIs `ShowVideoAd` / `GetVideoAdView` fail fast with a clear log (banner / native / image flows unchanged).

**MAX:** rewarded / interstitial **video** from the native adapter expects the **full** SDK + IMA stack. Use **`FullWithVideo`** for MAX units that show **video**; **`LiteNoVideo`** is aimed at **banner / native / image**-heavy integrations where APK size matters (confirm behaviour with your lite AAR).

---

## Quick comparison

| | **No custom player** | **Custom player** |
|--|----------------------|---------------------|
| **Direct (Unity)** | Omit `VideoPlaybackFactory` | `SDKConfig.Builder.VideoPlaybackFactory(...)` or `VideoAdView.VideoPlaybackFactory` + `IVideoSurfacePlayback` |
| **MAX (Android native)** | Omit Java `videoPlayerProvider` / `VideoPlayerProvider` | First Java `BidscubeSDK.initialize` with `SDKConfig.Builder` that sets the provider (per native SDK docs) |

---

## See also

- **`INTEGRATION.md`** — init, logs, `BIDSCUBE_DISABLE_UNITY_VIDEO`, full C# API.
- **`APPLOVIN_MAX.md`** — MAX dashboard, adapter class names, startup order.
- **`ANDROID_BUNDLED_SDK.md`** — AAR, Gradle, one core on classpath.
