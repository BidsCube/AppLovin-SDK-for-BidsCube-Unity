# Release checklist

Full process: [RELEASE.md](RELEASE.md).

## Before tagging

- [ ] Run **`bash tools/verify-release-ready.sh`** (exit **0**)
- [ ] `package.json` and `AdapterPackageInfo.UpmVersion` match the UPM version you are releasing (currently **1.0.22**)
- [ ] `AdapterPackageInfo.BundledMaxAdapterAarVersion` matches **`applovin-bidscube-max-adapter-*.aar`** on disk (currently **1.2.6**)
- [ ] `README.md` and **`Documentation~/INSTALL.md`** — `#v…` examples match published tags
- [ ] `CHANGELOG.md` has a **`## [version]`** section for this release

## Tag and push

```bash
git commit -m "Release com.bidscube.applovin.max <version>"
git tag -a "v<version>" -m "com.bidscube.applovin.max <version>"
git push origin main && git push origin "v<version>"
```

Release **`com.bidscube.sdk`** first if you bump the peer in `package.json`.
