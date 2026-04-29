# Bundled Bidscube MAX adapter + core Android SDK (Android)

> **UPM `com.bidscube.applovin.max` 1.0.12+** ships the **AARs** and **C#** `AppLovinMaxUnityReflection` only. The **Editor** `BidscubeAndroidGradlePostprocessor` / `BidscubeAndroidExportSettings` flow described in this file was part of **older** monolithic packages; if your tree has no `Editor/Android/*.cs` from this repo, apply the **Gradle** / **`unityLibrary`** steps **manually** (or use automation from **`com.bidscube.sdk`** when your version includes it). The **dependency** and **layout** sections below stay valid as **reference**.

## Version matrix (align with `package.json` and `AdapterPackageInfo`)

| Item | Version | Where it is set |
|------|--------:|-----------------|
| UPM **this** package (`com.bidscube.applovin.max`) | **1.0.12** | `package.json` → `version`; `AdapterPackageInfo.UpmVersion` in `Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs` |
| UPM core SDK peer (`com.bidscube.sdk`, Unity) | **1.2.5** | `package.json` → `dependencies` (must match what you install in the host `manifest.json`) |
| Android **MAX** adapter AAR | **1.0.4** | `applovin-bidscube-max-adapter-1.0.4.aar`; `AdapterPackageInfo.BundledMaxAdapterAarVersion` |
| Android **lite** core AAR (bundled) | **1.2.3** | `bidscube-sdk-lite-1.2.3.aar`; `AdapterPackageInfo.NativeAndroidBidscubeSdkVersion` |
| Android **full** core (IMA / video) | **1.2.3** | Not bundled in this UPM: use Maven `com.bidscube:bidscube-sdk:1.2.3@aar` and/or your own `bidscube-sdk-1.2.3.aar` in `unityLibrary/libs/` — same **semver** as lite |
| iOS **BidscubeSDKAppLovin** (CocoaPods) | **1.0.4** | Should track native releases; **1.0.4** matches the Android adapter **1.0.4** line |
| **AppLovin** Android SDK (Gradle) | **13.+** | Typically from the **official** MAX Unity plugin + your Gradle template; keep **one** resolved version |
| **AppLovin** iOS (`AppLovinSDK` pod) | **≥ 13.0.0, &lt; 14.0** | Same major line as Android **13** |

## What ships in the UPM package

- **`Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar`** — Bidscube **custom network** adapter for AppLovin MAX (`com.applovin.mediation.adapters.BidscubeMediationAdapter`). Unity **PluginImporter** enables this for **Android** so it is part of the exported Gradle project.
- **`Runtime/Plugins/Android/bidscube-sdk-lite-1.2.3.aar`** — **lite** core (no IMA) bundled in this UPM (**1.2.3** = `AdapterPackageInfo.NativeAndroidBidscubeSdkVersion`). **`.meta`** typically keeps it out of a duplicate merge; for **full** video stack use **`implementation 'com.bidscube:bidscube-sdk:1.2.3@aar'`** and/or add **`bidscube-sdk-1.2.3.aar`** under **`libs/`** (same version as lite).

**Primary path (default):** **`BidscubeAndroidCoreDependencyMode.BundledUnityLibraryLibsAar`** — exactly **one** `implementation files('libs/…')` line for the core, **offline-friendly** (no Maven repo required for the core AAR). Optional **`MavenBidscubeSdkAar`** / **`CustomGradleLines`** / **`SkipInjectionIntegratorOwnsCore`** unchanged.

On Gradle export, **`BidscubeAndroidGradlePostprocessor`** injects **`// __BIDSCUBE_SDK_GRADLE_DEPS__`** with AppLovin **13.+**, UMP, ads-identifier, CardView, Material, Glide, and **conditionally** Media3 + Google IMA (see **Feature set** below). **Launcher** **`coreLibraryDesugaring`** stays on by default (**`NoDesugarMode`**).

## Feature set: `LiteNoVideo` vs `FullWithVideo`

**Static (set before Android export / build, e.g. `[InitializeOnLoad]` Editor script):**

**Option A — asset (best for Git):** create **`BidscubeAndroidExportSettings`** via the **Create** menu and commit it.

**Option B — code fallback** (no asset in project):

```csharp
using BidscubeSDK.Editor.Android;

BidscubeAndroidGradlePostprocessor.FeatureSet = BidscubeAndroidFeatureSet.LiteNoVideo; // default
// or
BidscubeAndroidGradlePostprocessor.FeatureSet = BidscubeAndroidFeatureSet.FullWithVideo;
```

| `BidscubeAndroidFeatureSet` | Core JAR in `unityLibrary/libs/` | Extra Gradle `implementation` lines |
|-----------------------------|----------------------------------|---------------------------------------|
| **`LiteNoVideo`** (default) | `bidscube-sdk-lite-1.2.3.aar` (this release) | **Omits** `androidx.media3:media3-common`, `media3-ui`, `com.google.ads.interactivemedia.v3:interactivemedia` |
| **`FullWithVideo`** | `bidscube-sdk-1.2.3.aar` (obtain full AAR or Maven **1.2.3**) | **Adds** Media3 **1.4.1** + interactivemedia **3.33.0** |

**Logs (Unity Editor, Gradle export):** `Using Bidscube Android feature set: …`, `Copied bundled core AAR: …`, `Skipping Media3 / Google IMA…` or `Including Media3 and Google IMA…`.

**Player scripting define:** `BidscubeAndroidScriptingDefinesPreprocessor` adds **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`** for Android builds when **`FeatureSet == LiteNoVideo`**, so **Direct SDK** `ShowVideoAd` / `GetVideoAdView` log a clear failure without touching banner/native paths.

**Maven + lite:** if you combine **`LiteNoVideo`** with **`MavenBidscubeSdkAar`**, the resolved Maven artifact may still be a “full” SDK — prefer **bundled lite AAR** for a true minimal graph.

**MAX video:** use **`FullWithVideo`** when MAX ad units need **rewarded / video** from the native stack; **`LiteNoVideo`** targets **banner / native / image**-heavy apps (smaller dependency graph).

### Recommended for GitHub / teams / CI

1. In the **game** repository (not the UPM package), create **`Assets → Create → Bidscube → Android Export Settings`**.
2. Set **`featureSet`** to **`LiteNoVideo`** or **`FullWithVideo`** and **commit the `.asset`** so every developer and headless CI run the same Gradle graph without custom `[InitializeOnLoad]` scripts.
3. Keep **at most one** such asset in the project (the resolver uses the first match from `AssetDatabase.FindAssets`).
4. **`BidscubeAndroidGradlePostprocessor.FeatureSet`** remains the **fallback** when no asset exists (e.g. quick local tests, or minimal samples).

This layout keeps the **MAX adapter** self-contained while **one** core SDK variant sits on the classpath.

**Maven coordinate (optional mode only):** `implementation 'com.bidscube:bidscube-sdk:1.2.3@aar'` — **`@aar`** avoids POM-only resolution (version **1.2.3** = native core; keep in sync with `AdapterPackageInfo.NativeAndroidBidscubeSdkVersion`). Gradle resolves from **your** declared repositories when using **`MavenBidscubeSdkAar`**.

## Core SDK resolution modes (`BidscubeAndroidGradlePostprocessor`)

Set **before** Android export / build (e.g. in a small **`[InitializeOnLoad]`** Editor script in your game assembly that references **`BidscubeSDK.Editor`**):

| `CoreDependencyMode` | Behaviour |
|----------------------|-----------|
| **`BundledUnityLibraryLibsAar`** (default) | Copies **`bidscube-sdk-lite-….aar`** or **`bidscube-sdk-….aar`** into **`unityLibrary/libs/`** per **`FeatureSet`**, then injects **`implementation files('libs/…')`**. **No Maven** needed for the core AAR. |
| **`MavenBidscubeSdkAar`** | Injects `implementation 'com.bidscube:bidscube-sdk:1.2.3@aar'` (match `AdapterPackageInfo.NativeAndroidBidscubeSdkVersion`). Host Gradle must expose a repo that serves that artifact (Central, internal mirror, etc.). |
| **`CustomGradleLines`** | Injects **`CustomCoreImplementationGradleLines`** after the marker instead of the bundled / Maven line. Use for a **custom path**, **`flatDir` + `name:`**, **`project(':module')`**, etc. After export, confirm **`unityLibrary/build.gradle`** and the path Unity uses for your AAR. |
| **`SkipInjectionIntegratorOwnsCore`** | Does **not** inject core; you must add **exactly one** core SDK line elsewhere (Custom Base Gradle, `mainTemplate`, or another module). The post-processor logs a **warning** if it cannot detect a `implementation` referencing bidscube. |

Example (local AAR next to Unity-exported libs — verify path on your Unity version):

```csharp
using BidscubeSDK.Editor.Android;
// In InitializeOnLoad or pre-export hook:
BidscubeAndroidGradlePostprocessor.CoreDependencyMode = BidscubeAndroidCoreDependencyMode.CustomGradleLines;
BidscubeAndroidGradlePostprocessor.CustomCoreImplementationGradleLines =
    "implementation files('libs/bidscube-sdk-lite-1.2.3.aar')\n";
```

**Backward compatibility:** older samples assumed **`MavenBidscubeSdkAar`**. To restore that behaviour (core from Gradle repos only), set **`BidscubeAndroidGradlePostprocessor.CoreDependencyMode = BidscubeAndroidCoreDependencyMode.MavenBidscubeSdkAar`** before export (e.g. **`[InitializeOnLoad]`** Editor script).

## Other Android dependencies (scope)

**`CoreDependencyMode` applies only to the Bidscube core SDK** (`com.bidscube.sdk.*`). The same **`BidscubeAndroidGradlePostprocessor`** block adds **AppLovin SDK**, **UMP**, **ads-identifier**, **CardView**, **Material**, **Glide** on every export, and **Media3 + Google IMA** only when **`FeatureSet == FullWithVideo`**. Those coordinates resolve through **your** Gradle repositories (Maven-compatible). A **fully Maven-free** graph for every transitive is **not** implemented; use mirrors / offline cache if needed.

## Version sync

- **`com.bidscube.applovin.max`:** `AdapterPackageInfo.UpmVersion` = `package.json` `version`. **`NativeAndroidBidscubeSdkVersion`** and **`BundledMaxAdapterAarVersion`** must match the **filenames** of the bundled AARs on disk.
- **`com.bidscube.sdk`:** the Unity peer version is **`package.json` → `dependencies` → `com.bidscube.sdk` (currently 1.2.5)** — use the same in the host project’s **`Packages/manifest.json`**.
- **Gradle / Maven:** pin **`com.bidscube:bidscube-sdk`** to the same **1.2.3** as the lite AAR when you need a consistent native stack.

## Updating the adapter AAR

From the Bidscube Android SDK repo (example):

```bash
cp applovin-adapter/build/outputs/aar/applovin-adapter-release.aar <path-to-unity-package>/Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.4.aar
```

Adjust filename / version constants / docs when the adapter semver changes.

## Updating the reference core AARs (`bidscube-sdk-*.aar` + `bidscube-sdk-lite-*.aar`)

From the Bidscube Android SDK repo, build **both** flavors for the same native version (example **1.2.3** = current `AdapterPackageInfo.NativeAndroidBidscubeSdkVersion`):

```bash
# Full (withIma) → bidscube-sdk-1.2.3.aar
cp sdk/build/outputs/aar/sdk-withIma-release.aar <path-to-unity-package>/Runtime/Plugins/Android/bidscube-sdk-1.2.3.aar
# Lite (noIma) → bidscube-sdk-lite-1.2.3.aar
cp sdk/build/outputs/aar/sdk-noIma-release.aar <path-to-unity-package>/Runtime/Plugins/Android/bidscube-sdk-lite-1.2.3.aar
```

After a bump, update **`AdapterPackageInfo`**, **AAR filenames**, and **`package.json`** / docs together. This companion package may ship **lite** only; the **full** AAR can still be added beside it for `FullWithVideo` / manual **`files('libs/…')`**. Preserve **Android: disabled** on **`.meta`** as needed so Unity does not merge duplicates.

## Gradle post-processor

**`BidscubeAndroidGradlePostprocessor`** runs after Gradle is generated and:

- Injects the dependency block (marker `// __BIDSCUBE_SDK_GRADLE_DEPS__`) including the **core** line per **`CoreDependencyMode`** and **`FeatureSet`** (default: **`files('libs/bidscube-sdk-lite-….aar')`** + lite copy). Legacy exports with a plain Maven coordinate (no **`@aar`**) are rewritten on the next export when mode is **`MavenBidscubeSdkAar`**.
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
