# Release checklist — `com.bidscube.applovin.max`

Повний процес: [`RELEASE.md`](RELEASE.md).

## Перед тегом

- [ ] `package.json` / `AdapterPackageInfo.UpmVersion` = поточна UPM-версія (зараз **1.0.20**)
- [ ] `README.md` та **`Documentation~/INSTALL.md`** — приклади **`#v…`** збігаються з тегами
- [ ] `CHANGELOG.md` — секція **`## [версія]`**
- [ ] `bash tools/verify-release-ready.sh` — exit **0**

## Тег і push

```bash
bash tools/verify-release-ready.sh
git add -A && git status
git commit -m "Release com.bidscube.applovin.max <версія>"
git tag -a "v<версія>" -m "com.bidscube.applovin.max <версія>"
git push origin main && git push origin "v<версія>"
```

Спочатку реліз **`com.bidscube.sdk`**, якщо змінився peer у `package.json`.
