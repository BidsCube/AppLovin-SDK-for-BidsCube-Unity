## [Unreleased]

---

## [1.0.8] - 2026-04-23

### Added

- **Android:** **`BidscubeAndroidFeatureSet`** (Runtime) — **`LiteNoVideo`** (default) vs **`FullWithVideo`**. **`BidscubeAndroidExportSettings`** ScriptableObject (optional, commit to Git) overrides **`BidscubeAndroidGradlePostprocessor.FeatureSet`** for Gradle + scripting defines. Post-processor copies **`bidscube-sdk-lite-*.aar`** vs **`bidscube-sdk-*.aar`** and skips Media3 / IMA in lite mode. **`BidscubeAndroidScriptingDefinesPreprocessor`**, **`BidscubeAndroidBuildFeatures`**, **`ShowVideoAd` / `GetVideoAdView`** guards.

### Changed

- **Docs:** **`VIDEO_PLAYBACK.md`**, **`INTEGRATION.md`**, **`APPLOVIN_MAX.md`**, **`README.md`**, **`ANDROID_BUNDLED_SDK.md`** — **`FeatureSet`**, dual core AARs, MAX video vs lite export, client Git workflow.
- **`Constants.SdkVersion`** / **`package.json`** → **1.0.8**.

---

## [1.0.7] - 2026-04-22

### Added

- **Android:** **`BidscubeAndroidCoreDependencyMode.BundledUnityLibraryLibsAar`** — on Gradle export, copies **`bidscube-sdk-<ver>.aar`** from the UPM package into **`unityLibrary/libs/`** and injects **`implementation files('libs/…')`** so the core SDK does not require a Maven repository.

### Changed

- **Android (default):** **`BidscubeAndroidGradlePostprocessor.CoreDependencyMode`** is now **`BundledUnityLibraryLibsAar`** (was **`MavenBidscubeSdkAar`**). To keep resolving the core from Gradle repos only, set **`CoreDependencyMode = MavenBidscubeSdkAar`** before export (**`ANDROID_BUNDLED_SDK.md`**).
- **Android:** **`Runtime/Plugins/Android/bidscube-sdk-1.2.3.aar`** — **PluginImporter** leaves **Android disabled** so Unity does not merge the same AAR twice; the post-processor owns the copy into **`unityLibrary/libs/`**.
- **Release:** GitHub Actions ZIP excludes **`__MACOSX/*`** and **`*.DS_Store`** in addition to **`.git/*`**.
- **`Constants.SdkVersion`** / **`package.json`** → **1.0.7**.

---

## [1.0.6] - 2026-04-21

### Added

- **Android:** **`BidscubeAndroidCoreDependencyMode`** on **`BidscubeAndroidGradlePostprocessor`** — default **`MavenBidscubeSdkAar`**; optional **`CustomGradleLines`** / **`SkipInjectionIntegratorOwnsCore`** for local **`.aar`**, **`project(...)`**, integrator-owned core, or internal Maven — see **`Documentation~/ANDROID_BUNDLED_SDK.md`**.

### Fixed

- **Android:** **`BidscubeAndroidGradlePostprocessor`** injects **launcher** **`coreLibraryDesugaringEnabled`** and **`coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.1.4'`** (idempotent) so **`CheckAarMetadata`** succeeds when **`com.bidscube:bidscube-sdk`** requires desugaring on **`:launcher`**. Set **`NoDesugarMode = true`** to skip if host Gradle already owns desugaring.
- **Android:** Gradle post-processor injects **`com.bidscube:bidscube-sdk:<version>@aar`** so the core resolves as an **AAR** (avoids POM-only **`packaging=pom`** issues). Legacy lines without **`@aar`** are normalized on export; missing **`@aar`** logs an Editor **error**.

### Changed

- **`BidscubeAndroidGradlePostprocessor.NoDesugarMode`** defaults to **`false`** (plugin injects launcher desugaring). **`true`** skips injection and logs an export **warning**.
- **Android:** Publisher init log mentions **`CoreDependencyMode`**; validation renamed to **`ValidateCoreBidscubeSdkDependency`** (**`TEST_PLAN.md`**).
- **Docs:** **`ANDROID_BUNDLED_SDK.md`** — **`CoreDependencyMode`** scope vs AppLovin/Media3 transitives; **`APPLOVIN_MAX.md`** / **`INTEGRATION.md`** — Direct vs MAX playback (custom **`IVideoSurfacePlayback`** only in Direct; MAX uses default native playback for the pinned **`com.bidscube:bidscube-sdk`**).
- **`Constants.SdkVersion`** / **`package.json`** → **1.0.6**. Bundled **`applovin-bidscube-max-adapter-1.0.4.aar`**; iOS CocoaPods **`BidscubeSDKAppLovin`** pin **1.0.4**; native core Maven pin **`NativeAndroidBidscubeSdkVersion`** **1.2.3** (default **`com.bidscube:bidscube-sdk:1.2.3@aar`** with MAX adapter **1.0.4**).

---

## [1.0.5] - 2026-04-21

### Fixed

- **Android:** Gradle post-processor injects **`com.bidscube:bidscube-sdk:<version>@aar`** so the core SDK resolves as an **AAR** (avoids POM-only resolution when the Maven coordinate uses root **`packaging=pom`**). Legacy exports without **`@aar`** are rewritten on the next export; missing **`@aar`** after rewrite logs a **clear Editor error**.
- **Android:** **`BidscubeAndroidSdkInterop`** / publisher checklist messages for **`ClassNotFoundException`** now point integrators at **AAR vs POM** resolution.

### Changed

- **Android (breaking vs prior post-processor):** the plugin **never** injects or edits **`coreLibraryDesugaring`**, **`desugar_jdk_libs`**, **`coreLibraryDesugaringEnabled`**, or launcher desugaring (no automatic desugaring; host Gradle owns it). **`BidscubeAndroidGradlePostprocessor.NoDesugarMode`** defaults to **`true`** and only controls an Editor **warning** on each Android export reminding integrators to add desugaring in host Gradle when needed; set **`NoDesugarMode = false`** to suppress that warning after your templates are configured.
- **`Constants.SdkVersion`** / **`package.json`** → **1.0.5**. Bundled Android MAX adapter AAR remains **`applovin-bidscube-max-adapter-1.0.4.aar`**; iOS CocoaPods **`BidscubeSDKAppLovin`** pin remains **1.0.4** until a matching native release bumps them.

---

## [1.0.4] - 2026-04-15

### Added

- **Init diagnostics (publishers / QA):** after each **`BidscubeSDK.Initialize`**, Unity logs **`Init (publisher row):`** — one line with UPM version, expected **`com.bidscube:bidscube-sdk`** Maven pin, **`integrationMode`**, C# init flag, and Android Java sync outcome (**`AndroidJava=OK`**, **`JAR_MISSING`**, skipped activity, etc.). Android interop exposes **`FormatPublisherChecklistLine()`** state machine for that suffix. Filter logcat with **`[BidscubeSDK] Init`**.
- **`[VideoAdView]`** logs which **linear** playback backend is active: built-in **`UnityEngine.Video.VideoPlayer`** vs **`IVideoSurfacePlayback`** from **`VideoPlaybackFactory`** / **`SDKConfig.Builder.VideoPlaybackFactory`**; reminds about **`BIDSCUBE_DISABLE_UNITY_VIDEO`** for smaller builds when a custom factory is registered.

### Changed

- **Android:** removed bundled **`bidscube-sdk-*.aar`**; **`BidscubeAndroidGradlePostprocessor`** injects **`implementation 'com.bidscube:bidscube-sdk:<NativeAndroidBidscubeSdkVersion>'`** (Maven Central). Only **`applovin-bidscube-max-adapter-*.aar`** ships in `Runtime/Plugins/Android/`. Migration: existing `unityLibrary/build.gradle` exports that already have our marker but no Maven core line get the coordinate appended automatically.
- **`Constants.SdkVersion`** / **`package.json`** → **1.0.4** (Unity Package Manager rejects four-part versions like `1.0.3.1`; use **1.0.4** instead). Bundled Android MAX adapter AAR **`applovin-bidscube-max-adapter-1.0.4.aar`**; iOS CocoaPods **`BidscubeSDKAppLovin`** pin **`1.0.4`** via **`BidscubeIosPodfilePostprocessor`** (aligned with UPM).
- **`BidscubeAndroidSdkInterop`:** **`ClassNotFoundException`** for **`com.bidscube.sdk.BidscubeSDK`** is logged as **WARNING** (Unity C# creatives can still run); other init failures remain errors. Warning when Java **`SDKConfig.Builder`** lacks **`adRequestAuthority`** / **`BaseURL`** setters now points integrators at UPM upgrade and removing duplicate **`project(':bidscube-sdk-…')`** / legacy AAR lines.
- **`Documentation~/INTEGRATION.md`:** logcat guidance — Unity **`[BidscubeSDK]`** lines vs native tags **`BidscubeSDK`** / **`BidscubeSDKImpl`**; **`Init (publisher row)`** closing line.

### Fixed

- **Direct SDK video (VAST / JSON):** fetch path uses **`SetupUI(false)`** so IMA is not attached while **`UnityEngineVideoSurfacePlayback`** prepares the stream; **`TryEnsureSurfacePlayback(forLoadVideoAdCoroutine: true)`** after **`SetupUI`** so surface playback exists before assigning media URL. **`OnDestroy`:** stop surface playback after unsubscribing events.
- **Android player:** prior ad roots under **`AdViewController`** are torn down with **`DestroyImmediate`** when replacing fullscreen video to reduce overlapping **`VideoPlayer`** / **`MediaHTTP`** instances in the same frame.

---

## [1.0.3] - 2026-04-08

### Added

- **UPM dependency installer** — `Editor/BidscubeUpmDependencyInstaller.cs` ensures **`com.unity.ugui`** and **`com.unity.textmeshpro`** are present via Package Manager once per Editor session (parity with [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) “no manual TMP/UGUI setup” behavior; `package.json` still declares them for Git UPM resolution).
- **Bundled Android MAX adapter** — `Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.3.aar` (`com.applovin.mediation.adapters.BidscubeMediationAdapter`) so you do **not** add the Bidscube adapter from a separate distribution for Android.
- **Gradle:** `com.applovin:applovin-sdk:13.+` injected with other Bidscube dependencies (**minimum AppLovin MAX Android SDK 13.0** line).
- **Editor (iOS):** `BidscubeIosPodfilePostprocessor` appends **`AppLovinSDK`** (`>= 13.0.0`, `< 14.0`) and **`BidscubeSDKAppLovin`** (`1.0.3`) to an exported **Podfile** when those pods are not already declared — parity with [AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS). Skips **`BidscubeSDKAppLovin`** if a standalone **`BidscubeSDK`** pod is present.

### Changed

- **Android MAX adapter** (`BidscubeMediationAdapter`): if Java `BidscubeSDK` is **already initialized** from Unity (recommended), the adapter reports **INITIALIZED_SUCCESS** without requiring **App ID** from MAX server parameters; fixes MAX treating Bidscube as failed when Unity init runs first. Stronger **consumer ProGuard** keep rules for the adapter constructor. Rebuilt bundled **`applovin-bidscube-max-adapter-1.0.3.aar`**.
- **`Constants.SdkVersion`** → `1.0.3` (UPM and user-agent string).
- Historical note: **1.0.3** initially shipped a bundled `bidscube-sdk-1.2.2.aar`; core SDK is now Maven-only (see **1.0.4**).

### Notes

- **iOS MAX:** CocoaPods **`BidscubeSDKAppLovin`** (not bundled as source; resolved via CocoaPods). Docs and dashboard settings aligned with the iOS repo (**`ALBidscubeMediationAdapter`**, **App ID** = placement ID, **`request_authority` / `ssp_host`**). Android remains self-contained aside from Maven Central / `google()` resolution.

---

## [1.0.1] - 2026-03-23

### Added

- **`SDKConfig.AdRequestAuthority`** and **`SDKConfig.Builder.AdRequestAuthority(string)`** — SSP host only (optional port / IPv6), default `ssp-bcc-ads.com` (parity with Android `DeviceInfo.DEFAULT_AD_REQUEST_AUTHORITY`). **`Builder.BaseURL(string)`** uses the same normalization (legacy alias).
- **`AdRequestAuthorityNormalizer`**, **`SspAdUriHelper`** — normalization and `https://<authority>/sdk` construction without breaking `host:port` (Android `SspAdUriHelper` parity).
- **`Documentation~/AD_REQUEST_ENDPOINT.md`** — ad request endpoint, query tables, mediation notes.
- **Editor:** menu **Bidscube → Validate SSP URL parity (console)**; samples support **`BIDSCUBE_SSP_AUTHORITY`** (Editor) and optional inspector **Ad Request Authority**.

### Changed

- **`URLBuilder`** — per-ad-type query parameters aligned with Android `ImageAdUrlBuilder`, `VideoAdUrlBuilder`, `NativeAdUrlBuilder` (image/video no longer carry the same GDPR block as native).
- **`SDKConfig.BaseURL`** — computed getter from `AdRequestAuthority` (always `https://…/sdk`).
- **`Constants.SdkVersion`** → `1.0.1`.
- **Android interop** — prefers native **`adRequestAuthority`** on `SDKConfig.Builder`, falls back to **`baseURL`** setters when the AAR has no authority API.

---

## [1.0.0] - 2026-03-22

### Breaking

- **Removed IronSource / Level Play Unity integration artifacts**: Java adapters under `Runtime/Plugins/Android/com/ironsource/...`, iOS `BidscubeLevelPlay*` sources, and `BidscubeLevelPlayBridge.cs`. Mediation is **AppLovin MAX** + the **Bidscube MAX adapter** only (see `Documentation~/APPLOVIN_MAX.md`).
- **Removed** `Constants.LevelPlayAdapterVersion` and `Constants.LevelPlayUnityBridgeGameObjectName`.

### Migration

- **Was:** Level Play custom adapters + Unity bridge + `UnitySendMessage` to `BidscubeLevelPlayBridge`.
- **Now:** Add **AppLovin MAX** and the **Bidscube MAX adapter**; call `BidscubeSDK.Initialize` with `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` early so the native adapter shares the same SDK instance. Load/show only through MAX.
- **Legacy configs:** stored `integrationMode` / wire value `levelPlay` (or `level_play`) should map to **`AppLovinMaxMediation`** — use `SDKConfig.Builder().IntegrationModeFromWire(...)` (same idea as Flutter `levelPlay` → `appLovinMaxMediation`).

### Added

- Bundled **`bidscube-sdk`** Android AAR under `Runtime/Plugins/Android` and Editor **`BidscubeAndroidGradlePostprocessor`** to inject required Maven dependencies (full Bidscube Android stack without a separate Maven install for the core SDK). See `Documentation~/ANDROID_BUNDLED_SDK.md`.
- **`package.json` → `repository`** (canonical GitHub URL for releases).
- **`tools/verify-release-ready.sh`**, **`.github/workflows/ci.yml`**, **`.gitattributes`** — checks and repo hygiene for GitHub publication.
- **`BidscubeIntegrationMode`**: `DirectSdk` vs `AppLovinMaxMediation`.
- **`SDKConfig`**: `IntegrationMode`, `EnableTestMode`; Android interop forwards **`BaseURL`** / **test mode** to native `SDKConfig.Builder` via method aliases (parity with Flutter).
- Startup logs: `integrationMode=` in C# and on Android in `BidscubeAndroidSdkInterop`.
- Docs: `Documentation~/APPLOVIN_MAX.md`, `Documentation~/TEST_PLAN.md`.

### Changed

- C# creative APIs throw **`InvalidOperationException`** in **AppLovin MAX** mode (analogous to Flutter blocking `get*AdView` in mediation).
- `BidscubeAndroidSdkInterop` moved to `Runtime/BidscubeSDK/Core/`.
- **Android Gradle post-processor**: ensures `compileSdk` / `compileSdkVersion` ≥ **34** and `minSdk` / `minSdkVersion` ≥ **26** in generated `unityLibrary` and `launcher`; appends `android.suppressUnsupportedCompileSdk=34,35,36` to **`gradle.properties`** when needed for AGP 8 **`CheckAarMetadata`** with Unity **compileSdk 35+**; `desugar_jdk_libs` **2.1.4**; upgrades prior injected **2.0.4** lines.

### Notes

- **UPM `1.0.0`** — first public **AppLovin MAX** line release (no separate Level Play bridge). If an old **`v1.0.0`** tag on the remote points at a different commit, delete it on origin and recreate the tag on the current commit before publishing (see `RELEASE.md`).
