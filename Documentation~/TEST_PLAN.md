# Minimal test plan (Unity)

## Android: Gradle export (`@aar`, desugaring, init)

1. Clean Gradle state: remove prior exported **`Library/Bee/.../Gradle`** (or full **`Library`**) in the Unity project, then **Export** / **Build** Android again.
2. In generated **`unityLibrary/build.gradle`**: confirm **`implementation 'com.bidscube:bidscube-sdk:<ver>@aar'`** (post-processor + **`ValidateBidscubeCoreSdkUsesAarSuffix`** in Editor).
3. Confirm this package does **not** append **`coreLibraryDesugaring`** / **`coreLibraryDesugaringEnabled`** — add them in **host** Custom Gradle if **`CheckAarMetadata`** or Java 8+ APIs require desugaring (**`ANDROID_BUNDLED_SDK.md`**).
4. From Gradle project root: **`./gradlew :unityLibrary:assembleRelease --refresh-dependencies`** (or **`assembleDebug`**).
5. Device/emulator (prefer **lower minSdk** you support): smoke **`BidscubeSDK.Initialize`** — expect **`[BidscubeSDK] Init (Android Java): SUCCESS`**, no **`ClassNotFoundException`** for **`com.bidscube.sdk.BidscubeSDK`**.

## Android: custom SSP host (`AdRequestAuthority`)

1. Build an Android player with `SDKConfig.Builder().AdRequestAuthority("your-staging-host.com").Build()` and `BidscubeSDK.Initialize(config)` (or `BaseURL("https://your-staging-host/sdk")` — same normalization).
2. Capture traffic (proxy / logcat network tags per your setup).
3. Confirm ad requests target the **configured host**, not only the default production URL.

## AppLovin MAX: shared native instance (smoke)

1. **Android:** use `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` and C# `Initialize` **before** MAX SDK init (same order as Flutter). **iOS:** either the same C# order or adapter-only init per [AppLovin-SDK-for-BidsCube-iOS](https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-iOS).
2. **Android:** Gradle export includes injected **AppLovin SDK 13+** and bundled adapter AAR. **iOS:** **Podfile** lists **`BidscubeSDKAppLovin`** `1.0.4` and **`AppLovinSDK`** **13.x** (post-processor or manual `pod install`).
3. Load/show a **test** banner or interstitial through MAX.
4. Expect: no `InvalidOperationException` from Bidscube C# creative APIs (you should not call them); mediated loads succeed (**Android:** shared native instance when C# init is used; **iOS:** per adapter / native docs).

## Direct SDK mode (regression)

1. `IntegrationMode(BidscubeIntegrationMode.DirectSdk)` (or default).
2. Exercise banner / image / video / native from sample scene; confirm callbacks and visuals.

## Legacy config

1. `IntegrationModeFromWire("levelPlay")` resolves to **AppLovin MAX mediation** and behaves like MAX mode (creative APIs throw).
