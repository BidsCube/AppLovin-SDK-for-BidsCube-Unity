# Minimal test plan (Unity)

## Android: custom SSP host (`AdRequestAuthority`)

1. Build an Android player with `SDKConfig.Builder().AdRequestAuthority("your-staging-host.com").Build()` and `BidscubeSDK.Initialize(config)` (or `BaseURL("https://your-staging-host/sdk")` — same normalization).
2. Capture traffic (proxy / logcat network tags per your setup).
3. Confirm ad requests target the **configured host**, not only the default production URL.

## AppLovin MAX: shared native instance (smoke)

1. Use `IntegrationMode(BidscubeIntegrationMode.AppLovinMaxMediation)` and initialize **before** MAX SDK init (same order as Flutter).
2. Add MAX + Bidscube adapter dependencies per adapter documentation.
3. Load/show a **test** banner or interstitial through MAX.
4. Expect: no `InvalidOperationException` from Bidscube C# creative APIs (you should not call them); adapter successfully uses the pre-initialized native Bidscube SDK.

## Direct SDK mode (regression)

1. `IntegrationMode(BidscubeIntegrationMode.DirectSdk)` (or default).
2. Exercise banner / image / video / native from sample scene; confirm callbacks and visuals.

## Legacy config

1. `IntegrationModeFromWire("levelPlay")` resolves to **AppLovin MAX mediation** and behaves like MAX mode (creative APIs throw).
