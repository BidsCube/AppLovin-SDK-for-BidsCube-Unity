# Bidscube AppLovin MAX adapter (`com.bidscube.applovin.max`)

UPM package **1.0.27** (Git tag **`v1.0.27`**) — wires the Bidscube **AppLovin MAX** native Android/iOS adapters into Unity and provides optional reflection helpers to **`MaxSdk`**. **Requires** the core Unity SDK **`com.bidscube.sdk`** (declared as **1.2.17** in this package’s `package.json`).

This package does **not** implement AppLovin mediation logic. Load and show ads through the **official AppLovin MAX Unity SDK** (`MaxSdk`); native **`BidscubeMediationAdapter`** / **`ALBidscubeMediationAdapter`** handle inventory.

**Supported through MAX:** Banner, MREC, Interstitial, Rewarded.

**Not supported:** Native MAX (unless native Android/iOS adapters implement real native asset mapping — not advertised in this release).

OpenRTB 2.6 support, when available, is provided by the native Bidscube SDKs used by the native AppLovin MAX adapters. The Unity package does not parse OpenRTB responses and does not build or POST OpenRTB bid requests.

**Release status (Option C — current native artifacts):** OpenRTB 2.6-style response parsing is not implemented yet in the bundled native MAX stack. Do not claim OpenRTB 2.6 support for AppLovin MAX mediation in this UPM release.

| Platform | OpenRTB 2.6-style response parsing | Native artifacts |
|----------|-----------------------------------|------------------|
| **Android** | Not implemented (native core **`1.2.19`** + adapter **`1.2.19`**) | Bundled AARs; adapter signal `openrtb_2_6_response_parsing: false` |
| **iOS** | Not implemented | `BidscubeSDKAppLovin` **1.1.2** — OpenRTB 2.6-style podded video response parsing is not implemented on iOS yet. The adapter does not build or POST OpenRTB bid requests. |

Expected flow when native support ships:

```text
Unity game → official MAX Unity SDK → native MAX adapter → native Bidscube SDK → OpenRTB 2.6-style response parser
```

Full install, manifest snippets, dashboard, and troubleshooting: **[Documentation~/INSTALL.md](Documentation~/INSTALL.md)**.

## AppLovin MAX Setup

1. Add this package to the Unity project.
2. Add or verify **`com.bidscube.sdk`** (see `package.json`; host app usually pins [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) **#v1.2.17**) and the **official AppLovin MAX Unity SDK** — this adapter does **not** ship **`MaxSdk`**. Android TV and Apple TV: [Documentation~/TV.md](Documentation~/TV.md).
3. Open the sample scene (**Package Manager → Samples → SDK Demo**) or your integration scene.
4. Enter the AppLovin MAX **SDK key**.
5. Enter AppLovin **ad unit IDs**.
6. Run **External Dependency Manager → Android Resolver → Force Resolve**.
7. Build an **Android APK** (**File → Build Settings → Android**).

## Usage examples

### 1. Add packages (`Packages/manifest.json`)

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.17",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.27"
  }
}
```

Also install the **official AppLovin MAX Unity plugin** (scoped registry). Without it, `MaxSdk` is unavailable.

### 2. Initialize Bidscube, then MAX (mediation mode)

In **AppLovin MAX mode**, initialize Bidscube first with **`BidscubeIntegrationMode.AppLovinMaxMediation`**, then call **`MaxSdk.InitializeSdk()`**. Load and show creatives only through **`MaxSdk`** — do not call Bidscube `ShowImageAd` / `ShowVideoAd` for the same inventory.

```csharp
using System.Collections;
using BidscubeSDK;
using BidscubeSDK.Mediation;
using UnityEngine;

public class BidscubeMaxBootstrap : MonoBehaviour
{
    [SerializeField] private string _appLovinSdkKey = "YOUR_MAX_SDK_KEY";
    [SerializeField] private string _interstitialAdUnitId = "YOUR_INTERSTITIAL_AD_UNIT";
    [SerializeField] private string _rewardedAdUnitId = "YOUR_REWARDED_AD_UNIT";
    [SerializeField] private string _bidscubePlacementId = "your_bidscube_placement";

    private void Start()
    {
        var config = new SDKConfig.Builder()
            .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
            .EnableLogging(true)
            .AutoClose(false) // default: keep end card / last frame until user closes
            .UserId("your-publisher-user-id") // sent as user_id on SSP requests for postbacks
            .Build();

        BidscubeSDK.Initialize(config);

        if (!BidscubeSDK.IsInitialized())
        {
            Debug.LogError("[Bidscube] SDK init failed.");
            return;
        }

        StartCoroutine(InitializeMaxAndPreload());
    }

    private IEnumerator InitializeMaxAndPreload()
    {
        if (!AppLovinMaxUnityReflection.IsMaxSdkAvailable)
        {
            Debug.LogError("[MAX] Official MAX Unity plugin not found.");
            yield break;
        }

        AppLovinMaxUnityReflection.TrySetSdkKey(_appLovinSdkKey);
        AppLovinMaxUnityReflection.TryInitializeSdk();

        const float timeoutSec = 45f;
        var waited = 0f;
        while (!AppLovinMaxUnityReflection.TryIsInitialized() && waited < timeoutSec)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!AppLovinMaxUnityReflection.TryIsInitialized())
        {
            Debug.LogError("[MAX] Timed out waiting for MaxSdk.IsInitialized().");
            yield break;
        }

        AppLovinMaxUnityReflection.TryLoadInterstitial(_interstitialAdUnitId);
        AppLovinMaxRewardedBridge.LoadRewarded(_rewardedAdUnitId);
    }
}
```

If you already reference the MAX assembly directly, use `MaxSdk` / `MaxSdkCallbacks` instead of `AppLovinMaxUnityReflection`. The reflection helper only forwards to the official MAX Unity SDK when installed; it is not a separate mediation layer.

### 3. Interstitial video (MAX only, no reward)

The native adapter maps MAX interstitial to Bidscube **`showInterstitialVideoAd`**. Reward callbacks are never fired for interstitial.

```csharp
using UnityEngine;

public class MaxInterstitialExample : MonoBehaviour
{
    [SerializeField] private string _interstitialAdUnitId;

    private void OnEnable()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (_, __) => Debug.Log("[MAX] Interstitial displayed");
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (_, __) =>
        {
            Debug.Log("[MAX] Interstitial hidden — preload next");
            MaxSdk.LoadInterstitial(_interstitialAdUnitId);
        };
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (_, error, __) =>
            Debug.LogWarning("[MAX] Interstitial display failed: " + error.Message);
    }

    private void OnDisable()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoaded;
    }

    private void OnInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("[MAX] Interstitial loaded: " + adUnitId);
    }

    public void ShowInterstitialIfReady()
    {
        if (!MaxSdk.IsInterstitialReady(_interstitialAdUnitId))
        {
            Debug.Log("[MAX] Interstitial not ready — loading…");
            MaxSdk.LoadInterstitial(_interstitialAdUnitId);
            return;
        }

        MaxSdk.ShowInterstitial(_interstitialAdUnitId);
    }
}
```

### 4. Rewarded video (MAX primary; optional direct SDK fallback)

Primary path: **`MaxSdk.LoadRewardedAd`** / **`MaxSdk.ShowRewardedAd`**. MAX **`OnAdReceivedRewardEvent`** fires only after Bidscube core **`onUserRewarded`** (skip / close / error does not reward).

**Direct SDK fallback is disabled by default.** When MAX has no fill or the ad is not ready, you may opt in to **`AppLovinMaxRewardedBridge`** for Bidscube direct rewarded (`ShowRewardedVideoAd` on core **1.2.12+**, else `ShowVideoAd`):

> Direct SDK fallback bypasses AppLovin MAX mediation priority and should not be enabled for the same inventory unless product/monetization explicitly approves it.

```csharp
using BidscubeSDK;
using BidscubeSDK.Mediation;
using UnityEngine;

public class MaxRewardedExample : MonoBehaviour, IAdCallback
{
    [SerializeField] private string _rewardedAdUnitId;
    [SerializeField] private string _bidscubePlacementId;

    private void OnEnable()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += (_, __) => Debug.Log("[MAX] Rewarded loaded");
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (_, reward, __) =>
        {
            Debug.Log("[MAX] User rewarded: " + reward.Label + " / " + reward.Amount);
            GrantReward();
        };
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (_, __) =>
        {
            AppLovinMaxRewardedBridge.LoadRewarded(_rewardedAdUnitId);
        };
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (_, error, __) =>
            Debug.LogWarning("[MAX] Rewarded display failed: " + error.Message);
    }

    public void ShowRewarded()
    {
        AppLovinMaxRewardedBridge.LoadRewarded(_rewardedAdUnitId);

        var usedMax = AppLovinMaxRewardedBridge.ShowRewarded(
            _rewardedAdUnitId,
            _bidscubePlacementId,
            this);

        if (usedMax)
            Debug.Log("[MAX] ShowRewardedAd via MAX");
        else
            Debug.Log("[Bidscube] Direct SDK rewarded fallback");
    }

    private void GrantReward()
    {
        // coins, extra life, etc.
    }

    // IAdCallback — used only on direct SDK fallback path
    public void OnAdLoaded(string placementId) { }
    public void OnAdDisplayed(string placementId) { }
    public void OnAdClosed(string placementId) { }
    public void OnAdFailed(string placementId, int errorCode, string errorMessage)
        => Debug.LogWarning("[Bidscube fallback] failed: " + errorMessage);
    public void OnVideoAdCompleted(string placementId) => GrantReward();
    // … implement remaining IAdCallback members from com.bidscube.sdk
}
```

Disable fallback to keep MAX-only behavior (default):

```csharp
// Default is false; set true only for explicit QA/debug fallback:
AppLovinMaxRewardedBridge.EnableDirectSdkFallback = true;
```

Check readiness before show:

```csharp
if (AppLovinMaxRewardedBridge.IsMaxRewardedReady(rewardedAdUnitId))
{
    MaxSdk.ShowRewardedAd(rewardedAdUnitId);
}
```

### 5. Banner and MREC (optional)

Banners and MRECs use standard MAX APIs. The Bidscube adapter handles native inventory configured in the MAX dashboard.

```csharp
using BidscubeSDK.Mediation;

public void ShowMaxBanner(string bannerAdUnitId)
{
    if (AppLovinMaxUnityReflection.TryCreateBannerBottomCenter(bannerAdUnitId))
        AppLovinMaxUnityReflection.TryShowBanner(bannerAdUnitId);
}

public void ShowMaxMRec(string mrecAdUnitId)
{
    if (AppLovinMaxUnityReflection.TryCreateMRec(mrecAdUnitId))
        AppLovinMaxUnityReflection.TryShowMRec(mrecAdUnitId);
}

public void HideMaxBanner(string bannerAdUnitId)
{
    AppLovinMaxUnityReflection.TryHideBanner(bannerAdUnitId);
    AppLovinMaxUnityReflection.TryDestroyBanner(bannerAdUnitId);
}
```

### 6. End-to-end manager (copy-paste starting point)

Minimal manager that wires init, interstitial, and rewarded with fallback — see also **Samples~/SDK Demo** (`BidscubeExampleScene`, mode **AppLovin MAX**):

```csharp
using System.Collections;
using BidscubeSDK;
using BidscubeSDK.Mediation;
using UnityEngine;

public class BidscubeMaxAdsManager : MonoBehaviour, IAdCallback
{
    [Header("MAX")]
    [SerializeField] private string _sdkKey;
    [SerializeField] private string _interstitialId;
    [SerializeField] private string _rewardedId;

    [Header("Bidscube fallback")]
    [SerializeField] private string _placementId;

    private bool _maxReady;

    public void InitializeAds()
    {
        BidscubeSDK.Initialize(new SDKConfig.Builder()
            .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
            .EnableLogging(true)
            .Build());

        StartCoroutine(InitMaxCoroutine());
    }

    private IEnumerator InitMaxCoroutine()
    {
        AppLovinMaxUnityReflection.TrySetSdkKey(_sdkKey);
        AppLovinMaxUnityReflection.TryInitializeSdk();

        while (!AppLovinMaxUnityReflection.TryIsInitialized())
            yield return null;

        _maxReady = true;
        AppLovinMaxUnityReflection.TryLoadInterstitial(_interstitialId);
        AppLovinMaxRewardedBridge.LoadRewarded(_rewardedId);
    }

    public void ShowInterstitial()
    {
        if (!_maxReady || !AppLovinMaxUnityReflection.TryIsInterstitialReady(_interstitialId))
        {
            AppLovinMaxUnityReflection.TryLoadInterstitial(_interstitialId);
            return;
        }
        AppLovinMaxUnityReflection.TryShowInterstitial(_interstitialId);
    }

    public void ShowRewarded() =>
        AppLovinMaxRewardedBridge.ShowRewarded(_rewardedId, _placementId, this);

    public void OnAdFailed(string p, int c, string m) { }
    public void OnAdLoaded(string p) { }
    public void OnAdDisplayed(string p) { }
    public void OnAdClosed(string p) { }
    public void OnVideoAdCompleted(string p) { /* fallback reward */ }
    // … remaining IAdCallback stubs
}
```

### Mapping reference

| MAX API | Bidscube native (adapter) | Reward |
|---------|---------------------------|--------|
| `LoadInterstitial` / `ShowInterstitial` | `showInterstitialVideoAd` (video) or `showImageAd` (static) | Never |
| `LoadRewardedAd` / `ShowRewardedAd` | `showRewardedVideoAd` | Only on `onUserRewarded` |
| `AppLovinMaxRewardedBridge` fallback | `ShowRewardedVideoAd` / `ShowVideoAd` (Unity C#) | Core SDK callbacks |

Show failures are reported as MAX **display failed**, not load failed.

## Android modes

**Android** uses the bundled Android AppLovin MAX adapter AAR and keeps Android-specific video modes:

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

## iOS

**iOS** uses one **`BidscubeSDKAppLovin`** CocoaPods dependency (currently **1.1.2**). There are no Android-style video modes on iOS. On Xcode export, **`BidscubeIosPodfilePostprocessor`** appends **`pod 'BidscubeSDKAppLovin', '<version>'`** when missing. Verify **`Podfile.lock`** contains **`BidscubeSDKAppLovin`** and the final app contains **`ALBidscubeMediationAdapter`**.

## Bundled Android AARs

- `bidscube-sdk-lite-no-video-1.2.19.aar`
- `bidscube-sdk-webview-video-1.2.19.aar`
- `bidscube-sdk-legacy-media-video-1.2.19.aar`
- `bidscube-sdk-full-video-1.2.19.aar`
- `applovin-bidscube-max-adapter-1.2.19.aar`

Native core **`1.2.19`** matches MAX adapter **`1.2.19`**. Android TV fullscreen and remote focus follow the direct BidsCube SDK. See `Documentation~/TV.md`.

Full MAX QA: sibling project **`BidscubeUnityAppLovinTestApp`** (see its README).

## AppLovin MAX server parameters

The Unity package cannot set AppLovin MAX **server parameters** at runtime. Configure them in the **AppLovin MAX dashboard** (custom network → Bidscube).

`app_id` is an optional AppLovin MAX server parameter. It is not a BidsCube id and it is not sent on SSP requests. The ad-unit **Placement ID** is the only id sent as query `id`.

| Parameter | Status (native adapters **1.2.19** / **`BidscubeSDKAppLovin` 1.1.2**) |
|-----------|------------------------------------------------------------------------|
| **`app_id`** | **Optional.** Empty does not block Android init. When set, it is passed to `SDKConfig.appId` and does not replace `bundle`. |
| **`request_authority`** | **Optional.** SSP host override (also via `SDKConfig.Builder.AdRequestAuthority` before MAX init). Empty keeps **`ssp-bcc-ads.com`**. |
| **`ssp_host`** | **Optional.** Alias for `request_authority` when that key is empty. Same default host. |
| **`user_id`** / **`userId`** | **Active** — publisher user id for postbacks (also via `SDKConfig.Builder().UserId(...)` / `BidscubeSDK.SetUserId(...)` before MAX init) |
| **`auto_close`** / **`autoClose`** | **Active** — init server parameter; `true` auto-dismisses fullscreen video after linear complete/skip; default **`false`** (Companion end card / last frame until manual close). Also via `SDKConfig.Builder().AutoClose(...)` before MAX init. |
| **`openrtb_pod_metadata_enabled`** | **Reserved / future** — not read by current native MAX adapters |
| **`video_pod_duration_validation_mode`** | **Reserved / future** |
| **`video_pod_skip_policy`** | **Reserved / future** |
| **`video_pod_continue_on_slot_error`** | **Reserved / future** |
| **`video_pod_show_counter`** | **Reserved / future** |

### Placement 22701

Rewarded traffic for a BidsCube placement is the MAX **Placement ID** on that ad unit. **22701** is that placement. **22340** is a different placement: a load with Placement ID 22340 never requests 22701.

After the SDK initializes, rewarded load builds:

`https://ssp-bcc-ads.com/sdk?c=v&m=xml&id=22701&app=1&bundle=io.shusmo.idle.blockcraft&...`

`bundle` is the Android package name. `id` is the Placement ID. Leave `request_authority` and `ssp_host` empty unless the host is not `ssp-bcc-ads.com`. Leave `app_id` empty. Do not put `22701` or `22340` in that field.

Bundled AARs are **1.2.19**. Unity package **v1.0.25** bundled **1.2.11**, which still failed init when `app_id` was empty. **1.2.4** is not this line.

## More

- [CHANGELOG.md](CHANGELOG.md)  
- [LICENSE.md](LICENSE.md)  
- Maintainers: [RELEASE.md](RELEASE.md), [docs/internal/README.md](docs/internal/README.md)
