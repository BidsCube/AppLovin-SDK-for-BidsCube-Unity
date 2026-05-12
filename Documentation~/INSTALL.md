# Install: `com.bidscube.applovin.max` + `com.bidscube.sdk`

## 1. Unity packages (UPM)

In **`Packages/manifest.json`** add the core SDK and this adapter (use the Git tags you ship against):

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.11",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.21"
  }
}
```

Or **Package Manager → + → Add package from git URL** (core first, then adapter).

This adapter declares **`com.bidscube.sdk`** only. You add **official MAX** (`com.applovin.mediation.ads`, scoped registry) per [AppLovin UPM docs](https://unity.packages.applovin.com/).

## 2. AppLovin MAX Unity SDK (required)

This package does **not** contain **`MaxSdk`**. Install the official AppLovin MAX Unity plugin; otherwise **`AppLovinMaxUnityReflection`** cannot initialize MAX.

## 3. Android

- **Minimum:** API **26+** for the bundled AARs; align **compileSdk** / Gradle with your Unity template.
- **LiteNoVideo (default):** uses **`bidscube-sdk-lite-no-video-1.2.5.aar`** — no rewarded/video support, no Media3 / Google IMA, no forced **`coreLibraryDesugaring`** on the launcher.
- **WebViewVideoNoDesugar:** uses **`bidscube-sdk-webview-video-1.2.5.aar`** — HTML5 video in Android WebView, no Media3 / Google IMA, no forced **`coreLibraryDesugaring`**.
- **LegacyMediaVideoNoDesugar:** uses **`bidscube-sdk-legacy-media-video-1.2.5.aar`** — `VideoView` / `MediaPlayer` video, no Media3 / Google IMA, no forced **`coreLibraryDesugaring`**.
- **FullWithVideo:** uses **`bidscube-sdk-full-video-1.2.5.aar`** (or Maven **`com.bidscube:sdk-full-video`** per postprocessor). Editor: **Tools → Bidscube SDK → Android Build Features** or **Bidscube → Android Export Settings** → **FullWithVideo**.
- **Duplicate AAR:** if the postprocessor copies AARs into **`unityLibrary/libs/`**, disable **Android** import on the duplicate **`bidscube-sdk-*.aar`** in the Inspector so Gradle does not merge the same binary twice.

## 4. iOS (MAX)

**Podfile:** **`AppLovinSDK`** (13.x) and **`BidscubeSDKAppLovin`** (match your native adapter release, e.g. **1.0.4**). Tune for your CI / post-build.

## 5. Initialization order

- If you set **SSP / `AdRequestAuthority`** via **`BidscubeSDK.Initialize`**, call **`BidscubeSDK.Initialize`** **before** **`MaxSdk.InitializeSdk`** on Android so native config matches the adapter.
- For mediation, use **`SDKConfig.Builder.IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`**, then initialize MAX. In this mode do **not** use core C# show APIs (`GetBannerAdView`, `ShowVideoAd`, …) — use MAX only.

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .AdRequestAuthority("your-ssp-host.example.com")
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
// then MaxSdk.InitializeSdk(...) per AppLovin
```

Core API details: **`com.bidscube.sdk`** repo.

## 6. AppLovin MAX dashboard (minimum)

1. Add **Bidscube** as a **custom SDK network**.
2. **Android:** adapter class **`com.applovin.mediation.adapters.BidscubeMediationAdapter`** (bundled AAR).
3. **iOS:** **`ALBidscubeMediationAdapter`** (name per your native build).
4. Network **App ID** for Bidscube = **Bidscube placement ID**. Optional server parameters: **`request_authority`** / **`ssp_host`**.

## 7. Common issues

| Symptom | Action |
| --- | --- |
| No **`MaxSdk`** | Install official MAX Unity SDK. |
| **`ClassNotFoundException` `com.bidscube.sdk.BidscubeSDK`** | One core dependency in **`unityLibrary/build.gradle`** (`libs/` file or Maven **`@aar`**). |
| **Duplicate class / DEX** | Remove duplicate core; do not import the same AAR via Unity **and** `unityLibrary/libs/`. |
| **Gradle / desugaring** | **LiteNoVideo**, **WebViewVideoNoDesugar**, and **LegacyMediaVideoNoDesugar** should not need desugaring; **FullWithVideo** may need **`desugar_jdk_libs`** on the launcher. |

SSP host: **`SDKConfig.Builder.AdRequestAuthority(...)`** (see **`com.bidscube.sdk`**).
