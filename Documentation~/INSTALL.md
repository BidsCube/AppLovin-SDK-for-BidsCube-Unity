# Install: `com.bidscube.applovin.max` + `com.bidscube.sdk`

## 1. Unity packages (UPM)

In **`Packages/manifest.json`** add the core SDK and this adapter (use the Git tags you ship against):

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.12",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.22"
  }
}
```

Or **Package Manager → + → Add package from git URL** (core first, then adapter).

This adapter declares **`com.bidscube.sdk`** only. You add **official MAX** (`com.applovin.mediation.ads`, scoped registry) per [AppLovin UPM docs](https://unity.packages.applovin.com/).

## 2. AppLovin MAX Unity SDK (required)

This package does **not** contain **`MaxSdk`**. Install the official AppLovin MAX Unity plugin; otherwise **`AppLovinMaxUnityReflection`** cannot initialize MAX.

## 3. Interstitial Video and Rewarded Video (MAX)

Rewarded Video and Interstitial Video are separated at the adapter level.

**Interstitial Video**

- AppLovin MAX uses **`LoadInterstitial`** / **`ShowInterstitial`**
- Bidscube adapter maps it to Bidscube interstitial video flow
- No reward callback is ever fired

**Rewarded Video**

- AppLovin MAX uses **`LoadRewardedAd`** / **`ShowRewardedAd`**
- Bidscube adapter maps it to Bidscube rewarded video flow
- MAX reward is triggered only after Bidscube core SDK **`onUserRewarded`**
- Skip / close / error does not reward the user

**AppLovin MAX mode = use `MaxSdk` APIs** for the primary path. For rewarded without MAX fill, use **`AppLovinMaxRewardedBridge`** (see below). The SDK/adapter does not decide when to show interstitial; the app/publisher controls timing and frequency.

```csharp
// Interstitial
MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (adUnitId, adInfo) => { };
MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (adUnitId, adInfo) => { };
MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (adUnitId, adInfo) => { };
MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (adUnitId, error, adInfo) => { };

MaxSdk.LoadInterstitial(interstitialAdUnitId);

if (MaxSdk.IsInterstitialReady(interstitialAdUnitId))
{
    MaxSdk.ShowInterstitial(interstitialAdUnitId);
}
```

```csharp
// Rewarded
MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (adUnitId, adInfo) => { };
MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += (adUnitId, adInfo) => { };
MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (adUnitId, adInfo) => { };
MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (adUnitId, error, adInfo) => { };

MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (adUnitId, reward, adInfo) =>
{
    // give reward to user
};

MaxSdk.LoadRewardedAd(rewardedAdUnitId);

if (MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
{
    MaxSdk.ShowRewardedAd(rewardedAdUnitId);
}
```

When MAX rewarded is not ready, use **`AppLovinMaxRewardedBridge.ShowRewarded(maxAdUnitId, bidscubePlacementId, callback)`** to fall back to Bidscube SDK direct rewarded (`ShowRewardedVideoAd` on core **1.2.12+**, else `ShowVideoAd`). See **README** for details.

## 4. Android

- **Minimum:** API **26+** for the bundled AARs; align **compileSdk** / Gradle with your Unity template.
- **LiteNoVideo (default):** uses **`bidscube-sdk-lite-no-video-1.2.5.aar`** — no video support; rewarded video and interstitial video (video path) fail as unsupported; no Media3 / Google IMA; no forced **`coreLibraryDesugaring`** on the launcher.
- **WebViewVideoNoDesugar:** uses **`bidscube-sdk-webview-video-1.2.5.aar`** — HTML5 video in Android WebView; reward depends on core SDK completion/reward callback; no Media3 / Google IMA; no forced **`coreLibraryDesugaring`**.
- **LegacyMediaVideoNoDesugar:** uses **`bidscube-sdk-legacy-media-video-1.2.5.aar`** — `VideoView` / `MediaPlayer` video; reward depends on core SDK completion/reward callback; no Media3 / Google IMA; no forced **`coreLibraryDesugaring`**.
- **FullWithVideo:** uses **`bidscube-sdk-full-video-1.2.5.aar`** (or Maven **`com.bidscube:sdk-full-video`** per postprocessor). Full video through Media3/IMA; may require **`coreLibraryDesugaring`**. Editor: **Tools → Bidscube SDK → Android Build Features** or **Bidscube → Android Export Settings** → **FullWithVideo**.
- **MAX adapter AAR:** **`applovin-bidscube-max-adapter-1.2.6.aar`** (bundled).
- **Duplicate AAR:** if the postprocessor copies AARs into **`unityLibrary/libs/`**, disable **Android** import on the duplicate **`bidscube-sdk-*.aar`** in the Inspector so Gradle does not merge the same binary twice.

## 5. iOS (MAX)

**Podfile:** **`AppLovinSDK`** (13.x) and **`BidscubeSDKAppLovin`** (match your native adapter release, e.g. **1.0.4**). Tune for your CI / post-build.

## 6. Initialization order

- If you set **SSP / `AdRequestAuthority`** via **`BidscubeSDK.Initialize`**, call **`BidscubeSDK.Initialize`** **before** **`MaxSdk.InitializeSdk`** on Android so native config matches the adapter.
- For mediation, use **`SDKConfig.Builder.IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`**, then initialize MAX. Prefer MAX load/show APIs; for rewarded without MAX fill use **`AppLovinMaxRewardedBridge.ShowRewarded(...)`** instead of ad-hoc core C# show calls.

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .AdRequestAuthority("your-ssp-host.example.com")
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
// then MaxSdk.InitializeSdk(...) per AppLovin
```

Core API details: **`com.bidscube.sdk`** repo.

## 7. AppLovin MAX dashboard (minimum)

1. Add **Bidscube** as a **custom SDK network**.
2. **Android:** adapter class **`com.applovin.mediation.adapters.BidscubeMediationAdapter`** (bundled AAR).
3. **iOS:** **`ALBidscubeMediationAdapter`** (name per your native build).
4. Network **App ID** for Bidscube = **Bidscube placement ID**. Optional server parameters: **`request_authority`** / **`ssp_host`**.

## 8. Common issues

| Symptom | Action |
| --- | --- |
| No **`MaxSdk`** | Install official MAX Unity SDK. |
| **`ClassNotFoundException` `com.bidscube.sdk.BidscubeSDK`** | One core dependency in **`unityLibrary/build.gradle`** (`libs/` file or Maven **`@aar`**). |
| **Duplicate class / DEX** | Remove duplicate core; do not import the same AAR via Unity **and** `unityLibrary/libs/`. |
| **Gradle / desugaring** | **LiteNoVideo**, **WebViewVideoNoDesugar**, and **LegacyMediaVideoNoDesugar** should not need desugaring; **FullWithVideo** may need **`desugar_jdk_libs`** on the launcher. |
| **Reward on skip/close** | Expected: no reward unless **`OnAdReceivedRewardEvent`** fires after full rewarded playback. |

SSP host: **`SDKConfig.Builder.AdRequestAuthority(...)`** (see **`com.bidscube.sdk`**).
