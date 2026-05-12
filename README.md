# Bidscube AppLovin MAX adapter (`com.bidscube.applovin.max`)

UPM package **1.0.21** (Git tag **`v1.0.21`**) — adds the Bidscube **AppLovin MAX** Android/iOS bridge, bundled native AARs, and reflection hooks to **`MaxSdk`**. **Requires** the core Unity SDK **`com.bidscube.sdk`** (declared as **1.2.11** in this package’s `package.json`).

Full install, manifest snippets, dashboard, and troubleshooting: **[Documentation~/INSTALL.md](Documentation~/INSTALL.md)**.

## AppLovin MAX Setup

1. Add this package to the Unity project.
2. Add or verify **`com.bidscube.sdk`** (see `package.json`; host app usually pins [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) **#v1.2.11**) and the **official AppLovin MAX Unity SDK** — this adapter does **not** ship **`MaxSdk`**.
3. Open the sample scene (**Package Manager → Samples → SDK Demo**) or your integration scene.
4. Enter the AppLovin MAX **SDK key**.
5. Enter AppLovin **ad unit IDs**.
6. Run **External Dependency Manager → Android Resolver → Force Resolve**.
7. Build an **Android APK** (**File → Build Settings → Android**).

## Android modes

**LiteNoVideo**

- uses `bidscube-sdk-lite-no-video`
- no rewarded/video support
- no Media3 / Google IMA
- no core library desugaring

**WebViewVideoNoDesugar**

- uses `bidscube-sdk-webview-video`
- HTML5 video through Android WebView
- no Media3 / Google IMA
- no core library desugaring

**LegacyMediaVideoNoDesugar**

- uses `bidscube-sdk-legacy-media-video`
- video through `VideoView` / `MediaPlayer`
- no Media3 / Google IMA
- no core library desugaring

**FullWithVideo**

- uses `bidscube-sdk-full-video`
- full video support with Google IMA / Media3
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

## More

- [CHANGELOG.md](CHANGELOG.md)  
- [LICENSE.md](LICENSE.md)  
- Maintainers: [RELEASE.md](RELEASE.md), [docs/internal/README.md](docs/internal/README.md)
