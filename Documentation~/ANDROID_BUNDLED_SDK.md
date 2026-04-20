# Bundled Bidscube MAX adapter + Maven core SDK (Android)

## What ships in the UPM package

- **`Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar`** — Bidscube **custom network** adapter for AppLovin MAX (`com.applovin.mediation.adapters.BidscubeMediationAdapter`). This is the **only** Bidscube Android library shipped as a binary inside the package.
- **Core `com.bidscube:bidscube-sdk`** — **not** bundled inside the UPM package. On Gradle export, **`BidscubeAndroidGradlePostprocessor`** injects  
  `implementation 'com.bidscube:bidscube-sdk:<NativeAndroidBidscubeSdkVersion>@aar'`  
  (see `Runtime/BidscubeSDK/Core/Constants.cs`, currently **1.2.2**) into **`unityLibrary/build.gradle`**, next to AppLovin 13.x, Media3, IMA, UMP, Glide, and Material. **Core library desugaring is never injected by this plugin** — add **`coreLibraryDesugaring`** / **`coreLibraryDesugaringEnabled`** in **host** Gradle when your graph needs it (see *Host-provided desugaring* below). Export logs a **warning** while **`NoDesugarMode`** is **`true`** (default). The **`@aar`** suffix forces resolution of the **Android library artifact**; some Maven publications expose a root **`packaging=pom`** coordinate where a plain dependency can resolve **metadata only** (no **`com.bidscube.sdk.BidscubeSDK`** in the APK). Gradle still fetches the artifact from **Maven Central** (network on first resolve / CI cache).

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
- **`NoDesugarMode`** (default **`true`**) controls only an Editor **warning** on export; the plugin **never** writes desugaring lines into Gradle (host projects are never overridden on that front). Validate Android with a **clean** Gradle cache, **`./gradlew --refresh-dependencies`**, **`assembleDebug`/`assembleRelease`**, and a **lower-API** smoke test.

## Host-provided core library desugaring

If AGP **`CheckAarMetadata`** or dependencies using Java 8+ APIs require it, enable **core library desugaring** in **your** Gradle (Unity **Custom Main Gradle Template**, **Custom Launcher Gradle Template**, or **Custom Base Gradle Template**), for example inside **`android { compileOptions { … } }`** on **`unityLibrary`** and **`launcher`**:

```gradle
compileOptions {
    coreLibraryDesugaringEnabled true
}
```

and in **`dependencies { }`**:

```gradle
coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.1.4'
```

Exact file layout depends on your Unity / AGP version; mirror Android Studio / [Android Java 8+ support](https://developer.android.com/studio/write/java8-support) guidance. After host Gradle is in place, you may set **`BidscubeAndroidGradlePostprocessor.NoDesugarMode = false`** in an Editor script to suppress the export warning (optional).

## Publisher checklist

- **Do not** add a second `implementation 'com.bidscube:bidscube-sdk:…@aar'` (or plain coordinate) in Custom Base Gradle / `mainTemplate` — duplicate classes / `isInitialized` confusion.
- **Do not** drop **`applovin-bidscube-max-adapter-*.aar`** from the package if you use Bidscube on MAX Android.
- Offline / air-gapped builds must pre-populate Gradle caches for `com.bidscube:bidscube-sdk` and transitives, or vendor the core AAR manually in a private Maven — the stock UPM flow expects Maven Central reachability for the core line.
