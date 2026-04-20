# Bundled Bidscube MAX adapter + Maven core SDK (Android)

## What ships in the UPM package

- **`Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar`** — Bidscube **custom network** adapter for AppLovin MAX (`com.applovin.mediation.adapters.BidscubeMediationAdapter`). This is the **only** Bidscube Android library shipped as a binary inside the package.
- **Core `com.bidscube:bidscube-sdk`** — **not** bundled inside the UPM package. On Gradle export, **`BidscubeAndroidGradlePostprocessor`** injects  
  `implementation 'com.bidscube:bidscube-sdk:<NativeAndroidBidscubeSdkVersion>@aar'`  
  (see `Runtime/BidscubeSDK/Core/Constants.cs`, currently **1.2.2**) into **`unityLibrary/build.gradle`**, next to AppLovin 13.x, Media3, IMA, UMP, Glide, and Material. **Launcher** **`coreLibraryDesugaring`** / **`coreLibraryDesugaringEnabled`** are injected by default (idempotent markers) so **`CheckAarMetadata`** passes when the core AAR requires desugaring on **`:launcher`**. Set **`BidscubeAndroidGradlePostprocessor.NoDesugarMode = true`** to skip that injection and own desugaring in host Gradle. The **`@aar`** suffix forces resolution of the **Android library artifact**; some Maven publications expose a root **`packaging=pom`** coordinate where a plain dependency can resolve **metadata only** (no **`com.bidscube.sdk.BidscubeSDK`** in the APK). Gradle still fetches the artifact from **Maven Central** (network on first resolve / CI cache).

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
- When **`NoDesugarMode`** is **`false`** (default), appends **launcher** **`coreLibraryDesugaringEnabled true`** and **`coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:<DesugarJdkLibsVersion>'`** (see `BidscubeAndroidGradlePostprocessor.DesugarJdkLibsVersion`, **2.1.4**) using markers `// __BIDSCUBE_CORE_LIBRARY_DESUGARING__` — skipped if already present or if **`NoDesugarMode`** is **`true`**. Validate Android with a **clean** Gradle cache, **`./gradlew --refresh-dependencies`**, **`assembleDebug`/`assembleRelease`**, and a **lower-API** smoke test.

## Host-provided core library desugaring (optional)

If you use **Custom Launcher Gradle** and already declare desugaring, set **`BidscubeAndroidGradlePostprocessor.NoDesugarMode = true`** in an Editor script so the plugin does **not** append duplicate lines. If you still need a reference snippet (e.g. for **`unityLibrary`** in a custom template), use:

```gradle
compileOptions {
    coreLibraryDesugaringEnabled true
}
```

and in **`dependencies { }`**:

```gradle
coreLibraryDesugaring 'com.android.tools:desugar_jdk_libs:2.1.4'
```

Exact file layout depends on your Unity / AGP version; mirror Android Studio / [Android Java 8+ support](https://developer.android.com/studio/write/java8-support) guidance.

## Publisher checklist

- **Do not** add a second `implementation 'com.bidscube:bidscube-sdk:…@aar'` (or plain coordinate) in Custom Base Gradle / `mainTemplate` — duplicate classes / `isInitialized` confusion.
- **Do not** drop **`applovin-bidscube-max-adapter-*.aar`** from the package if you use Bidscube on MAX Android.
- Offline / air-gapped builds must pre-populate Gradle caches for `com.bidscube:bidscube-sdk` and transitives, or vendor the core AAR manually in a private Maven — the stock UPM flow expects Maven Central reachability for the core line.
