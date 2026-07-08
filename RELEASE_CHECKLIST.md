# Release checklist

Full process: [RELEASE.md](RELEASE.md).

## Before tagging

- [ ] Run **`bash tools/verify-release-ready.sh`** (exit **0**)
- [ ] Bundled Android adapter AAR replaced with fixed version (**`applovin-bidscube-max-adapter-1.2.10.aar`**)
- [ ] No **`bidscube_test_signal`** in bundled adapter AAR (forbidden-string check passes)
- [ ] No Native MAX dummy/support claim in docs or adapter AAR
- [ ] **`AdapterPackageInfo`** versions match AAR filenames and iOS pod pin (**`BidscubeSDKAppLovin` 1.1.0**)
- [ ] **`BidscubeIosPodfilePostprocessor`** adds **`BidscubeSDKAppLovin`** on iOS export
- [ ] **`AppLovinMaxRewardedBridge.EnableDirectSdkFallback`** defaults to **`false`**
- [ ] **`README.md`** / **`Documentation~/INSTALL.md`** — MAX is primary load/show path; direct fallback is opt-in only
- [ ] **`package.json`** and **`AdapterPackageInfo.UpmVersion`** match the UPM version you are releasing (currently **1.0.24**)
- [ ] Bundled core AARs **`bidscube-sdk-*-1.2.10.aar`** match MAX adapter **`1.2.10`** (no **1.2.5** stale files)
- [ ] **`./tools/verify-release-ready.sh`** reports **Android adapter/core method compatibility OK**
- [ ] **`CHANGELOG.md`** has a **`## [version]`** section for this release
- [ ] Sibling **`BidscubeUnityAppLovinTestApp`** exists for MAX QA
- [ ] Release ZIP has no **`.git`**, **`__MACOSX`**, **`._*`**, **`Library/`**, **`Temp/`**, **`Obj/`**, **`Build/`**, **`Builds/`**
- [ ] **OpenRTB:** `OpenRtb26*ResponseParsingSupported` flags match native releases; docs use Option A/B/C (currently **Option C**); `./tools/verify-release-ready.sh` OpenRTB checks pass

## Tag and push

```bash
git commit -m "Release com.bidscube.applovin.max <version>"
git tag -a "v<version>" -m "com.bidscube.applovin.max <version>"
git push origin main && git push origin "v<version>"
```

Release **`com.bidscube.sdk`** first if you bump the peer in `package.json`.

Create release archives with **`git archive`** or CI — not Finder (avoids **`.git`**, **`__MACOSX`**, **`._*`** in ZIP).
