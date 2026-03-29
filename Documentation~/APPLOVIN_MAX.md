# AppLovin MAX mediation (Unity)

Bidscube as a **custom SDK network** in **AppLovin MAX**: the app is built in Unity; **load and show** run through MAX. From Unity you perform **early initialization** of the native Bidscube stack so the **Bidscube MAX adapter** uses the **same SDK instance** as your game (same pattern as the Flutter plugin).

## Integration modes (C#)

| Mode | `SDKConfig.Builder` | Ads from Unity C# |
|------|---------------------|-------------------|
| **Direct / SDK** | `IntegrationMode(BidscubeIntegrationMode.DirectSdk)` (default) | Yes — `GetBannerAdView`, `ShowVideoAd`, etc. |
| **AppLovin MAX** | `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` | **No** — those APIs throw; use MAX only |

Legacy serialized values `levelPlay` / `level_play` still deserialize to **AppLovin MAX mediation** (parity with Flutter).

Wire strings for JSON / `PlayerPrefs`: `direct`, `directSdk`, `appLovinMax` (see `BidscubeIntegrationModeWire.FromWire`).

## Mediation flow (native adapter)

Single documented path — native **Bidscube SDK** APIs invoked by the **AppLovin MAX** custom adapter (not Unity `UnitySendMessage`):

| Format | Native SDK | Inside MAX |
|--------|------------|------------|
| **Banner** | `getImageAdView` → `View` | Passed into MAX; **AdDisplayManager** / **BannerViewFactory** host **WebView** with rendered ADM. |
| **Interstitial** | `showImageAd` | Full-screen image overlay from rendered ADM. |
| **Video / rewarded** | `showVideoAd` | **IMA** + fullscreen container. |
| **Native** | `getNativeAdView` → payload | Adapter builds **MaxNativeAd** (assets); **do not** pass the Bidscube SDK view as the MAX creative. |

## Unity project setup

1. Add this package and the **AppLovin MAX Unity plugin** (or integrate MAX natively per AppLovin docs).
2. Add the **Bidscube AppLovin MAX adapter** for Android and iOS from your adapter distribution (Maven / CocoaPods / local), matching the **native Bidscube SDK** versions expected by that adapter.
3. **Android**: bundled `bidscube-sdk-1.0.0.aar` + `BidscubeAndroidGradlePostprocessor` (see `ANDROID_BUNDLED_SDK.md`). **minSdk 24+**.
4. **iOS**: add **`BidscubeSDKAppLovin`** (or the pod name your adapter docs specify), **AppLovinSDK**, and **Google IMA** as required by the adapter README — align **major/minor** with the Flutter plugin podspec / Android `com.bidscube:bidscube-sdk` when you standardize releases.
5. At startup, call:

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .AdRequestAuthority("your-ssp-host.example.com")   // forwarded to native Android SDKConfig.Builder.adRequestAuthority when supported
    .EnableTestMode(false)                  // optional; forwarded when native Builder exposes setters
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
```

Then initialize MAX and load/show with **MAX ad units** only.

## What not to do in MAX mode

Do **not** call C# APIs that attach Bidscube creatives to the Unity scene (`GetBannerAdView`, `GetNativeAdView`, `GetVideoAdView`, `ShowImageAd`, `ShowVideoAd`, `ShowNativeAd`, etc.). They exist for **direct SDK** mode only.

## References

- AppLovin: [Integrating custom SDK networks](https://support.axon.ai/en/max/mediated-network-guides/integrating-custom-sdk-networks/)
- Flutter plugin README (same mediation rules): sibling repo `AppLovin-SDK-Flutter`
