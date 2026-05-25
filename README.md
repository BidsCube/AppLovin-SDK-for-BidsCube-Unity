# Bidscube AppLovin MAX adapter (`com.bidscube.applovin.max`)

UPM package **1.0.22** (Git tag **`v1.0.22`**) — adds the Bidscube **AppLovin MAX** Android/iOS bridge, bundled native AARs, and reflection hooks to **`MaxSdk`**. **Requires** the core Unity SDK **`com.bidscube.sdk`** (declared as **1.2.12** in this package’s `package.json`).

Full install, manifest snippets, dashboard, and troubleshooting: **[Documentation~/INSTALL.md](Documentation~/INSTALL.md)**.

## AppLovin MAX Setup

1. Add this package to the Unity project.
2. Add or verify **`com.bidscube.sdk`** (see `package.json`; host app usually pins [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) **#v1.2.12**) and the **official AppLovin MAX Unity SDK** — this adapter does **not** ship **`MaxSdk`**.
3. Open the sample scene (**Package Manager → Samples → SDK Demo**) or your integration scene.
4. Enter the AppLovin MAX **SDK key**.
5. Enter AppLovin **ad unit IDs**.
6. Run **External Dependency Manager → Android Resolver → Force Resolve**.
7. Build an **Android APK** (**File → Build Settings → Android**).

## Interstitial Video and Rewarded Video (MAX)

Rewarded Video and Interstitial Video are separated at the adapter level.

**Interstitial Video**

- AppLovin MAX uses **`LoadInterstitial`** / **`ShowInterstitial`**
- Bidscube adapter maps it to Bidscube interstitial video flow (`showInterstitialVideoAd`)
- No reward callback is ever fired

**Rewarded Video**

- AppLovin MAX uses **`LoadRewardedAd`** / **`ShowRewardedAd`**
- Bidscube adapter maps it to Bidscube rewarded video flow (`showRewardedVideoAd`)
- MAX reward is triggered only after Bidscube core SDK **`onUserRewarded`**
- Skip / close / error does not reward the user

**AppLovin MAX mode = use `MaxSdk` APIs** for the primary path. For rewarded, when MAX has no fill, use **`AppLovinMaxRewardedBridge`** (see below) instead of calling core SDK show APIs directly elsewhere in app code.

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

When MAX rewarded is not ready (no fill, load still pending, or MAX plugin missing), use **`AppLovinMaxRewardedBridge`** to fall back to the Bidscube core SDK:

```csharp
using BidscubeSDK.Mediation;

// Load MAX first (optional)
AppLovinMaxRewardedBridge.LoadRewarded(rewardedAdUnitId);

// MAX if ready; otherwise Bidscube ShowRewardedVideoAd / ShowVideoAd on bidscubePlacementId
AppLovinMaxRewardedBridge.ShowRewarded(rewardedAdUnitId, bidscubePlacementId, adCallback);
```

Set **`AppLovinMaxRewardedBridge.EnableDirectSdkFallback = false`** to disable fallback and keep MAX-only behavior.

## Android modes

**LiteNoVideo**

- uses `bidscube-sdk-lite-no-video`
- no video support
- rewarded video will fail as unsupported
- interstitial video will fail as unsupported if video path is requested
- no Media3 / Google IMA
- no core library desugaring

**WebViewVideoNoDesugar**

- uses `bidscube-sdk-webview-video`
- supports video without desugaring (HTML5 video through Android WebView)
- reward depends on core SDK completion/reward callback
- no Media3 / Google IMA
- no core library desugaring

**LegacyMediaVideoNoDesugar**

- uses `bidscube-sdk-legacy-media-video`
- supports video via legacy Android media player (`VideoView` / `MediaPlayer`)
- reward depends on core SDK completion/reward callback
- no Media3 / Google IMA
- no core library desugaring

**FullWithVideo**

- uses `bidscube-sdk-full-video`
- full video support with Google IMA / Media3
- reward depends on core SDK completion/reward callback
- launcher desugaring may be enabled

Toggle in Unity: **Tools → Bidscube SDK → Android Build Features**, or use a **Bidscube Android Export Settings** asset (see INSTALL).

`LiteNoVideo`, `WebViewVideoNoDesugar`, and `LegacyMediaVideoNoDesugar` should build without `coreLibraryDesugaringEnabled`. `FullWithVideo` may inject:

```gradle
coreLibraryDesugaringEnabled true
coreLibraryDesugaring "com.android.tools:desugar_jdk_libs:2.0.4"
```

## Bundled Android AARs

- `bidscube-sdk-lite-no-video-1.2.5.aar`
- `bidscube-sdk-webview-video-1.2.5.aar`
- `bidscube-sdk-legacy-media-video-1.2.5.aar`
- `bidscube-sdk-full-video-1.2.5.aar`
- `applovin-bidscube-max-adapter-1.2.6.aar`

## More

- [CHANGELOG.md](CHANGELOG.md)  
- [LICENSE.md](LICENSE.md)  
- Maintainers: [RELEASE.md](RELEASE.md), [docs/internal/README.md](docs/internal/README.md)
