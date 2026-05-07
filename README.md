# Bidscube AppLovin MAX adapter (`com.bidscube.applovin.max`)

UPM package **1.0.20** (Git tag **`v1.0.20`**) — adds the Bidscube **AppLovin MAX** Android/iOS bridge, bundled native AARs, and reflection hooks to **`MaxSdk`**. **Requires** the core Unity SDK **`com.bidscube.sdk`** (declared as **1.2.9** in this package’s `package.json`).

Full install, manifest snippets, dashboard, and troubleshooting: **[Documentation~/INSTALL.md](Documentation~/INSTALL.md)**.

## AppLovin MAX Setup

1. Add this package to the Unity project.
2. Add or verify **`com.bidscube.sdk`** (see `package.json`; host app usually pins [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity) **#v1.2.9**) and the **official AppLovin MAX Unity SDK** — this adapter does **not** ship **`MaxSdk`**.
3. Open the sample scene (**Package Manager → Samples → SDK Demo**) or your integration scene.
4. Enter the AppLovin MAX **SDK key**.
5. Enter AppLovin **ad unit IDs**.
6. Run **External Dependency Manager → Android Resolver → Force Resolve**.
7. Build an **Android APK** (**File → Build Settings → Android**).

## Android modes

**Lite / No Video:**

- uses `bidscube-sdk-lite-no-video`
- does not include video dependencies
- does not require core library desugaring

**Full / Video:**

- uses `bidscube-sdk-full-video`
- enables video support
- may enable core library desugaring if video dependencies require it

Toggle in Unity: **Tools → Bidscube SDK → Android Build Features**, or use a **Bidscube Android Export Settings** asset (see INSTALL).

**Lite / No Video** should build without `coreLibraryDesugaringEnabled`. **Full / Video** may inject:

```gradle
coreLibraryDesugaringEnabled true
coreLibraryDesugaring "com.android.tools:desugar_jdk_libs:2.0.4"
```

## Bundled Android AARs

- `bidscube-sdk-lite-no-video-1.2.4.aar`
- `bidscube-sdk-full-video-1.2.4.aar`

## More

- [CHANGELOG.md](CHANGELOG.md)  
- [LICENSE.md](LICENSE.md)  
- Maintainers: [RELEASE.md](RELEASE.md), [docs/internal/README.md](docs/internal/README.md)
