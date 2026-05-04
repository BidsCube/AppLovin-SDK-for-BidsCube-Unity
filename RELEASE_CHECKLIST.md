# Release checklist — `com.bidscube.applovin.max`

Run [`tools/verify-release-ready.sh`](tools/verify-release-ready.sh) before tagging.

## Release `v1.0.14`

- [ ] `package.json` version is **1.0.14**
- [ ] `AdapterPackageInfo.UpmVersion` is **1.0.14**
- [ ] README uses **`v1.0.14`** / **`1.0.14`** where applicable
- [ ] CHANGELOG has **`## [1.0.14]`**
- [ ] **`FullWithVideo`** is the **default** (bootstrap, EditorPrefs default, new **`BidscubeAndroidExportSettings`** assets)
- [ ] First Unity import does **not** define **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`** on Android (until user selects **LiteNoVideo**)
- [ ] Selecting **LiteNoVideo** adds **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`**
- [ ] Selecting **FullWithVideo** removes **`BIDSCUBE_ANDROID_LITE_NO_VIDEO`**
- [ ] **FullWithVideo** Gradle export contains **Media3**
- [ ] **FullWithVideo** Gradle export contains **Google IMA** (`interactivemedia`)
- [ ] **LiteNoVideo** Gradle export does **NOT** contain **Media3**
- [ ] **LiteNoVideo** Gradle export does **NOT** contain **Google IMA**
- [ ] Exactly **one** Bidscube core dependency line (lite **`files`** or full **`@aar`** / **`files`**, never both cores)
- [ ] Official **AppLovin MAX Unity SDK** is still **external** (not in this package / not in `package.json` dependencies except peer **`com.bidscube.sdk`**)
- [ ] **Unity-Test-App** builds with **FullWithVideo**
- [ ] **Unity-Test-App** builds with **LiteNoVideo**
- [ ] MAX **Mediation Debugger** opens (official MAX plugin in host project)
- [ ] **Banner** flow works
- [ ] **Rewarded/video** flow works in **FullWithVideo**
- [ ] Video APIs **fail gracefully** in **LiteNoVideo**

## Git tag (after checks)

```bash
git add .
git commit -m "Release AppLovin Unity adapter 1.0.14"
git tag v1.0.14
git push origin main
git push origin v1.0.14
```
