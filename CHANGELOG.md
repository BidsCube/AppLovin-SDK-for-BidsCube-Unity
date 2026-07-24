## [Unreleased]

---

## [1.0.25] - 2026-07-24

### Added

- **Publisher `user_id`:** pass at init via **`SDKConfig.Builder().UserId(...)`** or after login via **`BidscubeSDK.SetUserId(...)`** — forwarded to native Android/iOS and sent as query param **`user_id`** on SSP ad requests for postback attribution.
- Depends on **`com.bidscube.sdk` 1.2.16** (native Android/iOS bridge + C# direct ads).

### Changed

- Bundled Android core SDK + MAX adapter AARs **1.2.10 → 1.2.11** (native `user_id` / `SDKConfig.Builder.userId`).
- iOS CocoaPods pin **`BidscubeSDKAppLovin` 1.1.1** (native `user_id` + MAX server parameter forwarding).

---

## [1.0.24] - 2026-07-08

### Fixed

- **Android native compatibility:** bundled core SDK AARs updated from **1.2.5** to **1.2.10** to match **`applovin-bidscube-max-adapter-1.2.10.aar`**. Fixes potential runtime **`NoSuchMethodError`** for `collectSignal`, `clearPreloadCache`, `setMediationAdapterVersion`, preload APIs, and mediation `initialize(...)`.

### Added

- **`tools/verify-release-ready.sh`:** **`javap`** adapter/core method compatibility check (classfile fallback when JDK tools unavailable).
- **`tools/build-android-max-adapter-into-package.sh`:** fails if staged Android core AAR version does not match **`NativeAndroidBidscubeSdkVersion`**; removes stale core/adapter AARs.

OpenRTB 2.6 support, when available, is provided by the native Bidscube SDKs used by the native AppLovin MAX adapters. The Unity package does not parse OpenRTB responses and does not build or POST OpenRTB bid requests.

OpenRTB 2.6-style response parsing is not implemented yet in the native MAX stack for this release (Option C).

---

## [1.0.23] - 2026-07-08

### Changed

- **Android MAX adapter:** bundled **`applovin-bidscube-max-adapter-1.2.10.aar`** (fixed native adapter from AppLovin-SDK-for-BidsCube-Android; no test signal / dummy Native MAX).
- **iOS:** **`BidscubeIosPodfilePostprocessor`** injects **`BidscubeSDKAppLovin` 1.1.0** on Xcode export when missing (does not duplicate **`AppLovinSDK`** from the official MAX Unity plugin).
- **`AppLovinMaxRewardedBridge.EnableDirectSdkFallback`** default is **`false`** — MAX load/show is the primary path; direct SDK fallback is opt-in QA/debug only.
- **`AppLovinMaxUnityReflection`:** MREC helpers (**`TryCreateMRec`**, **`TryShowMRec`**, **`TryHideMRec`**, **`TryDestroyMRec`**) and clarified helper-only docs (no mediation layer).
- **Docs:** supported MAX formats (Banner, MREC, Interstitial, Rewarded); Native MAX not advertised; Android four video modes vs single iOS pod; release ZIP hygiene validation.
- **OpenRTB 2.6 (Option C):** explicit native-only delegation docs; **`AdapterPackageInfo.OpenRtb26*ResponseParsingSupported = false`**; verify script blocks ambiguous OpenRTB claims and checks adapter `openrtb_2_6_response_parsing` signal.

OpenRTB 2.6 support, when available, is provided by the native Bidscube SDKs used by the native AppLovin MAX adapters. The Unity package does not parse OpenRTB responses and does not build or POST OpenRTB bid requests.

OpenRTB 2.6-style response parsing is not implemented yet in the native MAX stack for this release (Option C).

### Added

- **`AdapterPackageInfo`:** **`OpenRtb26AndroidResponseParsingSupported`** / **`OpenRtb26IosResponseParsingSupported`** flags aligned with native **1.2.10** / **`BidscubeSDKAppLovin` 1.1.0**.
- **AppLovin MAX server parameter matrix** in README/INSTALL (`request_authority` / `ssp_host` active; OpenRTB pod params reserved).

- **`tools/verify-release-ready.sh`:** forbidden-string scan on bundled adapter AAR (**`bidscube_test_signal`**, dummy Native strings, **`MaxNativeAdAdapter`** / **`loadNativeAd`**).
- **Release workflow:** runs **`verify-release-ready.sh`** and rejects ZIPs containing **`.git`**, **`__MACOSX`**, **`._*`**, Unity **`Library/`** / **`Temp/`** / etc.
- **Sibling QA project:** **`BidscubeUnityAppLovinTestApp`** (outside this package) for full MAX integration testing.

---

## [1.0.22] - 2026-05-25

### Changed

- Updated AppLovin MAX adapter mapping for separated Interstitial Video and Rewarded Video core SDK contracts.
- MAX Rewarded now maps to Bidscube `showRewardedVideoAd`.
- MAX Interstitial Video now maps to Bidscube `showInterstitialVideoAd` where video interstitial flow is used.
- Reward is forwarded to MAX only from Bidscube `onUserRewarded`.
- **UPM / peer:** bumped **`com.bidscube.applovin.max`** to **1.0.22** and aligned the declared peer dependency to **`com.bidscube.sdk` 1.2.12**.
- **Android packaging:** bundled MAX adapter AAR updated to **`applovin-bidscube-max-adapter-1.2.6.aar`**; bundled core AARs at **1.2.5** rebuilt with **`showInterstitialVideoAd`** / **`showRewardedVideoAd`** and **`onUserRewarded`** separation for all four Android export modes.

### Fixed

- Rewarded video no longer relies on generic `showVideoAd`.
- Reward is no longer inferred from close/hidden/skipped events or `onVideoAdCompleted`.
- Interstitial video never triggers reward.
- Interstitial/rewarded show failures now use MAX display-failed callbacks instead of load-failed callbacks.
- **`AppLovinMaxRewardedBridge`:** when MAX rewarded is not ready, optional fallback to Bidscube SDK direct rewarded APIs.

---

## [1.0.21] - 2026-05-12

### Changed

- **UPM / peer:** bumped **`com.bidscube.applovin.max`** to **1.0.21** and aligned the declared peer dependency to **`com.bidscube.sdk` 1.2.11**.
- **Android packaging:** bundled four-mode Bidscube core AAR filenames now target native SDK **1.2.5**; bundled MAX adapter AAR metadata now targets **1.2.5**.

### Fixed

- **Rewarded video / Android modes:** release metadata, install docs, verification scripts, and release notes now match the four-mode Android export flow that was validated for Lite, WebView, Legacy Media, and Full video builds.

---

## [1.0.20] - 2026-05-06

### Changed

- **Peer dependency:** **`com.bidscube.sdk` 1.2.10**.
- **Android modes:** AppLovin adapter now tracks four Android core modes — **LiteNoVideo**, **WebViewVideoNoDesugar**, **LegacyMediaVideoNoDesugar**, **FullWithVideo**.

### Fixed

- **Editor:** **`BidscubeAndroidBuildFeaturesWindow`** no longer imports **`BidscubeSDK.Android`** — uses the same **`EditorPrefs`** key as core and **`BidscubeDefineApplicator.ApplyFromStoredFeatureSet()`**, so the adapter compiles when the core **`BidscubeSDK`** runtime assembly is resolved without pulling Android types transitively.
- **Gradle export:** bundled AAR metadata and validation scripts now cover **lite**, **webview video**, **legacy media video**, and **full video** Android artifacts.

---

## [1.0.19] - 2026-05-06

### Changed

- **UPM / tag:** **1.0.19** — `package.json`, `AdapterPackageInfo.UpmVersion`, **README**, **Documentation~/INSTALL.md** (peer **`com.bidscube.sdk` 1.2.8** unchanged).

---

## [1.0.18] - 2026-05-06

### Added

- **Android:** Bundled core AARs renamed to **`bidscube-sdk-lite-no-video-1.2.4.aar`** and **`bidscube-sdk-full-video-1.2.4.aar`**, matching native Maven **`com.bidscube:sdk-lite-no-video`** / **`com.bidscube:sdk-full-video`** at **1.2.4**.
- **Gradle:** **LiteNoVideo** strips **`coreLibraryDesugaring`** / **`coreLibraryDesugaringEnabled`** from generated **launcher** and **unityLibrary** `build.gradle`. **FullWithVideo** ensures **`coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.0.4'`** and **`coreLibraryDesugaringEnabled true`** in the launcher when missing.
- **Shared editor/runtime:** Android export settings, scripting defines, and **`BidscubeAndroidGradleProjectPatcher`** live in **`com.bidscube.sdk` 1.2.8** (`BidscubeSDK.Android.Editor`).

### Changed

- **Peer dependency:** **`com.bidscube.sdk` 1.2.8**.
- **Maven full core** line is **`com.bidscube:sdk-full-video:{ver}@aar`** (replaces **`bidscube-sdk`**).
- **Docs:** **`README.md`** shortened to install pointers; single guide **`Documentation~/INSTALL.md`** (removed older split files under `Documentation~/`).

---

## [1.0.17] - 2026-05-06

### Changed

- **Package / adapter UPM version** **1.0.17** — `package.json`, `AdapterPackageInfo.UpmVersion`, docs and demo manifests aligned with peer **`com.bidscube.sdk` 1.2.7** (no native AAR or MAX adapter binary change in this release).

---

## [1.0.16] - 2026-05-06

### Added

- **Android:** **`BidscubeAndroidExportSettings.enableDesugaring`** — when **unchecked** (settings asset required), **`BidscubeAndroidGradlePostprocessor`** strips **`coreLibraryDesugaring`** and sets **`coreLibraryDesugaringEnabled false`** in generated **`launcher`** and **`unityLibrary`** `build.gradle`. **Note:** bundled **`bidscube-sdk-lite` / `bidscube-sdk`** AAR metadata usually **requires** desugaring on **`:launcher`** — builds fail **`checkReleaseAarMetadata`** unless desugaring stays enabled or the core dependency does not declare the requirement. Editor logs a **warning** when stripping is requested.
- **Docs:** **`Documentation~/APPLOVIN_MEDIATION_STEPS.md`** — short MAX + Bidscube checklist; links from **README**, **INTEGRATION**, **APPLOVIN_MAX**; expanded guidance to **keep desugaring enabled** for bundled AARs.

### Changed

- **Peer dependency:** **`com.bidscube.sdk` 1.2.7** (align with core Unity SDK release).
- **`AdapterPackageInfo.UpmVersion`** / **`package.json`** → **1.0.16**.

---

## [1.0.15] - 2026-05-04

### Fixed

- **Editor:** added missing **`.meta`** files for `Editor` scripts, `RELEASE_CHECKLIST.md`, and `tools` scripts so Unity UPM imports **asmdef** / **Editor** assemblies in immutable package cache.
- **Android Gradle:** **LiteNoVideo** uses only the bundled **lite** AAR (`files('libs/bidscube-sdk-lite-…')`); no **`com.bidscube:bidscube-sdk`** Maven line, no **Media3**, no **Google IMA**.
- **FullWithVideo** no longer injects an unresolved **`com.bidscube:bidscube-sdk:1.2.3@aar`** when the full AAR is missing — export **fails in the Editor** with a clear message unless **`bidscube-sdk-1.2.3.aar`** is present or **`MavenBidscubeSdkAar`** is used with a reachable artifact.

### Changed

- **Default Android mode** is **LiteNoVideo** again (bootstrap, EditorPrefs default, new export settings assets).
- **`AdapterPackageInfo.UpmVersion`** / **`package.json`** → **1.0.15**.

### Validation

- `tools/verify-release-ready.sh` checks **`.meta`** coverage and stale **`.meta`** files; **LiteNoVideo** postprocessor path must not add **`com.bidscube:bidscube-sdk`** or video Maven coordinates.

---

## [1.0.14] - 2026-05-04

### Fixed

- Aligned Android build mode logic for **`FullWithVideo`** and **`LiteNoVideo`**.
- Made **`FullWithVideo`** the default build mode (bootstrap, EditorPrefs default, new **`BidscubeAndroidExportSettings`** assets).
- Ensured **`LiteNoVideo`** does not inject Media3 / Google IMA dependencies.
- Ensured **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`** matches the selected Android build mode (with **`BidscubeDefineApplicator`** + **`BidscubeAndroidExportSettingsResolver`**).
- Fixed documentation conflicts between README, **`ANDROID_BUNDLED_SDK.md`**, Editor window, and bootstrap defaults.

### Changed

- Bumped UPM package version to **`1.0.14`**.
- Updated **`AdapterPackageInfo.UpmVersion`** to **`1.0.14`**.
- Gradle logs: **`[Bidscube AppLovin] Android feature set: …`**, **`Skipping Media3 and Google IMA dependencies`** / **`Including Media3 and Google IMA dependencies`**.

### Validation

- **FullWithVideo** injects full video dependency graph (Bidscube core **`…@aar`** + Media3 + IMA when no duplicate exists).
- **LiteNoVideo** builds without Media3 / Google IMA in the managed Gradle block.

---

## [1.0.13] - 2026-04-30

### Added

- **Release tooling:** `tools/build-release-lite-no-player.sh` and `tools/build-release-full-with-player.sh` — validate AAR layout and emit named ZIP artifacts for the same UPM tree.
- **CI:** package identity checks, forbidden binaries / Unity cache folders, Gradle postprocessor smoke validation.

### Changed

- **Package identity:** UPM id is **`com.bidscube.applovin.max`** (not the core SDK); peer dependency **`com.bidscube.sdk`** **1.2.5**; repository / changelog / docs URLs point to **`https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity`**.
- **Android modes:** **`BidscubeAndroidFeatureSet.LiteNoVideo`** vs **`FullWithVideo`** — **`BidscubeAndroidExportSettings`** asset, **`BidscubeAndroidExportSettingsResolver`**, **`BidscubeAndroidGradlePostprocessor`** (exactly **one** bundled core AAR into **`unityLibrary/libs/`** when using lite/files mode), Android define **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`** only in **LiteNoVideo**, **Media3 + Google IMA** only in **FullWithVideo**. (**Default mode corrected to FullWithVideo in 1.0.14.**)
- **Bundled lite core AAR:** PluginImporter **Android disabled** so Unity does not merge the same artifact twice; adapter AAR remains enabled for Android.
- **Samples:** video paths use **`#if !BIDSCUBE_ANDROID_LITE_NO_VIDEO`** / **`#if BIDSCUBE_ANDROID_LITE_NO_VIDEO`** with clear lite messaging.

### Notes

- **Native Android core** remains **1.2.3** while Unity **`com.bidscube.sdk`** is **1.2.5** — see **`README`** / **`ANDROID_BUNDLED_SDK.md`** (native release cadence may trail Unity patch).
- **Official AppLovin MAX Unity SDK** is **not** bundled; add it in the host project.

---

## [1.0.12] - 2026-04-29

### Changed

- **Package split:** **`com.bidscube.applovin.max`** is a **companion** to **`com.bidscube.sdk`**: ships **`AppLovinMaxUnityReflection`**, bundled **MAX** adapter AAR, and **lite** core AAR; **C# SDK surface** and optional **Editor / Gradle** automation are expected from **`com.bidscube.sdk`** (see README / INTEGRATION scope).
- **`AdapterPackageInfo.UpmVersion`** / **`package.json`** → **1.0.12** (replaces version sync via removed **`Constants.SdkVersion`** in this package).
- **Docs:** version matrix and peer **`com.bidscube.sdk` 1.2.5** / native **1.2.3** / MAX **1.0.4** / AppLovin **13.x** in **`ANDROID_BUNDLED_SDK.md`**, **`APPLOVIN_MAX.md`**, **`README`**, **`INTEGRATION.md`**, **`TEST_PLAN.md`**, **`RELEASE.md`**; removed misleading “this package injects AppLovin 13.+” and stale **`Constants.*`** references in favour of **`AdapterPackageInfo`**.

### Removed (from this package; use core SDK or your Gradle templates)

- Monolithic **Runtime** SDK sources, **WebView** native plugins, and **Editor** Android/iOS post-processors (lines previously under **`Editor/`** and most of **`Runtime/BidscubeSDK/`** outside **`Mediation/`**). Integrators who relied on the in-repo Gradle / Podfile hooks should follow **`Documentation~/ANDROID_BUNDLED_SDK.md`** manually or use tooling from **`com.bidscube.sdk`**.

---

## [1.0.11] - 2026-04-23

### Changed

- **`Constants.SdkVersion`** / **`package.json`** → **1.0.11**.

---

## [1.0.10] - 2026-04-23

### Fixed

- **Android Editor:** **`BidscubeAndroidExportSettingsResolver.GetEffectiveFeatureSet`** — **`TryLoadFirst`** must receive both **`out`** parameters; use **`TryLoadFirst(out var s, out _)`** (**CS7036**).
- **Android Editor:** **`BidscubeAndroidGradlePostprocessor`** — invalid verbatim-string regex literals for Gradle quote detection broke C# compilation; match **`['""]`** via doubled quotes in verbatim interpolated strings so **`EnsureLauncherCoreLibraryDesugaring`** compiles in Unity.
- **UPM:** **`bidscube-sdk-lite-*.aar.meta`** — **`guid`** was **31** hex digits; Unity requires **32** (YAML / asset import).
- **UPM:** Added missing **`.meta`** for **`BidscubeAndroidBuildFeatures`**, **`BidscubeAndroidFeatureSet`**, **`BidscubeAndroidExportSettings`**, **`BidscubeAndroidExportSettingsResolver`**, and **`BidscubeAndroidScriptingDefinesPreprocessor`** so Package Manager can import from a read-only cache without missing-meta / script import errors. **`BidscubeAndroidSdkInterop.cs.meta`** completed with **`MonoImporter`** (was GUID-only).

### Changed

- **Release ZIP:** Exclude AppleDouble **`._*`** / **`*/._*`** in addition to **`.git`**, **`__MACOSX/*`**, and **`*.DS_Store`**.
- **`Constants.SdkVersion`** / **`package.json`** → **1.0.10**.

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
