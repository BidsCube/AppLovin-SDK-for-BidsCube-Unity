# Release checklist — `com.bidscube.applovin.max`

Run [`tools/verify-release-ready.sh`](tools/verify-release-ready.sh) before tagging.

## Release `v1.0.15` (hotfix)

- [ ] `package.json` version is **1.0.15**
- [ ] `AdapterPackageInfo.UpmVersion` is **1.0.15**
- [ ] README / docs mention **`v1.0.15`** where applicable
- [ ] CHANGELOG has **`## [1.0.15]`**
- [ ] Editor `.meta` files exist for `Editor/*.cs` (Unity imports immutable UPM Editor assemblies)
- [ ] **`LiteNoVideo`** is **default** (bootstrap + EditorPrefs + new **`BidscubeAndroidExportSettings`**)
- [ ] **`LiteNoVideo`** Gradle: **`files('libs/bidscube-sdk-lite-…')`** only — **no** `com.bidscube:bidscube-sdk`, **no** Media3, **no** IMA
- [ ] **`FullWithVideo`** requires **`bidscube-sdk-1.2.3.aar`** OR **`MavenBidscubeSdkAar`** — otherwise Android export **errors** in Editor (no phantom Maven line)
- [ ] Official AppLovin MAX Unity SDK remains **external**

## Git tag (after validation)

```bash
git add .
git commit -m "Hotfix AppLovin Unity adapter 1.0.15"
git tag v1.0.15
git push origin main
git push origin v1.0.15
```
