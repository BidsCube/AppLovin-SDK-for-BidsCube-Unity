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

## Usage examples

### 1. Add packages (`Packages/manifest.json`)

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.12",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.22"
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

If you already reference the MAX assembly directly, you can use `MaxSdk` / `MaxSdkCallbacks` instead of `AppLovinMaxUnityReflection` for init and load calls.

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

### 4. Rewarded video (MAX first, Bidscube SDK fallback)

Primary path: **`MaxSdk.LoadRewardedAd`** / **`MaxSdk.ShowRewardedAd`**. MAX **`OnAdReceivedRewardEvent`** fires only after Bidscube core **`onUserRewarded`** (skip / close / error does not reward).

When MAX has no fill or the ad is not ready, use **`AppLovinMaxRewardedBridge`** to fall back to Bidscube direct rewarded (`ShowRewardedVideoAd` on core **1.2.12+**, else `ShowVideoAd`):

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

Disable fallback to keep MAX-only behavior:

```csharp
AppLovinMaxRewardedBridge.EnableDirectSdkFallback = false;
```

Check readiness before show:

```csharp
if (AppLovinMaxRewardedBridge.IsMaxRewardedReady(rewardedAdUnitId))
{
    MaxSdk.ShowRewardedAd(rewardedAdUnitId);
}
```

### 5. Banner (optional)

Banners use the standard MAX banner APIs. The Bidscube adapter handles native banner inventory configured in the MAX dashboard.

```csharp
using BidscubeSDK.Mediation;

public void ShowMaxBanner(string bannerAdUnitId)
{
    if (AppLovinMaxUnityReflection.TryCreateBannerBottomCenter(bannerAdUnitId))
        AppLovinMaxUnityReflection.TryShowBanner(bannerAdUnitId);
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
