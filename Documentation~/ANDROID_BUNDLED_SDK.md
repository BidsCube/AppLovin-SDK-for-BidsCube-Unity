# Bundled Bidscube Android SDK (AAR) + MAX adapter

The Unity package ships:

- **`Runtime/Plugins/Android/bidscube-sdk-1.0.0.aar`** — you do **not** add `com.bidscube:bidscube-sdk` from Maven manually for:
  - `BidscubeAndroidSdkInterop` — mirrors C# `SDKConfig` into Java `com.bidscube.sdk.BidscubeSDK` on device (shared instance for **AppLovin MAX** mediation)
- **`Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.3.aar`** — Bidscube **custom network** adapter for AppLovin MAX (`com.applovin.mediation.adapters.BidscubeMediationAdapter`). No separate adapter distribution needed on Android.

## Version alignment

Keep the **bidscube-sdk** AAR version in sync with:

- **Flutter** plugin: `implementation "com.bidscube:bidscube-sdk:1.0.0@aar"` in `AppLovin-SDK-Flutter/android/build.gradle`
- **AppLovin MAX** adapter expectations (same major/minor as your adapter release notes)

When the native SDK bumps, replace the AAR filename and update docs. **`Constants.SdkVersion`** follows the **Unity UPM** release (e.g. `1.0.3`); it may differ from the **Java SDK** AAR version string.

## Transitive dependencies

Local AARs do not carry Maven dependencies. On Android Gradle export, **`BidscubeAndroidGradlePostprocessor`** injects:

- **`com.applovin:applovin-sdk:13.+`** — AppLovin MAX Android SDK (**minimum 13.0** line; resolves to latest **13.x** on Maven Central)
- The same `implementation` lines as the native Bidscube Android SDK module (`media3`, IMA, UMP, Material, Glide, desugar libs)

Requirements:

- Unity **Android Build Support** installed (Editor script uses `UnityEditor.Android`).
- Gradle project must resolve `google()` / `mavenCentral()` (Unity default).

## Rebuilding the AARs (maintainers)

**Core SDK** (from the Bidscube Android SDK repository):

```bash
./gradlew :sdk:assembleRelease
cp sdk/build/outputs/aar/sdk-release.aar <path-to-unity-package>/Runtime/Plugins/Android/bidscube-sdk-1.0.0.aar
```

**MAX adapter** (same repository, `applovin-adapter` module):

```bash
./gradlew :applovin-adapter:assembleRelease
cp applovin-adapter/build/outputs/aar/applovin-adapter-release.aar <path-to-unity-package>/Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.3.aar
```

Bump adapter filename / `getAdapterVersion()` in Java when you change the packaged adapter version.

## Gradle: `CheckAarMetadata` / compileSdk

AndroidX **Material 1.12** and related dependencies declare a minimum **compileSdk** in AAR metadata (typically **34+**). If Unity generates `unityLibrary` / `launcher` with a lower `compileSdk`, the build fails on `CheckAarMetadataWorkAction`.

The **`BidscubeAndroidGradlePostprocessor`** runs after Gradle is generated and:

- Raises numeric `compileSdk` / `compileSdkVersion` and `minSdk` in `unityLibrary` and `launcher` (minimum **compileSdk 34**, **minSdk 26** to satisfy AAR-metadata dependencies);
- Appends to the root **`gradle.properties`** `android.suppressUnsupportedCompileSdk=34,35,36` when Unity builds with **compileSdk 35–36** while some AAR metadata only declares up to **34**;
- Uses **`desugar_jdk_libs:2.1.4`**.

In **Player Settings**, prefer **Minimum API Level** ≥ **26** (keep **24** only if you do not use the same Maven dependency set).
