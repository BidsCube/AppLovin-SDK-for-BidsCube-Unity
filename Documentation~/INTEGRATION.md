# Bidscube Unity SDK Integration Guide

**AppLovin MAX:** start with `Documentation~/APPLOVIN_MAX.md`. **Which player (default vs custom, Unity vs native Android):** `Documentation~/VIDEO_PLAYBACK.md`.

This guide focuses on **direct** Unity/C# usage (creatives from C#), init, logs, and API examples.

## Table of Contents

1. [Installation](#installation)
2. [Initialization](#initialization)
3. [Configure SDK in code (detailed)](#configure-sdk-in-code-detailed)
4. [Verifying the integration (logs)](#verifying-the-integration-logs)
5. [Video playback paths](#video-playback-paths)
6. [Configuration](#configuration)
7. [Showing Ads](#showing-ads)
8. [Ad Callbacks](#ad-callbacks)
9. [Ad Positioning](#ad-positioning)
10. [Consent Management](#consent-management)
11. [Examples](#examples)
12. [SDK Test Scene](#sdk-test-scene)

---

## Installation

The Bidscube Unity SDK can be installed in two ways:

### Unity Package Manager

1. In Unity Editor, open the Package Manager (`Window` → `Package Manager`)
2. Click the `+` button in the top-left corner
3. Select `Add package from git URL...`
4. Enter the repository URL: `https://github.com/BidsCube/AppLovin-SDK-Unity.git`  
   The public **Bidscube Unity SDK** sample line is [github.com/BidsCube/bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) (same `com.bidscube.sdk` name; **AppLovin-SDK-Unity** adds bundled Android SDK + MAX adapter AARs, Gradle AppLovin 13+, and optional Podfile hook — see `README.md`).
5. Optionally, specify a version tag: `https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.8`
6. Click `Add`
7. The SDK will be added as a package dependency

**Note:** For Git import, you may need to add the package to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.8"
  }
}
```

---

## Initialization

Before using the SDK, you must initialize it. The best place to do this is in your game's startup script (e.g., in a `GameManager` or `SDKInitializer` script).

### Basic Initialization

```csharp
using BidscubeSDK;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Initialize with default settings
        BidscubeSDK.BidscubeSDK.Initialize();
    }
}
```

### Advanced Initialization with Configuration

```csharp
using BidscubeSDK;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Create custom configuration
        var config = new SDKConfig.Builder()
            .EnableLogging(true)                    // Enable SDK logging
            .EnableDebugMode(false)                  // Disable debug mode for production
            .EnableTestMode(false)                  // Optional native test mode (Android when supported)
            .IntegrationMode(BidscubeIntegrationMode.DirectSdk)
            .DefaultAdTimeout(30000)                  // 30 second timeout
            .DefaultAdPosition(AdPosition.Unknown)    // Default position (centered)
            .AdRequestAuthority("ssp-bcc-ads.com")  // SSP host (SDK builds https://…/sdk)
            .Build();

        // Initialize with configuration
        BidscubeSDK.BidscubeSDK.Initialize(config);
    }
}
```

### Check if SDK is Initialized

```csharp
if (BidscubeSDK.BidscubeSDK.IsInitialized())
{
    // SDK is ready to use
}
```

`IsInitialized()` is **true** after a successful `Initialize(config)` / `Initialize()` — it reflects **C# configuration only**. On **Android device builds**, also read the **`Init (Android Java):`** / **`Init (publisher row):`** lines (see [Verifying the integration (logs)](#verifying-the-integration-logs)) to confirm the optional native `com.bidscube.sdk.BidscubeSDK` sync.

---

## Configure SDK in code (detailed)

Use **one** startup path (e.g. `Awake` / first scene / bootstrap `MonoBehaviour`) so every option lives on `SDKConfig.Builder` before `BidscubeSDK.Initialize(config)`.

| Step | What to set | Notes |
|------|-------------|--------|
| 1 | **`IntegrationMode(...)`** | **`DirectSdk`** (default) for `ShowVideoAd`, banners, native from C#. **`AppLovinMaxMediation`** for MAX-only — C# creative APIs **throw**; init still recommended on Android **before** `MaxSdk.InitializeSdk`. |
| 2 | **`EnableLogging(true)`** for QA | All lines below are suppressed when `false`. |
| 3 | **`AdRequestAuthority(...)`** or **`BaseURL(...)`** | Same normalization — host or `https://host/sdk` prefix; see `AD_REQUEST_ENDPOINT.md`. |
| 4 | **`DefaultAdTimeout`**, **`DefaultAdPosition`** | Request / layout defaults. |
| 5 | **`EnableTestMode`**, **`EnableDebugMode`** | Forwarded to native Android builder when supported; extra `DEBUG:` logs when debug on. |
| 6 | **`VideoPlaybackFactory(...)`** | Optional; only for **linear** VAST / direct URL when **not** using IMA for that path. Prefer here over static `VideoAdView.VideoPlaybackFactory` so config travels with the rest of the SDK. |
| 7 | **`AdSizeSettings(...)`** | Optional asset for default creative sizes. |

**Android:** if `UnityPlayer.currentActivity` is null on the first frame, call `Initialize` **after** the first `yield return null` (or from `Start` when the activity is ready). Symptoms of “too early”: **`Init (Android Java): skipped — Unity currentActivity is null`** in logcat.

**Example — production-style single builder (Direct SDK + custom video):**

```csharp
using BidscubeSDK;
using UnityEngine;

public sealed class BidscubeBootstrap : MonoBehaviour
{
    [SerializeField] private string adAuthority = "ssp-bcc-ads.com";

    private void Start()
    {
        var config = new SDKConfig.Builder()
            .IntegrationMode(BidscubeIntegrationMode.DirectSdk)
            .EnableLogging(true)
            .EnableDebugMode(false)
            .EnableTestMode(false)
            .DefaultAdTimeout(30_000)
            .DefaultAdPosition(AdPosition.Unknown)
            .AdRequestAuthority(adAuthority)
            // .VideoPlaybackFactory((hostGo, rawImage) => hostGo.AddComponent<MyAvProVideoPlayback>())
            .Build();

        BidscubeSDK.BidscubeSDK.Initialize(config);
    }
}
```

**Example — MAX mediation (no C# creatives, Android order):**

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .EnableLogging(true)
    .AdRequestAuthority("ssp-bcc-ads.com")
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
// Then: MaxSdk.InitializeSdk(...);
```

---

## Verifying the integration (logs)

Enable **`EnableLogging(true)`** (default on `SDKConfig.Builder`). Unity prefixes every SDK line with **`[BidscubeSDK]`** (see `Logger.cs`). Use **Editor Console** or **logcat** (`adb logcat -s Unity`) on device.

### After `BidscubeSDK.Initialize(config)`

| Log line (substring) | Meaning |
|----------------------|---------|
| **`Init (C#):`** | C# config saved: **`integrationMode=`**, **`unityPackage=`** (UPM), **`bundledNativeAndroid=`** (Maven pin for Gradle inject). |
| **`Init (Android Java): syncing…`** | Android player: Unity is attempting Java `BidscubeSDK.initialize` / `setActivity`. |
| **`Init (Android Java): SUCCESS`** | Native **`com.bidscube.sdk.BidscubeSDK.isInitialized()==true`** — MAX adapter can share this instance. |
| **`WARNING: Init (Android Java): … ClassNotFoundException`** | Java **`com.bidscube.sdk.BidscubeSDK`** not in APK — confirm default **`files('libs/bidscube-sdk-<ver>.aar')`** (exported **`unityLibrary/libs/`**) or **`MavenBidscubeSdkAar`** with **`…@aar`** (not POM-only). **Unity C# creatives can still work**; fix export / **`CoreDependencyMode`**. |
| **`Init (publisher row):`** | **One-line summary**: UPM version, **`androidCoreDefault=libs/bidscube-sdk-…`** (bundled; Maven alt documented), mode, **`csharp_BidscubeSDK_IsInitialized=true`**, and **`AndroidJava=…`** tail (`OK`, `JAR_MISSING`, `SKIPPED`, etc.). **Primary grep for publishers:** **`[BidscubeSDK] Init`**. |
| Native tags **`BidscubeSDK`**, **`BidscubeSDKImpl`** (no `[BidscubeSDK]` prefix) | Messages from the **Java** SDK; they complement but do not replace Unity logs. |

### When loading / playing video (`ShowVideoAd`, Direct SDK)

| Log line | Meaning |
|----------|---------|
| **`ShowVideoAd called`** / **`Video ad request URL:`** | Request built; check URL / placement. |
| **`[VideoAdView] Fetch/VAST path: using Unity surface only`** | VAST/JSON path without attaching IMA component for that fetch. |
| **`[VideoAdView] Linear surface playback: custom …`** | Your **`IVideoSurfacePlayback`** factory is active. |
| **`[VideoAdView] Linear surface playback: UnityEngine.Video.VideoPlayer`** | Default Unity **`VideoPlayer`** — add **`BIDSCUBE_DISABLE_UNITY_VIDEO`** + factory to strip **`UnityEngine.VideoModule`** from builds when you do not need it. |
| **`[VideoAdView] Successfully parsed VAST, video URL:`** | VAST OK; next step is prepare/play. |
| **`[VideoAdView] Preparing video player with URL:`** | Surface backend is buffering. |
| **`[VideoAdView] Video prepared successfully`** then **`Starting video playback...`** | Linear path healthy. |
| **`[VideoAdView] …`** **`InfoError`** | Failure (network, parse, timeout, missing factory when Unity Video disabled). |

**Callbacks** (`IAdCallback`): implement **`OnAdLoading`**, **`OnAdLoaded`**, **`OnAdDisplayed`**, **`OnAdFailed`**, **`OnAdClosed`** in your app code — they are the authoritative **business** outcome alongside logs.

---

## Video playback paths

**Direct SDK (Unity):** default = IMA (when used for that path) and/or **`UnityEngine.Video.VideoPlayer`** on `VideoAdView`; custom = **`IVideoSurfacePlayback`** via **`SDKConfig.Builder.VideoPlaybackFactory`** (call **`BindToRawImage`** on your instance inside the factory). **`BIDSCUBE_DISABLE_UNITY_VIDEO`** removes the Unity Video module — you must then supply a factory for the linear surface path.

**AppLovin MAX (Android native):** not `VideoAdView` — native **`com.bidscube.sdk`** playback; optional Java **`VideoPlayerProvider` / `videoPlayerProvider`** on the **first** successful native `initialize` per your AAR docs. This UPM does **not** forward that from C#.

Full matrix, Gradle/AAR context, and stubs: **`VIDEO_PLAYBACK.md`**, **`APPLOVIN_MAX.md`**, **`ANDROID_BUNDLED_SDK.md`**.

**MAX init (short):** `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`; **Android:** `BidscubeSDK.Initialize(config)` **before** `MaxSdk.InitializeSdk`; do not call C# creative APIs in MAX mode. `IntegrationModeFromWire("levelPlay")` still maps to MAX.

---

## Configuration

### SDK Configuration Options

The `SDKConfig` class allows you to configure various aspects of the SDK:

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `EnableLogging` | `bool` | `true` | Enable/disable SDK logging |
| `EnableDebugMode` | `bool` | `false` | Enable/disable debug mode |
| `EnableTestMode` | `bool` | `false` | Passed to native Android `SDKConfig.Builder` when supported |
| `IntegrationMode` | `BidscubeIntegrationMode` | `DirectSdk` | Direct SDK vs **AppLovin MAX** mediation |
| `DefaultAdTimeout` | `int` | `30000` | Default ad loading timeout in milliseconds |
| `DefaultAdPosition` | `AdPosition` | `Unknown` | Default ad position (centered) |
| `AdRequestAuthority` | `string` | `ssp-bcc-ads.com` | SSP host (optional port / IPv6). Normalized; see `Documentation~/AD_REQUEST_ENDPOINT.md`. |
| `BaseURL` | `string` (read-only) | `https://ssp-bcc-ads.com/sdk` | Derived: `https://<AdRequestAuthority>/sdk`. Builder alias: `BaseURL(string)` same normalization as `AdRequestAuthority(string)`. |
| `VideoPlaybackFactory` | `Func<GameObject, RawImage, IVideoSurfacePlayback>` | `null` | Optional **Direct SDK** linear surface; see **`VIDEO_PLAYBACK.md`**. Builder: `VideoPlaybackFactory(...)`. |

### Configuration Builder Pattern

```csharp
var config = new SDKConfig.Builder()
    .EnableLogging(true)
    .EnableDebugMode(false)
    .DefaultAdTimeout(30000)
    .DefaultAdPosition(AdPosition.Header)
    .AdRequestAuthority("your-custom-ssp-host.com")
    .Build();

BidscubeSDK.BidscubeSDK.Initialize(config);
```

---

## Showing Ads

The SDK supports three types of ads: **Image/Banner**, **Video**, and **Native**.

### Image/Banner Ads

Image ads are static banner advertisements that can be displayed at various positions on the screen.

#### Basic Image Ad

```csharp
using BidscubeSDK;

// Show image ad with default position (centered)
BidscubeSDK.BidscubeSDK.ShowImageAd("your-placement-id", new MyAdCallback());
```

#### Positioned Banner Ads

```csharp
// Show header banner (top of screen)
BidscubeSDK.BidscubeSDK.ShowHeaderBanner("your-placement-id", new MyAdCallback());

// Show footer banner (bottom of screen)
BidscubeSDK.BidscubeSDK.ShowFooterBanner("your-placement-id", new MyAdCallback());

// Show sidebar banner (right side)
BidscubeSDK.BidscubeSDK.ShowSidebarBanner("your-placement-id", new MyAdCallback());
```

#### Custom Position Banner

```csharp
// Show banner at custom position with specific dimensions
BidscubeSDK.BidscubeSDK.ShowCustomBanner(
    "your-placement-id",
    AdPosition.Header,  // Position
    320,                 // Width in pixels
    50,                  // Height in pixels
    new MyAdCallback()
);
```

### Video Ads

Video ads are full-screen video advertisements that play automatically.

#### Basic Video Ad

```csharp
// Show video ad
BidscubeSDK.BidscubeSDK.ShowVideoAd("your-placement-id", new MyAdCallback());
```

#### Skippable Video Ad

```csharp
// Show skippable video ad with custom skip button text
BidscubeSDK.BidscubeSDK.ShowSkippableVideoAd(
    "your-placement-id",
    "Skip Ad",           // Skip button text
    new MyAdCallback()
);
```

**Note:** Video ads are always displayed in full-screen mode regardless of position settings.

### Native Ads

Native ads are customizable advertisements that match your app's design.

```csharp
// Show native ad
BidscubeSDK.BidscubeSDK.ShowNativeAd("your-placement-id", new MyAdCallback());
```

---

## Ad Callbacks

Implement the `IAdCallback` interface to receive ad events, or extend the `AdCallback` base class for convenience.

### Using IAdCallback Interface

```csharp
using BidscubeSDK;

public class MyAdHandler : MonoBehaviour, IAdCallback
{
    public void OnAdLoading(string placementId)
    {
        Debug.Log($"Ad is loading: {placementId}");
    }

    public void OnAdLoaded(string placementId)
    {
        Debug.Log($"Ad loaded: {placementId}");
    }

    public void OnAdDisplayed(string placementId)
    {
        Debug.Log($"Ad displayed: {placementId}");
    }

    public void OnAdClicked(string placementId)
    {
        Debug.Log($"Ad clicked: {placementId}");
    }

    public void OnAdClosed(string placementId)
    {
        Debug.Log($"Ad closed: {placementId}");
    }

    public void OnAdFailed(string placementId, int errorCode, string errorMessage)
    {
        Debug.LogError($"Ad failed: {placementId}, Error: {errorCode} - {errorMessage}");
    }

    // Video-specific callbacks
    public void OnVideoAdStarted(string placementId)
    {
        Debug.Log($"Video ad started: {placementId}");
    }

    public void OnVideoAdCompleted(string placementId)
    {
        Debug.Log($"Video ad completed: {placementId}");
    }

    public void OnVideoAdSkipped(string placementId)
    {
        Debug.Log($"Video ad skipped: {placementId}");
    }

    public void OnVideoAdSkippable(string placementId)
    {
        Debug.Log($"Video ad is now skippable: {placementId}");
    }

    public void OnInstallButtonClicked(string placementId, string buttonText)
    {
        Debug.Log($"Install button clicked: {placementId}, Text: {buttonText}");
    }
}
```

### Using AdCallback Base Class

```csharp
using BidscubeSDK;

public class MyAdHandler : AdCallback
{
    public override void OnAdLoaded(string placementId)
    {
        Debug.Log($"Ad loaded: {placementId}");
        // Only override methods you need
    }

    public override void OnAdFailed(string placementId, int errorCode, string errorMessage)
    {
        Debug.LogError($"Ad failed: {errorMessage}");
    }
}
```

### Using Callbacks

```csharp
// Create callback instance
var callback = new MyAdHandler();

// Show ad with callback
BidscubeSDK.BidscubeSDK.ShowImageAd("your-placement-id", callback);
```

---

## Ad Positioning

Ads can be positioned at various locations on the screen. The SDK supports both manual positioning and server-determined positioning.

### Ad Position Enum

```csharp
public enum AdPosition
{
    Unknown = 0,           // Centered (default)
    AboveTheFold = 1,      // Above the fold
    BelowTheFold = 3,      // Below the fold
    Header = 4,            // Top of screen
    Footer = 5,            // Bottom of screen
    Sidebar = 6,           // Right side
    FullScreen = 7         // Full screen (video ads only)
}
```

### Manual Position Override

You can manually set the ad position, which will override the server response:

```csharp
// Set manual position
BidscubeSDK.BidscubeSDK.SetAdPosition(AdPosition.Header);

// Show ad (will use Header position)
BidscubeSDK.BidscubeSDK.ShowImageAd("your-placement-id", callback);
```

### Get Current Position

```csharp
// Get manual position
AdPosition manualPosition = BidscubeSDK.BidscubeSDK.GetAdPosition();

// Get server response position
AdPosition serverPosition = BidscubeSDK.BidscubeSDK.GetResponseAdPosition();

// Get effective position (manual override takes priority)
AdPosition effectivePosition = BidscubeSDK.BidscubeSDK.GetEffectiveAdPosition();
```

### Position Priority

The SDK uses the following priority order for ad positioning:

1. **Manual Position** (if set via `SetAdPosition()`) - Highest priority
2. **Server Response Position** (from ad response) - Medium priority
3. **Default Position** (Unknown/Centered) - Lowest priority

---

## Consent Management

The SDK includes built-in consent management for GDPR and CCPA compliance.

### Request Consent Info

```csharp
using BidscubeSDK;

public class ConsentManager : MonoBehaviour, IConsentCallback
{
    void Start()
    {
        // Request consent info update
        BidscubeSDK.BidscubeSDK.RequestConsentInfoUpdate(this);
    }

    public void OnConsentInfoUpdated()
    {
        Debug.Log("Consent info updated");
        // Check if consent form is required
        // Show consent form if needed
    }

    public void OnConsentInfoUpdateFailed(Exception error)
    {
        Debug.LogError($"Consent info update failed: {error.Message}");
    }

    public void OnConsentFormShown()
    {
        Debug.Log("Consent form shown");
    }

    public void OnConsentFormError(Exception error)
    {
        Debug.LogError($"Consent form error: {error.Message}");
    }

    public void OnConsentGranted()
    {
        Debug.Log("Consent granted");
        // User granted consent, can now show ads
    }

    public void OnConsentDenied()
    {
        Debug.Log("Consent denied");
        // User denied consent, handle accordingly
    }

    public void OnConsentNotRequired()
    {
        Debug.Log("Consent not required");
        // Consent not required for this user
    }

    public void OnConsentStatusChanged(bool hasConsent)
    {
        Debug.Log($"Consent status changed: {hasConsent}");
    }
}
```

### Show Consent Form

```csharp
// Show consent form
BidscubeSDK.BidscubeSDK.ShowConsentForm(new MyConsentCallback());
```

---

## Examples

### Complete Integration Example

```csharp
using BidscubeSDK;
using UnityEngine;

public class AdManager : MonoBehaviour, IAdCallback
{
    [Header("Ad Configuration")]
    public string placementId = "your-placement-id";
    public AdPosition adPosition = AdPosition.Header;

    void Start()
    {
        // Initialize SDK
        var config = new SDKConfig.Builder()
            .EnableLogging(true)
            .DefaultAdTimeout(30000)
            .Build();
        
        BidscubeSDK.BidscubeSDK.Initialize(config);

        // Set ad position
        BidscubeSDK.BidscubeSDK.SetAdPosition(adPosition);

        // Show ad after a delay
        Invoke(nameof(ShowAd), 2f);
    }

    void ShowAd()
    {
        BidscubeSDK.BidscubeSDK.ShowImageAd(placementId, this);
    }

    // IAdCallback implementation
    public void OnAdLoading(string placementId)
    {
        Debug.Log("Loading ad...");
    }

    public void OnAdLoaded(string placementId)
    {
        Debug.Log("Ad loaded successfully");
    }

    public void OnAdDisplayed(string placementId)
    {
        Debug.Log("Ad displayed");
    }

    public void OnAdClicked(string placementId)
    {
        Debug.Log("Ad clicked");
    }

    public void OnAdClosed(string placementId)
    {
        Debug.Log("Ad closed");
    }

    public void OnAdFailed(string placementId, int errorCode, string errorMessage)
    {
        Debug.LogError($"Ad failed: {errorMessage}");
    }

    // Video callbacks (optional)
    public void OnVideoAdStarted(string placementId) { }
    public void OnVideoAdCompleted(string placementId) { }
    public void OnVideoAdSkipped(string placementId) { }
    public void OnVideoAdSkippable(string placementId) { }
    public void OnInstallButtonClicked(string placementId, string buttonText) { }
}
```

### Show Different Ad Types

```csharp
using BidscubeSDK;

public class AdExamples : MonoBehaviour
{
    public string imagePlacementId = "image-placement-id";
    public string videoPlacementId = "video-placement-id";
    public string nativePlacementId = "native-placement-id";

    void Start()
    {
        BidscubeSDK.BidscubeSDK.Initialize();
    }

    public void ShowImageAd()
    {
        BidscubeSDK.BidscubeSDK.ShowImageAd(imagePlacementId, new MyAdCallback());
    }

    public void ShowVideoAd()
    {
        BidscubeSDK.BidscubeSDK.ShowVideoAd(videoPlacementId, new MyAdCallback());
    }

    public void ShowNativeAd()
    {
        BidscubeSDK.BidscubeSDK.ShowNativeAd(nativePlacementId, new MyAdCallback());
    }
}
```

### Cleanup

When your app closes or you want to clean up SDK resources:

```csharp
void OnDestroy()
{
    BidscubeSDK.BidscubeSDK.Cleanup();
}
```

---

## SDK Test Scene

The SDK includes a comprehensive test scene (`SDKTestScene`) that demonstrates all SDK features and provides a working example of integration.

### Accessing the Test Scene

1. Open the package sample from `Samples/SDK Demo`
2. Open `SDK Test Scene.unity`
3. The scene contains:
   - UI buttons to test different ad types
   - Position selection dropdown
   - Manual position toggle
   - Log output display
   - All callback implementations

### Using the Test Scene

1. Open the test scene in Unity Editor
2. Configure your placement IDs in the `SDKTestScene` component
3. Press Play
4. Initialize SDK with Initialize SDK (Button)
 Then Use the UI buttons to test:
   - Image/Banner ads
   - Video ads
   - Native ads
   - Manual position override
   - Clean Up SDK to destroy adObjects

The test scene serves as both a testing tool and a reference implementation for integrating the SDK into your project.

---

## Additional Resources

- **Video playback (Direct vs MAX):** `Documentation~/VIDEO_PLAYBACK.md`
- **GitHub:** `https://github.com/BidsCube/AppLovin-SDK-Unity`
- **Releases / issues:** use the same repository on GitHub.

---

## Support

For support, please open an issue on the GitHub repository or contact the Bidscube support team.

---

## License

See `LICENSE.md` in the repository for license information.

