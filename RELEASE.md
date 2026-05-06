# Releasing `com.bidscube.applovin.max` (Bidscube AppLovin MAX adapter for Unity)

## Naming conventions (congruence)

| Item | Format | Example (1.0.14) |
|------|--------|------------------|
| **UPM `package.json` → `version`** | **`MAJOR.MINOR.PATCH`** only (Unity rejects `1.0.3.1`-style fourth segment). Optional prerelease: `1.0.14-rc.1`. | `1.0.14` |
| **Git tag** | `v` + the same version string | `v1.0.14` |
| **GitHub Release asset (ZIP)** | `Bidscube-SDK-Unity-{version}.zip` | `Bidscube-SDK-Unity-1.0.14.zip` |
| **GitHub Release title** (workflow) | `com.bidscube.applovin.max {version}` | `com.bidscube.applovin.max 1.0.14` |
| **GitHub repository** (recommended) | `AppLovin-SDK-Unity` | `github.com/BidsCube/AppLovin-SDK-Unity` |
| **`AdapterPackageInfo.UpmVersion`** | Must match UPM `version` | In `Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs` (companion to **`com.bidscube.sdk`**) |
| **iOS `BidscubeSDKAppLovin` pod** | Optional manual Podfile | Often **1.0.4** while UPM is **1.0.x** until native ships a new adapter |



## Version sources

1. **`package.json`** — UPM package version; must equal the tag **without** `v` (`com.bidscube.applovin.max`). **`dependencies` → `com.bidscube.sdk`** (e.g. **1.2.7**) is the **declared** core peer — update README / `INTEGRATION.md` examples when you bump it; keep it aligned with what you ship in **bidscube-sdk-unity**.
2. **`AdapterPackageInfo.UpmVersion`** — must match **`package.json`**. **`NativeAndroidBidscubeSdkVersion`** / **`BundledMaxAdapterAarVersion`** must match the bundled AAR **filenames** on disk. See also **`Documentation~/ANDROID_BUNDLED_SDK.md`** — **Version matrix**.

## Pre-release check

```bash
./tools/verify-release-ready.sh
```

## Pre-release checklist

- [ ] `CHANGELOG.md` includes an entry for this version.
- [ ] `README.md` / `Documentation~/` — `#vX.Y.Z` examples match `package.json`.
- [ ] `package.json` → `repository.url` points to the **actual** GitHub repository.
- [ ] Bundled **`applovin-bidscube-max-adapter-*.aar`**, **`bidscube-sdk-lite-1.2.3.aar`** (or new **`<ver>`** after a bump), **`.meta`** import flags, **`AdapterPackageInfo`**, and **`package.json` → `com.bidscube.sdk`** match the native / UPM releases you intend to support.

## Create a release on GitHub

1. Commit changes on `main` (or your release branch).
2. Ensure **`package.json` → `version`** equals the upcoming tag **without** `v`.
3. Create an annotated tag:

```bash
git tag -a "v1.0.14" -m "com.bidscube.applovin.max 1.0.14"
git push origin "v1.0.14"
```

4. The **Release (GitHub)** workflow (`.github/workflows/release.yml`) validates `package.json`, builds a ZIP without `.git`, and creates a GitHub Release with **`Bidscube-SDK-Unity-{version}.zip`** (e.g. **`Bidscube-SDK-Unity-1.0.14.zip`**).

### Release `v1.0.14` (2026-05-04)

- Bump **`package.json`** / **`AdapterPackageInfo.UpmVersion`** to **1.0.14** and document **[CHANGELOG](CHANGELOG.md)** section **`[1.0.14]`**.
- Run **`./tools/verify-release-ready.sh`** (checks README / CHANGELOG / Gradle branching).
- Android **default** export mode is **`FullWithVideo`**; **`LiteNoVideo`** remains opt-in via **Tools → Bidscube SDK → Android Build Features** or **`BidscubeAndroidExportSettings`**.

**UPM consumers:**

```json
"com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.14"
```

Add **`com.bidscube.sdk`** separately (see [bidscube-sdk-unity](https://github.com/BidsCube/bidscube-sdk-unity)).

## Renaming the repository from LevelPlay

If `origin` still uses the old name (e.g. `LevelPlay-SDK-for-BidsCube-Unity`):

```bash
git remote set-url origin git@github.com:BidsCube/AppLovin-SDK-Unity.git
```

On GitHub: **Settings → General → Repository name** → `AppLovin-SDK-Unity` (or your chosen name), then update `package.json` → `repository.url` and links in the README.

## GitHub Actions (where workflows live)

Workflow files are under **`.github/workflows/`** at the package repository root. If workflows do **not** appear under the **Actions** tab on GitHub, common causes are:

- The **`.github`** folder has **not** been pushed to GitHub.
- Workflows exist only on a branch that is **not** the default — GitHub lists workflows from the **default branch**.
- The repo is a **fork** with Actions disabled (**Settings → Actions**).

### CI

- **`ci.yml`** — checks on push/PR to `main` / `master`.

### Release (UPM ZIP + GitHub Release)

- **`release.yml`**:
  1. **Via tag:** after `git push origin v1.0.14` (or the current `v*` tag). The value in `package.json` must match the tag without `v`.
  2. **Manual:** **Actions → “Release (GitHub)” → Run workflow** → select a branch (e.g. `main`). Version is read from `package.json`; a GitHub Release with the ZIP is created. If a release for that tag already exists, the step fails.

Quick local check before tagging: `./tools/verify-release-ready.sh`.

## Post-publish Android validation (maintainers)

After bumping **`NativeAndroidBidscubeSdkVersion`** or the Gradle post-processor, validate that the core SDK **classes** reach the app, not only POM metadata:

1. Export an Android project from Unity (or use your CI template that runs Gradle).
2. From the Gradle project root: `./gradlew :unityLibrary:dependencies --configuration releaseRuntimeClasspath --refresh-dependencies` (or `debugRuntimeClasspath`) and confirm **`com.bidscube:bidscube-sdk`** resolves to an **AAR** (or run `./gradlew :unityLibrary:dependencyInsight --dependency bidscube-sdk --configuration releaseRuntimeClasspath`).
3. Build **`assembleRelease`** (or **`bundleRelease`**), then confirm the APK/AAB contains **`com/bidscube/sdk/BidscubeSDK.class`** (e.g. `unzip -l build/outputs/apk/.../….apk | grep BidscubeSDK` or **`dexdump`** / **`bundletool`** as appropriate). If the class is missing, fail the pipeline before tagging.

