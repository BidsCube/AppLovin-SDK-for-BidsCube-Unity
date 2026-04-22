# AppLovin MAX mediation (Unity)

Bidscube as a **custom SDK network** in **AppLovin MAX**: the app is built in Unity; **load and show** run through MAX. **Android:** call **`BidscubeSDK.Initialize`** early (MAX mediation mode) so the Java SDK shares **`AdRequestAuthority`**, test mode, and logging with the bundled adapter — same idea as the Flutter plugin. **iOS:** the native **`BidscubeSDKAppLovin`** pod’s adapter can initialize the BidCube runtime internally ([iOS distribution](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS)); optional C# **`BidscubeSDK.Initialize`** when you want Unity-driven **`SDKConfig`** before MAX runs.

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

### Video playback: Direct vs MAX (this UPM version)

| Path | Custom player |
|------|----------------|
| **Direct SDK** (Unity C# creatives) | Supported — **`SDKConfig.Builder.VideoPlaybackFactory`** / **`IVideoSurfacePlayback`** (see **`INTEGRATION.md`**). |
| **AppLovin MAX / native** (`showVideoAd`, MAX adapter) | **Not** wired from this Unity package. Playback follows the **published** native **`com.bidscube:bidscube-sdk`** behaviour (default native / IMA stack for that SDK version). |

There is **no** Unity C# API here that injects a host native VAST player into the MAX mediation pipeline. If a future **published** Android SDK adds a stable, documented hook for that, the Unity package can forward it again via **`BidscubeAndroidSdkInterop`**.

## MAX dashboard (custom SDK network)

In **MAX → Mediation → Manage → Networks →** *add Custom Network* ([AppLovin: integrating custom SDK networks](https://support.axon.ai/en/max/mediated-network-guides/integrating-custom-sdk-networks/)):

**Android / Fire OS**

| Field | Value |
|--------|--------|
| **Network type** | SDK |
| **Adapter class name** | `com.applovin.mediation.adapters.BidscubeMediationAdapter` |

Do **not** rename the Java class or package — MAX loads it by reflection.

**iOS**

| Field | Value |
|--------|--------|
| **Network type** | SDK |
| **iOS adapter class name** | `ALBidscubeMediationAdapter` (exact spelling) |

**MAX parameters (both platforms)**

| Field | Value |
|--------|--------|
| **App ID** | **BidCube placement ID** — MAX still labels this “App ID”; for this network it must be the placement ID. |
| **Placement ID** | Optional; leave empty unless your MAX setup needs a second value. |
| **Server parameters** (optional) | **`request_authority`** or **`ssp_host`** — SSP host or `host:port` (normalized the same way as standalone **`AdRequestAuthority`** / **`SDKConfig.Builder.AdRequestAuthority`**). When set, the adapter uses it as the ad request authority. |

On each ad unit, enable **Bidscube** under **Custom Networks & Deals** and set fields as above.

**Android:** if you **pre-initialize from Unity** with the full **`SDKConfig`**, **App ID** in MAX is still recommended; the adapter can report success without re-init when Java **`BidscubeSDK`** is already initialized. **iOS:** you may rely on adapter-only init and **App ID** (placement) alone per the [iOS SDK README](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS); use server params for SSP override when not using C# **`AdRequestAuthority`**.

---

## Unity project setup

1. Add this package and the **AppLovin MAX Unity plugin** (or integrate MAX natively per AppLovin docs).
2. **Android:** the package bundles **`applovin-bidscube-max-adapter-1.0.4.aar`** for MAX and ships a **reference** **`bidscube-sdk-1.2.3.aar`** (Android import off — see **`ANDROID_BUNDLED_SDK.md`**); **`BidscubeAndroidGradlePostprocessor`** injects the core SDK (default **`com.bidscube:bidscube-sdk:<version>@aar`** from **`Constants.NativeAndroidBidscubeSdkVersion`**, resolved via your Gradle repos — or **`CustomGradleLines`** / **`SkipInjectionIntegratorOwnsCore`** per **`ANDROID_BUNDLED_SDK.md`**) and **`com.applovin:applovin-sdk:13.+`** plus other Maven deps — no separate adapter distribution. Do **not** add a **second** core `implementation` for Bidscube in Custom Gradle. See **`ANDROID_BUNDLED_SDK.md`** (Gradle export raises **minSdk** to **26** for AAR metadata; AppLovin allows **23+**). The adapter AAR ships **consumer ProGuard rules** so R8 keeps `BidscubeMediationAdapter` when minification is on.
3. **iOS:** use CocoaPods **`BidscubeSDKAppLovin`** (BidCube runtime + **`ALBidscubeMediationAdapter`**) and **`AppLovinSDK`** **13.x**. On Unity iOS exports that generate a **Podfile**, **`BidscubeIosPodfilePostprocessor`** appends missing lines (see below). **Do not** add a separate **`BidscubeSDK`** pod for the same target if you already use **`BidscubeSDKAppLovin`**. **Google IMA** remains required by the native stack where applicable. Official reference: [AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS).
4. **Optional C# startup** (recommended on **Android**; **optional** on **iOS** if the adapter initializes native BidCube and you set **`request_authority` / `ssp_host`** in MAX when needed):

```csharp
var config = new SDKConfig.Builder()
    .IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)
    .AdRequestAuthority("your-ssp-host.example.com")   // Android: native SDKConfig; iOS: when C# init is used
    .EnableTestMode(false)                  // optional; forwarded when native Builder exposes setters
    .Build();
BidscubeSDK.BidscubeSDK.Initialize(config);
```

**Startup order:** on **Android**, call **`BidscubeSDK.Initialize(config)`** **before** **`MaxSdk.InitializeSdk(...)`** so Java **`com.bidscube.sdk.BidscubeSDK`** matches **`AdRequestAuthority`**, test mode, and logging. On **iOS**, align with your chosen path: either rely on the adapter (MAX init only) or call C# **`Initialize`** before MAX if you need Unity-driven config. Then load/show with **MAX ad units** only.

### iOS CocoaPods (manual or exported Podfile)

Match the native iOS distribution:

```ruby
platform :ios, '13.0'
use_frameworks!

target 'YourApp' do
  pod 'AppLovinSDK', '>= 13.0.0', '< 14.0'
  pod 'BidscubeSDKAppLovin', '1.0.4'
end
```

Then **`pod install`** and open the **`.xcworkspace`**. If **`BidscubeIosPodfilePostprocessor`** runs, it appends the same pods **only when** they are not already declared (and skips adding **`BidscubeSDKAppLovin`** if a standalone **`BidscubeSDK`** pod is present).

### Supported ad formats (MAX)

**Banner**, **MREC**, **interstitial**, **rewarded**, **native** — use your usual MAX APIs (**`MAInterstitialAd`**, **`MARewardedAd`**, **`MAAdView`**, **`MANativeAdLoader`**, etc.).

**Native:** if your MAX setup uses a native-specific local parameter, set **`is_native = true`** where applicable (see iOS adapter / dashboard notes).

## What not to do in MAX mode

Do **not** call C# APIs that attach Bidscube creatives to the Unity scene (`GetBannerAdView`, `GetNativeAdView`, `GetVideoAdView`, `ShowImageAd`, `ShowVideoAd`, `ShowNativeAd`, etc.). They exist for **direct SDK** mode only.

## Troubleshooting (Android)

- **Adapter not found / ClassNotFoundException** — Dashboard class name must be exactly `com.applovin.mediation.adapters.BidscubeMediationAdapter`. Confirm **`applovin-bidscube-max-adapter-1.0.4.aar`** is enabled for Android in the Unity Inspector (no “Any Platform” disable). With **R8 / minify**, the AAR’s consumer rules should keep the adapter; if you use a custom ProGuard file, add the same `-keep` lines as in the AAR’s `proguard.txt`.
- **Bidscube stuck “not initialized” in MAX** — Call **`BidscubeSDK.Initialize`** (MAX mode + your SSP authority) **before** MAX SDK init. If you only use adapter-side init, set a non-empty **App ID** (placement ID) on the custom network in MAX and/or **`request_authority` / `ssp_host`** server parameters.
- **Duplicate `com.applovin:applovin-sdk`** — This package injects **`13.+`**; the AppLovin MAX Unity plugin also adds the SDK. Gradle should resolve a single version; if you see duplicate-class errors, align versions in **Custom Main Gradle Template** or temporarily comment one `implementation` line and rebuild.

## Troubleshooting (iOS)

- **Ads do not load** — Confirm **App ID** in MAX holds the correct **BidCube placement ID** (not the App Store id).
- **SSP override** — Use only **host** or **`host:port`** in **`request_authority`** / **`ssp_host`** server parameters.
- **Custom network not found** — Class name must be exactly **`ALBidscubeMediationAdapter`**.
- **Pods / duplicate symbols** — Do not add **`BidscubeSDK`** and **`BidscubeSDKAppLovin`** for the same target; prefer **`BidscubeSDKAppLovin`** for MAX. Run **`pod install`** after Unity export; resolve any version conflicts with **`AppLovinSDK`** **13.x**.

## References

- AppLovin: [Integrating custom SDK networks](https://support.axon.ai/en/max/mediated-network-guides/integrating-custom-sdk-networks/)
- Bidscube iOS (runtime + MAX adapter): [github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS)
- Flutter plugin README (Android mediation overlap): sibling repo `AppLovin-SDK-Flutter`
