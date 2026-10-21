# Install: `com.bidscube.applovin.max` + `com.bidscube.sdk`

## 1. Unity packages (UPM)

In **`Packages/manifest.json`** add the core SDK and this adapter (use the Git tags you ship against):

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.17",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.27"
  }
}
```

Or **Package Manager → + → Add package from git URL** (core first, then adapter).

This adapter declares **`com.bidscube.sdk`** only. You add **official MAX** (`com.applovin.mediation.ads`, scoped registry) per [AppLovin UPM docs](https://unity.packages.applovin.com/).

## 2. AppLovin MAX Unity SDK (required)

This package does **not** contain **`MaxSdk`** or implement mediation. Install the official AppLovin MAX Unity plugin; load and show through **`MaxSdk`**. **`AppLovinMaxUnityReflection`** is an optional helper that forwards to MAX when installed.

**Supported through MAX:** Banner, MREC, Interstitial, Rewarded.

**Not supported:** Native MAX (unless native adapters implement real native asset mapping — not advertised in this release).

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

When MAX rewarded is not ready, you may opt in to **`AppLovinMaxRewardedBridge.ShowRewarded(maxAdUnitId, bidscubePlacementId, callback)`** for Bidscube SDK direct rewarded. **Direct SDK fallback is disabled by default** (`EnableDirectSdkFallback = false`).

> Direct SDK fallback bypasses AppLovin MAX mediation priority and should not be enabled for the same inventory unless product/monetization explicitly approves it.

## 4. Android

- **Minimum:** API **26+** for the bundled AARs; align **compileSdk** / Gradle with your Unity template.
- **LiteNoVideo (default):** uses **`bidscube-sdk-lite-no-video-1.2.19.aar`** — no video support; rewarded video and interstitial video (video path) fail as unsupported; no Media3 / Google IMA; no forced **`coreLibraryDesugaring`** on the launcher.
- **WebViewVideoNoDesugar:** uses **`bidscube-sdk-webview-video-1.2.19.aar`** — HTML5 video in Android WebView; reward depends on core SDK completion/reward callback; no Media3 / Google IMA; no forced **`coreLibraryDesugaring`**.
- **LegacyMediaVideoNoDesugar:** uses **`bidscube-sdk-legacy-media-video-1.2.19.aar`** — `VideoView` / `MediaPlayer` video; reward depends on core SDK completion/reward callback; no Media3 / Google IMA; no forced **`coreLibraryDesugaring`**.
- **FullWithVideo:** uses **`bidscube-sdk-full-video-1.2.19.aar`** (or Maven **`com.bidscube:sdk-full-video`** per postprocessor). Full video through Media3/IMA; may require **`coreLibraryDesugaring`**. Editor: **Tools → Bidscube SDK → Android Build Features** or **Bidscube → Android Export Settings** → **FullWithVideo**.
- **MAX adapter AAR:** **`applovin-bidscube-max-adapter-1.2.19.aar`** (bundled; release-ready — no test signal / dummy Native MAX).
- **Duplicate AAR:** if the postprocessor copies AARs into **`unityLibrary/libs/`**, disable **Android** import on the duplicate **`bidscube-sdk-*.aar`** in the Inspector so Gradle does not merge the same binary twice.

## 5. iOS (MAX)

**iOS** uses one **`BidscubeSDKAppLovin`** pod (currently **1.1.2**). There are no Android-style video modes on iOS. tvOS export does not add this pod; Apple TV uses the direct Unity video path. See [TV.md](TV.md).

On Xcode export, **`BidscubeIosPodfilePostprocessor`** appends **`pod 'BidscubeSDKAppLovin', '<version>'`** when that pod is not already declared. It does **not** duplicate **`AppLovinSDK`** if the official AppLovin MAX Unity SDK already adds it.

If a standalone **`bidscubeSdk`** pod is present, the postprocessor logs a warning — AppLovin MAX mediation requires **`BidscubeSDKAppLovin`** because it contains **`ALBidscubeMediationAdapter`**.

Verify after build: **`Podfile.lock`** contains **`BidscubeSDKAppLovin`** and the final app contains **`ALBidscubeMediationAdapter`**.

## OpenRTB 2.6 (native only — not Unity C#)

OpenRTB 2.6 support, when available, is provided by the native Bidscube SDKs used by the native AppLovin MAX adapters. The Unity package does not parse OpenRTB responses and does not build or POST OpenRTB bid requests.

**Release status (Option C):** OpenRTB 2.6-style response parsing is not implemented yet in the native MAX adapter stack bundled/pinned by this package.

| Platform | Status |
|----------|--------|
| **Android** (`bidscube-sdk-*` **1.2.19** + `applovin-bidscube-max-adapter` **1.2.19**) | Not implemented — raw VAST and root JSON **`adm`** only; no `bids[]`, `seatbid[].bid[]`, `slotinpod`, `poddur`, `rqddurs` pod playback |
| **iOS** (`BidscubeSDKAppLovin` **1.1.2**) | OpenRTB 2.6-style podded video response parsing is not implemented on iOS yet. The adapter does not build or POST OpenRTB bid requests. |

When native support ships, pod ordering priority in the native parser is: **`slotinpod`** → VAST **`<Ad sequence>`** → response order.

## AppLovin MAX server parameters

Configure in the **AppLovin MAX dashboard** (not from Unity C# at runtime):

| Parameter | Status |
|-----------|--------|
| **`app_id`** | **Optional** AppLovin server parameter (1.2.19). Empty does not fail init. Not sent on the SSP request and not the Placement ID. Adapter **1.2.11** (Unity **v1.0.25**) still fails init when this is empty. |
| **`request_authority`** | **Optional** on Android/iOS. Empty keeps **`ssp-bcc-ads.com`**. |
| **`ssp_host`** | **Optional** alias when `request_authority` is empty. Same default. |
| **`user_id`** / **`userId`** | **Active** — publisher user id on SSP ad requests (postback attribution). Prefer `SDKConfig.Builder().UserId(...)` at init or `BidscubeSDK.SetUserId(...)` after login; MAX dashboard value is optional fallback. |
| **`auto_close`** / **`autoClose`** | **Active** — init server parameter (read once at first SDK init). `true` auto-dismisses fullscreen video after linear complete/skip; default **`false`** (VAST Companion end card, last frame, or manual close). Also via `SDKConfig.Builder().AutoClose(...)` before MAX init. |
| **`openrtb_pod_metadata_enabled`** | **Reserved / future** |
| **`video_pod_duration_validation_mode`** | **Reserved / future** |
| **`video_pod_skip_policy`** | **Reserved / future** |
| **`video_pod_continue_on_slot_error`** | **Reserved / future** |
| **`video_pod_show_counter`** | **Reserved / future** |

## 6. Initialization order

- If you set **SSP / `AdRequestAuthority`** via **`BidscubeSDK.Initialize`**, call **`BidscubeSDK.Initialize`** **before** **`MaxSdk.InitializeSdk`** on Android so native config matches the adapter.
- For mediation, use **`SDKConfig.Builder.IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`**, then initialize MAX. Prefer MAX load/show APIs; for rewarded without MAX fill use **`AppLovinMaxRewardedBridge.ShowRewarded(...)`** instead of ad-hoc core C# show calls.

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .AdRequestAuthority("your-ssp-host.example.com")
    .AutoClose(false) // default: keep end card / last frame until user closes
    .UserId("your-publisher-user-id")
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
// then MaxSdk.InitializeSdk(...) per AppLovin

// After login:
BidscubeSDK.BidscubeSDK.SetUserId(loggedInUserId);
```

Core API details: **`com.bidscube.sdk`** repo.

## 7. AppLovin MAX dashboard (minimum)

1. Add **Bidscube** as a **custom SDK network**.
2. **Android:** adapter class **`com.applovin.mediation.adapters.BidscubeMediationAdapter`** (bundled AAR).
3. **iOS:** **`ALBidscubeMediationAdapter`** (name per your native build).
4. On the rewarded ad unit, set **Placement ID** to the BidsCube placement. For Idle Block Miner (`io.shusmo.idle.blockcraft`) that placement is **`22701`**. **`22340`** does not request 22701.
5. Server parameter **`app_id`** is optional. From adapter **1.2.19** an empty value does not stop init. Do not put the Placement ID there. Adapter **1.2.11** (Unity package **v1.0.25**) still refuses to initialize when it is empty.
6. Leave **`request_authority`** and **`ssp_host`** unset unless the SSP host is not **`ssp-bcc-ads.com`**.
7. Optional server parameters: **`user_id`**, **`auto_close`**. OpenRTB pod parameters are **reserved/future**.

Successful Android init logs `initialize: server app_id=...`. A rewarded load for this placement then requests `https://ssp-bcc-ads.com/sdk?id=22701&bundle=io.shusmo.idle.blockcraft&...`.

## 8. Common issues

| Symptom | Action |
| --- | --- |
| No **`MaxSdk`** | Install official MAX Unity SDK. |
| MAX cannot find Bidscube adapter on iOS | Verify **`Podfile.lock`** contains **`BidscubeSDKAppLovin`** and the final app contains **`ALBidscubeMediationAdapter`**. |
| **`ClassNotFoundException` `com.bidscube.sdk.BidscubeSDK`** | One core dependency in **`unityLibrary/build.gradle`** (`libs/` file or Maven **`@aar`**). |
| **Duplicate class / DEX** | Remove duplicate core; do not import the same AAR via Unity **and** `unityLibrary/libs/`. |
| **Gradle / desugaring** | **LiteNoVideo**, **WebViewVideoNoDesugar**, and **LegacyMediaVideoNoDesugar** should not need desugaring; **FullWithVideo** may need **`desugar_jdk_libs`** on the launcher. The export also adds **`com.google.android.material:material:1.12.0`**. |
| **`attr/colorOnSurface not found`** | The Android SDK already depends on `com.google.android.material:material:1.12.0`. A Unity `files()` / `libs` AAR does not pull that POM dependency. This package adds the same coordinate during Gradle export. |
| **`CS0103` `BidscubeAndroidSdkInterop`** | **`com.bidscube.sdk` 1.2.16 / 1.2.17** shipped 31-character `.meta` GUIDs. Opening the project with this package rewrites those two GUIDs to 32 hex characters. |
| **MAX attempts, no SSP requests** | On adapter **1.2.11**, empty `app_id` stopped `initialize` before any `/sdk` call. **1.2.19** continues init. The request id is Placement ID **22701** (`Loading rewarded ad for placement: 22701`). |
| **Reward on skip/close** | Expected: no reward unless **`OnAdReceivedRewardEvent`** fires after full rewarded playback. |

SSP host: **`SDKConfig.Builder.AdRequestAuthority(...)`** (see **`com.bidscube.sdk`**).
