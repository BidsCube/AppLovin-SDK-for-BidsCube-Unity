# Bundled Bidscube MAX adapter + Maven core SDK (Android)

## What ships in the UPM package

- **`Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar`** — Bidscube **custom network** adapter for AppLovin MAX (`com.applovin.mediation.adapters.BidscubeMediationAdapter`). This is the **only** Bidscube Android library shipped as a binary inside the package.
- **Core `com.bidscube:bidscube-sdk`** — **not** bundled inside the UPM package. On Gradle export, **`BidscubeAndroidGradlePostprocessor`** injects  
  `implementation 'com.bidscube:bidscube-sdk:<NativeAndroidBidscubeSdkVersion>@aar'`  
  (see `Runtime/BidscubeSDK/Core/Constants.cs`, currently **1.2.2**) into **`unityLibrary/build.gradle`**, next to AppLovin 13.x, Media3, IMA, UMP, Glide, and Material. **Core library desugaring is not injected by default** — see **`BidscubeAndroidGradlePostprocessor.NoDesugarMode`**. The **`@aar`** suffix forces resolution of the **Android library artifact**; some Maven publications expose a root **`packaging=pom`** coordinate where a plain dependency can resolve **metadata only** (no **`com.bidscube.sdk.BidscubeSDK`** in the APK). Gradle still fetches the artifact from **Maven Central** (network on first resolve / CI cache).

This layout keeps the **MAX adapter** self-contained in the repo while the **runtime** still uses one official core SDK artifact — the adapter AAR does not embed duplicate `com.bidscube.sdk.*` classes.

## Version sync

Keep **`Constants.NativeAndroidBidscubeSdkVersion`** and the injected Gradle coordinate aligned with the **`com.bidscube:bidscube-sdk`** line you intend to support (same semver as Flutter / native Android apps).

## Updating the adapter AAR

From the Bidscube Android SDK repo (example):

```bash
cp applovin-adapter/build/outputs/aar/applovin-adapter-release.aar <path-to-unity-package>/Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar
```

Adjust filename / version constants / docs when the adapter semver changes.

## Gradle post-processor

**`BidscubeAndroidGradlePostprocessor`** runs after Gradle is generated and:

- Injects the dependency block (marker `// __BIDSCUBE_SDK_GRADLE_DEPS__`) including **`com.bidscube:bidscube-sdk:…@aar`** when missing (covers fresh exports and older exports that only had local AARs). Legacy exports with a plain coordinate (no **`@aar`**) are rewritten on the next export.
- Ensures **`compileSdk` / `compileSdkVersion`** ≥ **34** and **`minSdk` / `minSdkVersion`** ≥ **26** in **`unityLibrary`** and **`launcher`**.
- **`NoDesugarMode`** defaults to **`true`**: the plugin does **not** inject **`coreLibraryDesugaring`**, **`desugar_jdk_libs`**, or **`coreLibraryDesugaringEnabled`**. Add those in **host** Gradle (Custom Base Gradle / exported **`mainTemplate`** / **`launcher`**) if your graph needs desugaring (e.g. AGP **`CheckAarMetadata`**, Java 8+ APIs on lower API). Each Android export logs a **Unity Editor warning** reminding you of this. If **`NoDesugarMode = false`**, the plugin may inject plugin-managed desugaring on **fresh** marker injects **only when** the exported **`unityLibrary`/`launcher` Gradle does not already declare** desugaring (host definitions are never overwritten). Validate any setup with a **clean** Gradle cache, **`./gradlew --refresh-dependencies`**, **`assembleDebug`/`assembleRelease`**, and a **lower-API** smoke test.

## Publisher checklist

- **Do not** add a second `implementation 'com.bidscube:bidscube-sdk:…@aar'` (or plain coordinate) in Custom Base Gradle / `mainTemplate` — duplicate classes / `isInitialized` confusion.
- **Do not** drop **`applovin-bidscube-max-adapter-*.aar`** from the package if you use Bidscube on MAX Android.
- Offline / air-gapped builds must pre-populate Gradle caches for `com.bidscube:bidscube-sdk` and transitives, or vendor the core AAR manually in a private Maven — the stock UPM flow expects Maven Central reachability for the core line.
