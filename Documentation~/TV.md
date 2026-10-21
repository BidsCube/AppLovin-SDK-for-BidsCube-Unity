# Android TV and Apple TV

This package (`com.bidscube.applovin.max`) wires TV behavior. Interstitial and rewarded video are the supported TV formats.

## Android TV

Bundled AARs **1.2.18** follow the direct BidsCube Android SDK:

- Leanback, television UI mode, Fire TV, and Amazon `AFT*` count as TV.
- `AUTO` opens video fullscreen on TV. On a phone, full-screen and unknown positions are fullscreen; header, footer, and sidebar stay windowed.
- Native `BidscubeSDK.setVideoFullscreenPolicy(FORCE_WINDOWED)` stays windowed on TV (QA).
- D-pad focus is on Skip when it is armed, on Close after the video, and on **Keep watching** in the rewarded leave dialog. The video surface does not take focus.
- Rewarded video has no Skip. Back before the end asks to leave and does not grant a reward.

```csharp
BidscubeSDK.Initialize(new SDKConfig.Builder().EnableLogging(true).Build());
MaxSdk.InitializeSdk();
```

The game still needs Player Settings **Android TV Compatibility**, a TV banner, and `LEANBACK_LAUNCHER`. This package only merges `touchscreen` and `leanback` with `required="false"`.

## Apple TV

Player Settings: switch the build target to **tvOS**. This package does not add the iOS pod `BidscubeSDKAppLovin` on that export. AppLovin MAX is not used on Apple TV.

Direct video from `com.bidscube.sdk` is fullscreen. `BidscubeTvRemoteDriver` starts automatically on tvOS and wires the Siri Remote:

- **Click** (Select) activates the focused button. Skip receives focus when it is armed. After the video, Close receives focus. Buttons are larger.
- **Menu** matches Android TV Back. Interstitial ignores Menu until Skip is armed, then skips. Rewarded hides Skip and asks “Leave video?” / “Keep watching”. Leave does not grant a reward. Keep watching resumes playback.
- Close cannot be pressed during linear playback.

```csharp
BidscubeSDK.Initialize(new SDKConfig.Builder().EnableLogging(true).Build());
BidscubeSDK.ShowInterstitialVideoAd(placementId, callback);
BidscubeSDK.ShowRewardedVideoAd(rewardedPlacementId, rewardedCallback);
```
