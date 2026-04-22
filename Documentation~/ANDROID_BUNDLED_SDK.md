# Bundled Bidscube MAX adapter + core Android SDK (Android)

## What ships in the UPM package

- **`Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar`** — Bidscube **custom network** adapter for AppLovin MAX (`com.applovin.mediation.adapters.BidscubeMediationAdapter`). Unity **PluginImporter** enables this for **Android** so it is part of the exported Gradle project.
- **`Runtime/Plugins/Android/bidscube-sdk-1.2.3.aar`** — core **`com.bidscube:bidscube-sdk`** AAR aligned with **`Constants.NativeAndroidBidscubeSdkVersion`**. Its **`.meta` disables the Android plugin** so Unity does **not** merge it into **`unityLibrary`** as a second plugin (avoids duplicate classes). **Default** **`BidscubeAndroidGradlePostprocessor.CoreDependencyMode`** (**`BundledUnityLibraryLibsAar`**) copies this file into **`unityLibrary/libs/`** on each Android Gradle export and injects **`implementation files('libs/bidscube-sdk-….aar')`** — **no Maven repository is required for the core artifact**. Set **`MavenBidscubeSdkAar`** if you prefer **`implementation 'com.bidscube:bidscube-sdk:…@aar'`** from your Gradle repos instead.
- **Core on the classpath — default:** **`BundledUnityLibraryLibsAar`** (local **`libs/`** + **`files(...)`**). **Optional:** Maven **`…@aar`** via **`MavenBidscubeSdkAar`**, or **`CustomGradleLines`** / **`SkipInjectionIntegratorOwnsCore`** as below. On Gradle export, **`BidscubeAndroidGradlePostprocessor`** adds a **`// __BIDSCUBE_SDK_GRADLE_DEPS__`** block into **`unityLibrary/build.gradle`** with AppLovin 13.x, Media3, IMA, UMP, Glide, Material, and **one** line for the core SDK (see **Core SDK resolution modes** below). **Launcher** **`coreLibraryDesugaring`** / **`coreLibraryDesugaringEnabled`** are injected by default (idempotent markers) so **`CheckAarMetadata`** passes when the core AAR requires desugaring on **`:launcher`**. Set **`BidscubeAndroidGradlePostprocessor.NoDesugarMode = true`** to skip that injection and own desugaring in host Gradle.

**Default** mode copies **`bidscube-sdk-<NativeAndroidBidscubeSdkVersion>.aar`** into **`unityLibrary/libs/`** and injects  
`implementation files('libs/bidscube-sdk-<NativeAndroidBidscubeSdkVersion>.aar')`  
(see `Runtime/BidscubeSDK/Core/Constants.cs`, currently **1.2.3**). **`MavenBidscubeSdkAar`** instead injects  
`implementation 'com.bidscube:bidscube-sdk:<NativeAndroidBidscubeSdkVersion>@aar'` — the **`@aar`** suffix avoids POM-only resolution when the Maven publication uses root **`packaging=pom`**. Gradle resolves that coordinate from **whatever repositories your project declares** (Central, mirror, **`mavenLocal()`**, private host).

This layout keeps the **MAX adapter** self-contained in the repo while the **runtime** still uses **one** core SDK on the classpath — the adapter AAR does not embed duplicate `com.bidscube.sdk.*` classes.

## Core SDK resolution modes (`BidscubeAndroidGradlePostprocessor`)

Set **before** Android export / build (e.g. in a small **`[InitializeOnLoad]`** Editor script in your game assembly that references **`BidscubeSDK.Editor`**):

| `CoreDependencyMode` | Behaviour |
|----------------------|-----------|
| **`BundledUnityLibraryLibsAar`** (default) | Copies **`Runtime/Plugins/Android/bidscube-sdk-<NativeAndroidBidscubeSdkVersion>.aar`** into **`unityLibrary/libs/`** and injects **`implementation files('libs/bidscube-sdk-….aar')`** after the marker. **No Maven** needed for the core AAR. |
| **`MavenBidscubeSdkAar`** | Injects `implementation 'com.bidscube:bidscube-sdk:<Constants.NativeAndroidBidscubeSdkVersion>@aar'`. Host Gradle must expose a repo that serves that artifact (Central, internal mirror, etc.). |
| **`CustomGradleLines`** | Injects **`CustomCoreImplementationGradleLines`** after the marker instead of the bundled / Maven line. Use for a **custom path**, **`flatDir` + `name:`**, **`project(':module')`**, etc. After export, confirm **`unityLibrary/build.gradle`** and the path Unity uses for your AAR. |
| **`SkipInjectionIntegratorOwnsCore`** | Does **not** inject core; you must add **exactly one** core SDK line elsewhere (Custom Base Gradle, `mainTemplate`, or another module). The post-processor logs a **warning** if it cannot detect a `implementation` referencing bidscube. |

Example (local AAR next to Unity-exported libs — verify path on your Unity version):

```csharp
using BidscubeSDK.Editor.Android;
// In InitializeOnLoad or pre-export hook:
BidscubeAndroidGradlePostprocessor.CoreDependencyMode = BidscubeAndroidCoreDependencyMode.CustomGradleLines;
BidscubeAndroidGradlePostprocessor.CustomCoreImplementationGradleLines =
    "implementation files('libs/bidscube-sdk-1.2.3.aar')\n";
```

**Backward compatibility:** older samples assumed **`MavenBidscubeSdkAar`**. To restore that behaviour (core from Gradle repos only), set **`BidscubeAndroidGradlePostprocessor.CoreDependencyMode = BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar`** before export (e.g. **`[InitializeOnLoad]`** Editor script).

## Other Android dependencies (scope)

**`CoreDependencyMode` applies only to the Bidscube core SDK** (`com.bidscube.sdk.*`). The same **`BidscubeAndroidGradlePostprocessor`** block still adds **Gradle `implementation` coordinates** for **AppLovin SDK**, **Media3**, **IMA**, **UMP**, **Glide**, **Material**, and related lines — those resolve through **whatever repositories** the exported project declares (Maven-compatible). A **fully Maven-free** graph for every transitive (vendor every AAR / JAR yourself) is **not** implemented in this package; handle that with host Gradle (mirrors, offline cache, custom templates) if you need it.

## Version sync

Keep **`Constants.NativeAndroidBidscubeSdkVersion`** and the injected Gradle coordinate aligned with the **`com.bidscube:bidscube-sdk`** line you intend to support (same semver as Flutter / native Android apps).

## Updating the adapter AAR

From the Bidscube Android SDK repo (example):

```bash
cp applovin-adapter/build/outputs/aar/applovin-adapter-release.aar <path-to-unity-package>/Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar
```

Adjust filename / version constants / docs when the adapter semver changes.

## Updating the reference core AAR (`bidscube-sdk-*.aar`)

From the Bidscube Android SDK repo (same version as **`Constants.NativeAndroidBidscubeSdkVersion`**), e.g.:

```bash
BidscubeVersion=1.2.3 ./gradlew :sdk:assembleRelease
cp sdk/build/outputs/aar/sdk-release.aar <path-to-unity-package>/Runtime/Plugins/Android/bidscube-sdk-1.2.3.aar
```

Keep the filename in sync with **`Constants.NativeAndroidBidscubeSdkVersion`**. Preserve **Android: disabled** on **`bidscube-sdk-*.aar.meta`** so Unity does not merge the AAR twice when using **`BundledUnityLibraryLibsAar`** (the post-processor still copies the file into **`unityLibrary/libs/`** for Gradle).

## Gradle post-processor

**`BidscubeAndroidGradlePostprocessor`** runs after Gradle is generated and:

- Injects the dependency block (marker `// __BIDSCUBE_SDK_GRADLE_DEPS__`) including the **core** line per **`CoreDependencyMode`** (default: **`files('libs/bidscube-sdk-….aar')`** + copy). Legacy exports with a plain Maven coordinate (no **`@aar`**) are rewritten on the next export when mode is **`MavenBidscubeSdkAar`**.
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

- **Do not** add a second core `implementation` for Bidscube (Maven + **`files(...)`** + **`project(...)`** twice) — duplicate classes / `isInitialized` confusion.
- **Do not** drop **`applovin-bidscube-max-adapter-*.aar`** from the package if you use Bidscube on MAX Android.
- Offline / air-gapped builds: either pre-populate Gradle caches for default **Maven** mode, **vendor the core AAR** and use **`CustomGradleLines`** / **`SkipInjectionIntegratorOwnsCore`**, or use an **internal Maven** repository — no hard dependency on **Maven Central** specifically.
