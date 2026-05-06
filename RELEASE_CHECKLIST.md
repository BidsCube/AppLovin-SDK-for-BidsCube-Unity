# Release checklist — `com.bidscube.applovin.max`

## Release `v1.0.16`

- [ ] `package.json` version is **1.0.16**
- [ ] `dependencies` → **`com.bidscube.sdk`** is **1.2.6**
- [ ] `AdapterPackageInfo.UpmVersion` is **1.0.16**
- [ ] README / docs mention **`v1.0.16`** where applicable (git URL examples)
- [ ] CHANGELOG has **`## [1.0.16]`**
- [ ] Run **`bash tools/verify-release-ready.sh`** from repo root (exit 0)

## Tag and push

```bash
cd AppLovin-SDK-Unity
bash tools/verify-release-ready.sh
git add -A && git status
git commit -m "Release com.bidscube.applovin.max 1.0.16"
git tag v1.0.16
git push origin main && git push origin v1.0.16
```

Create a **GitHub Release** from **`v1.0.16`**; paste **`CHANGELOG`** section **`[1.0.16]`** into release notes.

**Order:** release **`com.bidscube.sdk` `v1.2.6`** first (this package declares a hard peer on **1.2.6**).
