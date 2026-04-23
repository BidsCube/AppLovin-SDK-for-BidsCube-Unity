# Releasing `com.bidscube.sdk` (Bidscube Unity / AppLovin MAX)

## Naming conventions (congruence)

| Item | Format | Example (1.0.11) |
|------|--------|------------------|
| **UPM `package.json` → `version`** | **`MAJOR.MINOR.PATCH`** only (Unity rejects `1.0.3.1`-style fourth segment). Optional prerelease: `1.0.11-rc.1`. | `1.0.11` |
| **Git tag** | `v` + the same version string | `v1.0.11` |
| **GitHub Release asset (ZIP)** | `Bidscube-SDK-Unity-{version}.zip` | `Bidscube-SDK-Unity-1.0.11.zip` |
| **GitHub Release title** (workflow) | `com.bidscube.sdk {version}` | `com.bidscube.sdk 1.0.11` |
| **GitHub repository** (recommended) | `AppLovin-SDK-Unity` | `github.com/BidsCube/AppLovin-SDK-Unity` |
| **`Constants.SdkVersion`** | Unity package / user-agent string | Must match UPM `version`; **`Constants.NativeAndroidBidscubeSdkVersion`** must match **`bidscube-sdk-<ver>.aar`** and the **`files('libs/…')`** line (default **BundledUnityLibraryLibsAar**); **`MavenBidscubeSdkAar`** uses **`…@aar`** from repos; **`CoreDependencyMode`** overrides |
| **`BidscubeIosPodfilePostprocessor.BidscubeAppLovinPodVersion`** | CocoaPods **`BidscubeSDKAppLovin`** line appended to exported Podfile | May match UPM or stay on an earlier native pin (e.g. UPM **1.0.11** with pod **1.0.4**) until CocoaPods ships a newer adapter |



## Version sources

1. **`package.json`** — UPM package version; must equal the tag **without** `v`.
2. **`Runtime/BidscubeSDK/Core/Constants.cs` → `SdkVersion`** — Unity / user-agent version (match **`package.json`**). **`NativeAndroidBidscubeSdkVersion`** must match the bundled **`Runtime/Plugins/Android/bidscube-sdk-<ver>.aar`** filename and the copied **`unityLibrary/libs/`** artifact (default **`BundledUnityLibraryLibsAar`**). For **`MavenBidscubeSdkAar`**, keep the same semver as the published **`com.bidscube:bidscube-sdk`** coordinate. If you use **`CustomGradleLines`** / **`SkipInjectionIntegratorOwnsCore`**, keep the constant aligned with the native AAR you actually link for support parity.

## Pre-release check

```bash
./tools/verify-release-ready.sh
```

## Pre-release checklist

- [ ] `CHANGELOG.md` includes an entry for this version.
- [ ] `README.md` / `Documentation~/` — `#vX.Y.Z` examples match `package.json`.
- [ ] `package.json` → `repository.url` points to the **actual** GitHub repository.
- [ ] Bundled **`applovin-bidscube-max-adapter-*.aar`**, optional reference **`bidscube-sdk-<ver>.aar`** (filename matches **`NativeAndroidBidscubeSdkVersion`**; `.meta` keeps Android import off), **`NativeAndroidBidscubeSdkVersion`** / Gradle inject, and docs match the native Android release (if applicable).

## Create a release on GitHub

1. Commit changes on `main` (or your release branch).
2. Ensure **`package.json` → `version`** equals the upcoming tag **without** `v`.
3. Create an annotated tag:

```bash
git tag -a "v1.0.11" -m "com.bidscube.sdk 1.0.11"
git push origin "v1.0.11"
```

4. The **Release (GitHub)** workflow (`.github/workflows/release.yml`) validates `package.json`, builds a ZIP without `.git`, and creates a GitHub Release with **`Bidscube-SDK-Unity-{version}.zip`** (e.g. **`Bidscube-SDK-Unity-1.0.11.zip`**).

**UPM consumers:**

```json
"com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.11"
```

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
  1. **Via tag:** after `git push origin v1.0.11` (tag `v*`). The value in `package.json` must match the tag without `v`.
  2. **Manual:** **Actions → “Release (GitHub)” → Run workflow** → select a branch (e.g. `main`). Version is read from `package.json`; a GitHub Release with the ZIP is created. If a release for that tag already exists, the step fails.

Quick local check before tagging: `./tools/verify-release-ready.sh`.

## Post-publish Android validation (maintainers)

After bumping **`NativeAndroidBidscubeSdkVersion`** or the Gradle post-processor, validate that the core SDK **classes** reach the app, not only POM metadata:

1. Export an Android project from Unity (or use your CI template that runs Gradle).
2. From the Gradle project root: `./gradlew :unityLibrary:dependencies --configuration releaseRuntimeClasspath --refresh-dependencies` (or `debugRuntimeClasspath`) and confirm **`com.bidscube:bidscube-sdk`** resolves to an **AAR** (or run `./gradlew :unityLibrary:dependencyInsight --dependency bidscube-sdk --configuration releaseRuntimeClasspath`).
3. Build **`assembleRelease`** (or **`bundleRelease`**), then confirm the APK/AAB contains **`com/bidscube/sdk/BidscubeSDK.class`** (e.g. `unzip -l build/outputs/apk/.../….apk | grep BidscubeSDK` or **`dexdump`** / **`bundletool`** as appropriate). If the class is missing, fail the pipeline before tagging.

