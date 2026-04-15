# Bidscube Unity SDK Integration Guide

If you use **AppLovin MAX** mediation, read `Documentation~/APPLOVIN_MAX.md` first. This guide focuses on the **direct** Unity/C# SDK (creatives from C#).

This guide will help you integrate the Bidscube Unity SDK into your Unity project and start showing ads.

## Table of Contents

1. [Installation](#installation)
2. [Initialization](#initialization)
3. [Configuration](#configuration)
4. [Showing Ads](#showing-ads)
5. [Ad Callbacks](#ad-callbacks)
6. [Ad Positioning](#ad-positioning)
7. [Consent Management](#consent-management)
8. [Examples](#examples)
9. [SDK Test Scene](#sdk-test-scene)

---

## Installation

The Bidscube Unity SDK can be installed in two ways:

### Unity Package Manager

1. In Unity Editor, open the Package Manager (`Window` → `Package Manager`)
2. Click the `+` button in the top-left corner
3. Select `Add package from git URL...`
4. Enter the repository URL: `https://github.com/BidsCube/AppLovin-SDK-Unity.git`  
   The public **Bidscube Unity SDK** sample line is [github.com/BidsCube/bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) (same `com.bidscube.sdk` name; **AppLovin-SDK-Unity** adds bundled Android SDK + MAX adapter AARs, Gradle AppLovin 13+, and optional Podfile hook — see `README.md`).
5. Optionally, specify a version tag: `https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.3.1`
6. Click `Add`
7. The SDK will be added as a package dependency

**Note:** For Git import, you may need to add the package to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.3.1"
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

### AppLovin MAX mediation (init)

Use `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)`. **Android:** call `Initialize` **before** the AppLovin MAX SDK so native `AdRequestAuthority` and options match C#. **iOS:** C# `Initialize` is **optional** if you use the **`BidscubeSDKAppLovin`** adapter-only path; see `Documentation~/APPLOVIN_MAX.md` and [AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS). Do **not** use C# APIs that attach creatives (`GetBannerAdView`, `ShowVideoAd`, etc.) in MAX mode; they throw. Full flow: `Documentation~/APPLOVIN_MAX.md`.

For JSON / `PlayerPrefs`, `IntegrationModeFromWire("levelPlay")` still maps to MAX mediation (backward compatible).

**Recommended wiring (one place):** build a single `SDKConfig` with `IntegrationMode`, `AdRequestAuthority` / test flags, and (if needed) `VideoPlaybackFactory`, then call `BidscubeSDK.Initialize(config)` once at startup. On **Android + MAX**, call it **before** `MaxSdk.InitializeSdk`. Avoid a **second** `implementation 'com.bidscube:bidscube-sdk:…'` in Custom Gradle — the post-processor already injects one Maven coordinate; duplicates break DEX / init. **Logcat / Unity:** filter for **`[BidscubeSDK] Init (C#):`** then **`[BidscubeSDK] Init (Android Java): SUCCESS`** on device.

### Android: bundled native SDK (self-contained)

- Adding **`com.bidscube.sdk`** from Git (e.g. `…/AppLovin-SDK-Unity.git#v1.0.3.1`) is enough for Android: the package ships **`applovin-bidscube-max-adapter-*.aar`** and injects **`com.bidscube:bidscube-sdk:<version>`** from Maven Central (see `Constants.NativeAndroidBidscubeSdkVersion`, currently **1.2.2**) via **`BidscubeAndroidGradlePostprocessor`**. **Do not** add another `implementation 'com.bidscube:bidscube-sdk:…'` in **Custom Base Gradle** / **mainTemplate** — duplicates classes. Remove any legacy **`project(':bidscube-sdk-…')`** lines that vendor a second core SDK.
- On Gradle export, **`BidscubeAndroidGradlePostprocessor`** injects AppLovin 13.x, Media3, IMA, UMP, Glide, Material, desugar libs, **and** the core Bidscube coordinate into **`unityLibrary`**, enables **core library desugaring** there, and mirrors desugaring onto **`launcher`** when needed. Install Unity **Android Build Support** so the Editor script compiles. First Gradle resolve needs Maven Central access for **`com.bidscube:bidscube-sdk`**. Desugaring follows AAR / AndroidX metadata requirements.
- **Optional build size (Unity `VideoPlayer`):** for **AppLovin MAX–only** games you never call `ShowVideoAd` / `GetVideoAdView`, Unity may still include the **Video** module if the default `VideoAdView` path references it. To drop **`UnityEngine.VideoModule`**, add scripting define **`BIDSCUBE_DISABLE_UNITY_VIDEO`**. For **direct SDK** VAST without IMA, register a factory on **`SDKConfig.Builder().VideoPlaybackFactory(...)`** before `Initialize` (recommended — one place with SSP / mode / logging), or set **`VideoAdView.VideoPlaybackFactory`** as a fallback. Factory returns **`IVideoSurfacePlayback`** (e.g. AVPro). With **IMA only**, no custom factory is needed.
- Set player **Minimum API Level** to **24+** (matches the Android SDK).
- **AppLovin MAX**: add the MAX Unity plugin. **Android:** Bidscube MAX adapter AAR is bundled; Gradle injects **AppLovin SDK 13.0+**. **iOS:** CocoaPods **`BidscubeSDKAppLovin`** `1.0.3` and **`AppLovinSDK`** **13.x** (or rely on **`BidscubeIosPodfilePostprocessor`** on Podfile export) — `Documentation~/APPLOVIN_MAX.md`.

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
| `VideoPlaybackFactory` | `Func<GameObject, RawImage, IVideoSurfacePlayback>` | `null` | Optional custom linear video for VAST without IMA; see Android bundled SDK section. Builder: `VideoPlaybackFactory(...)`. |

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

- **GitHub:** `https://github.com/BidsCube/AppLovin-SDK-Unity`
- **Releases / issues:** use the same repository on GitHub.

---

## Support

For support, please open an issue on the GitHub repository or contact the Bidscube support team.

---

## License

See `LICENSE.md` in the repository for license information.

