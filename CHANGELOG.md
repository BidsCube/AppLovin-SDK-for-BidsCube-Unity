## [1.0.0] - 2026-03-22

### Breaking

- **Removed IronSource / Level Play Unity integration artifacts**: Java adapters under `Runtime/Plugins/Android/com/ironsource/...`, iOS `BidscubeLevelPlay*` sources, and `BidscubeLevelPlayBridge.cs`. Mediation is **AppLovin MAX** + the **Bidscube MAX adapter** only (see `Documentation~/APPLOVIN_MAX.md`).
- **Removed** `Constants.LevelPlayAdapterVersion` and `Constants.LevelPlayUnityBridgeGameObjectName`.

### Migration

- **Was:** Level Play custom adapters + Unity bridge + `UnitySendMessage` to `BidscubeLevelPlayBridge`.
- **Now:** Add **AppLovin MAX** and the **Bidscube MAX adapter**; call `BidscubeSDK.Initialize` with `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` early so the native adapter shares the same SDK instance. Load/show only through MAX.
- **Legacy configs:** stored `integrationMode` / wire value `levelPlay` (or `level_play`) should map to **`AppLovinMaxMediation`** — use `SDKConfig.Builder().IntegrationModeFromWire(...)` (same idea as Flutter `levelPlay` → `appLovinMaxMediation`).

### Added

- Bundled **`bidscube-sdk-1.0.0.aar`** under `Runtime/Plugins/Android` and Editor **`BidscubeAndroidGradlePostprocessor`** to inject required Maven dependencies (full Bidscube Android stack without a separate Maven install). See `Documentation~/ANDROID_BUNDLED_SDK.md`.
- **`package.json` → `repository`** (canonical GitHub URL for releases).
- **`tools/verify-release-ready.sh`**, **`.github/workflows/ci.yml`**, **`.gitattributes`** — перевірки та підготовка до публікації на GitHub.
- **`BidscubeIntegrationMode`**: `DirectSdk` vs `AppLovinMaxMediation`.
- **`SDKConfig`**: `IntegrationMode`, `EnableTestMode`; Android interop forwards **`BaseURL`** / **test mode** to native `SDKConfig.Builder` via method aliases (parity with Flutter).
- Startup logs: `integrationMode=` in C# and on Android in `BidscubeAndroidSdkInterop`.
- Docs: `Documentation~/APPLOVIN_MAX.md`, `Documentation~/TEST_PLAN.md`.

### Changed

- C# creative APIs throw **`InvalidOperationException`** in **AppLovin MAX** mode (analogous to Flutter blocking `get*AdView` in mediation).
- `BidscubeAndroidSdkInterop` moved to `Runtime/BidscubeSDK/Core/`.
- **Android Gradle post-processor**: ensures `compileSdk` / `compileSdkVersion` ≥ **34** and `minSdk` / `minSdkVersion` ≥ **26** in generated `unityLibrary` and `launcher`; appends `android.suppressUnsupportedCompileSdk=34,35,36` to **`gradle.properties`** when needed for AGP 8 **`CheckAarMetadata`** with Unity **compileSdk 35+**; `desugar_jdk_libs` **2.1.4**; upgrades prior injected **2.0.4** lines.

### Notes

- **UPM `1.0.0`** — перший публічний реліз лінії **AppLovin MAX** (без окремого Level Play bridge). Якщо на репозиторії вже існує старий тег **`v1.0.0`** з іншого коміту, перед публікацією цього релізу видаліть його на origin і створіть тег знову на актуальному коміті (див. `RELEASE.md`).
