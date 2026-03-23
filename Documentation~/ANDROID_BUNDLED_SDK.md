# Bundled Bidscube Android SDK (AAR)

The Unity package ships **`Runtime/Plugins/Android/bidscube-sdk-1.0.0.aar`** so you do **not** need to add `com.bidscube:bidscube-sdk` from Maven manually for:

- `BidscubeAndroidSdkInterop` — mirrors C# `SDKConfig` into Java `com.bidscube.sdk.BidscubeSDK` on device (shared instance for **AppLovin MAX** adapters)

## Version alignment

Keep this AAR’s version in sync with:

- **Flutter** plugin: `implementation "com.bidscube:bidscube-sdk:1.0.0@aar"` in `AppLovin-SDK-Flutter/android/build.gradle`
- **AppLovin MAX** Bidscube Android adapter expectations (same major/minor as your adapter release notes)

When the native SDK bumps, replace the AAR filename and update docs + `Constants.SdkVersion` if your release process ties them together.

## Transitive dependencies

A local AAR does not carry Maven dependencies. On Android Gradle export, **`BidscubeAndroidGradlePostprocessor`** (Editor) injects the same `implementation` lines as the native Android SDK module (`media3`, IMA, UMP, Material, Glide, desugar libs).

Requirements:

- Unity **Android Build Support** installed (Editor script uses `UnityEditor.Android`).
- Gradle project must resolve `google()` / `mavenCentral()` (Unity default).

## Rebuilding the AAR (maintainers)

From the Bidscube Android SDK repository:

```bash
./gradlew :sdk:assembleRelease
cp sdk/build/outputs/aar/sdk-release.aar <path-to-unity-package>/Runtime/Plugins/Android/bidscube-sdk-1.0.0.aar
```

Bump the filename / docs when the SDK version changes.

## Gradle: `CheckAarMetadata` / compileSdk

AndroidX **Material 1.12** and сусідні залежності оголошують у AAR-metadata мінімальний **compileSdk** (зазвичай **34+**). Якщо Unity згенерував `unityLibrary` / `launcher` з нижчим `compileSdk`, збірка падає на `CheckAarMetadataWorkAction`.

Пакет **`BidscubeAndroidGradlePostprocessor`** після генерації Gradle:

- піднімає числові `compileSdk` / `compileSdkVersion` і `minSdk` у `unityLibrary` та `launcher` (мінімум **compileSdk 34**, **minSdk 26** для узгодження з AAR-metadata залежностей);
- додає в кореневий **`gradle.properties`** `android.suppressUnsupportedCompileSdk=34,35,36`, якщо Unity збирає з **compileSdk 35–36**, а частина AAR у metadata вказує лише до **34**;
- використовує **`desugar_jdk_libs:2.1.4`**.

У **Player Settings** бажано **Minimum API Level** ≥ **26** (або залиште 24 лише якщо не підключаєте той самий набір Maven-залежностей).

## AppLovin MAX (not bundled here)

Mediation is **AppLovin MAX** + the **Bidscube MAX adapter** from your adapter distribution. Those artifacts are **not** included in this Unity package; add them per `Documentation~/APPLOVIN_MAX.md`.
