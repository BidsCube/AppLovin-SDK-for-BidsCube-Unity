# AppLovin MAX mediation (Unity)

**Short checklist (start here):** [`APPLOVIN_MEDIATION_STEPS.md`](APPLOVIN_MEDIATION_STEPS.md)

> **`com.bidscube.applovin.max` 1.0.16+** ships **`BidscubeAndroidGradlePostprocessor`** (Android Build Support) for **LiteNoVideo** (default) / **FullWithVideo**; iOS Podfile automation may come from **`com.bidscube.sdk`** or manual **Podfile** edits — see **`ANDROID_BUNDLED_SDK.md`**. C# **`BidscubeSDK`** and **`SDKConfig`** remain in **`com.bidscube.sdk`**.

| Component | Version |
|-----------|--------:|
| This UPM (`com.bidscube.applovin.max`) | **1.0.16** |
| Core UPM peer (`com.bidscube.sdk`, see `package.json` → `dependencies`) | **1.2.6** |
| Android MAX adapter AAR + iOS `BidscubeSDKAppLovin` | **1.0.4** |
| Android core native (`bidscube-sdk` / lite AAR) | **1.2.3** |
| AppLovin MAX Android (`com.applovin:applovin-sdk` / AppLovin iOS pod) | **13.x** |

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
| **Video / rewarded** | `showVideoAd` | **IMA** + fullscreen container (requires **full** core + IMA/Media3 on Gradle — **`BidscubeAndroidExportSettings.featureSet = FullWithVideo`** or **Tools → Bidscube SDK → Android Build Features**; see **`ANDROID_BUNDLED_SDK.md`**). |
| **Native** | `getNativeAdView` → payload | Adapter builds **MaxNativeAd** (assets); **do not** pass the Bidscube SDK view as the MAX creative. |

### Video playback: Direct vs MAX

Canonical guide: **`VIDEO_PLAYBACK.md`** (default vs custom, Unity vs Java).

| Path | Custom player |
|------|----------------|
| **Direct SDK** (Unity C# creatives) | **`SDKConfig.Builder.VideoPlaybackFactory`** + **`IVideoSurfacePlayback`** (Unity `VideoAdView` linear path). |
| **AppLovin MAX / native** | Native **`com.bidscube.sdk`** (IMA + fullscreen video surface). Optional **Java** `VideoPlayerProvider` / `videoPlayerProvider` on **`SDKConfig.Builder`** on the **first** successful native `BidscubeSDK.initialize` — **not** forwarded from C# in this UPM; confirm API names on your pinned AAR. |

There is **no** Unity C# hook in this package that injects a native MAX VAST player. A future interop could expose **`BidscubeAndroidSdkInterop`** forwarding if the published Android SDK stabilizes it.

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

### SDK Demo sample (`BidscubeExampleScene`)

After you import the **SDK Demo** sample and the official **AppLovin MAX Unity plugin** (`MaxSdk` / `MaxSdk.Scripts` assembly), the example scene shows:

- A top bar to switch **Direct Unity SDK** vs **AppLovin MAX (adapter)** (stored in `PlayerPrefs` when you use the default key `bidscube_integration_mode` and enable **Load Integration Mode From Player Prefs**, or you can set the mode in the Inspector).
- In **AppLovin MAX** integration mode: **Init SDK** runs `BidscubeSDK.Initialize` with mediation config first, then `MaxSdk.SetSdkKey` (optional) and `MaxSdk.InitializeSdk()`, then a **MAX toolbar** (load/show interstitial and rewarded, banner toggle, mediation debugger). Assign your MAX **ad unit IDs** and optional **AppLovin SDK key** on the `BidscubeExampleScene` component. This mirrors the native flow: MAX loads the network stack; the Bidscube **custom adapter** serves creatives.

1. Add this package and the **AppLovin MAX Unity plugin** (or integrate MAX natively per AppLovin docs).
2. **Android:** this package bundles **`applovin-bidscube-max-adapter-1.0.4.aar`** and, by default (**`LiteNoVideo`**), copies **`bidscube-sdk-lite-1.2.3.aar`** into **`unityLibrary/libs/`** (no Media3/IMA). For **rewarded/video** through the native Bidscube stack use **`FullWithVideo`** + full core AAR or Maven — see **`ANDROID_BUNDLED_SDK.md`**. Do **not** add a **second** core `implementation` for Bidscube. **minSdk** often **26**+ for the bundled AARs. **Keep Enable Desugaring on** in **`BidscubeAndroidExportSettings`** — bundled core AAR metadata usually requires **`coreLibraryDesugaring`** on **`:launcher`** (see **`APPLOVIN_MEDIATION_STEPS.md`**). The adapter AAR includes **ProGuard** consumer rules for `BidscubeMediationAdapter`.
3. **iOS:** use CocoaPods **`BidscubeSDKAppLovin` `1.0.4`** (BidCube runtime + **`ALBidscubeMediationAdapter`**) and **`AppLovinSDK`** **13.x** (see snippet below). If another package (e.g. **`com.bidscube.sdk`**) provides **`BidscubeIosPodfilePostprocessor`**, it may append these lines on export; otherwise add them **manually**. **Do not** add a separate **`BidscubeSDK`** pod for the same target if you already use **`BidscubeSDKAppLovin`**. **Google IMA** remains required by the native stack where applicable. Official reference: [AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS).
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

Then **`pod install`** and open the **`.xcworkspace`**. A **`BidscubeIosPodfilePostprocessor`**, if present in your project (e.g. from **`com.bidscube.sdk`**), may append the same pods **only when** they are not already declared (and may skip **`BidscubeSDKAppLovin`** if a standalone **`BidscubeSDK`** pod is present).

### Supported ad formats (MAX)

**Banner**, **MREC**, **interstitial**, **rewarded**, **native** — use your usual MAX APIs (**`MAInterstitialAd`**, **`MARewardedAd`**, **`MAAdView`**, **`MANativeAdLoader`**, etc.).

**Native:** if your MAX setup uses a native-specific local parameter, set **`is_native = true`** where applicable (see iOS adapter / dashboard notes).

## What not to do in MAX mode

Do **not** call C# APIs that attach Bidscube creatives to the Unity scene (`GetBannerAdView`, `GetNativeAdView`, `GetVideoAdView`, `ShowImageAd`, `ShowVideoAd`, `ShowNativeAd`, etc.). They exist for **direct SDK** mode only.

## Troubleshooting (Android)

- **Adapter not found / ClassNotFoundException** — Dashboard class name must be exactly `com.applovin.mediation.adapters.BidscubeMediationAdapter`. Confirm **`applovin-bidscube-max-adapter-1.0.4.aar`** is enabled for Android in the Unity Inspector (no “Any Platform” disable). With **R8 / minify**, the AAR’s consumer rules should keep the adapter; if you use a custom ProGuard file, add the same `-keep` lines as in the AAR’s `proguard.txt`.
- **Bidscube stuck “not initialized” in MAX** — Call **`BidscubeSDK.Initialize`** (MAX mode + your SSP authority) **before** MAX SDK init. If you only use adapter-side init, set a non-empty **App ID** (placement ID) on the custom network in MAX and/or **`request_authority` / `ssp_host`** server parameters.
- **Duplicate `com.applovin:applovin-sdk`** — The **AppLovin MAX Unity plugin** and/or your **Custom Main Gradle Template** / **`com.bidscube.sdk`** automation may each add an `implementation`. Gradle should resolve a **single** version; if you see duplicate-class errors, align to **one** `com.applovin:applovin-sdk` line (typically **13.x** with the MAX plugin) or remove the duplicate.

## Troubleshooting (iOS)

- **Ads do not load** — Confirm **App ID** in MAX holds the correct **BidCube placement ID** (not the App Store id).
- **SSP override** — Use only **host** or **`host:port`** in **`request_authority`** / **`ssp_host`** server parameters.
- **Custom network not found** — Class name must be exactly **`ALBidscubeMediationAdapter`**.
- **Pods / duplicate symbols** — Do not add **`BidscubeSDK`** and **`BidscubeSDKAppLovin`** for the same target; prefer **`BidscubeSDKAppLovin`** for MAX. Run **`pod install`** after Unity export; resolve any version conflicts with **`AppLovinSDK`** **13.x**.

## References

- AppLovin: [Integrating custom SDK networks](https://support.axon.ai/en/max/mediated-network-guides/integrating-custom-sdk-networks/)
- Bidscube iOS (runtime + MAX adapter): [github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS)
- Flutter plugin README (Android mediation overlap): sibling repo `AppLovin-SDK-Flutter`
