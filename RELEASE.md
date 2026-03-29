# Releasing `com.bidscube.sdk` (Bidscube Unity / AppLovin MAX)

## Naming conventions (congruence)

| Item | Format | Example (1.0.1) |
|------|--------|-----------------|
| **UPM `package.json` → `version`** | `MAJOR.MINOR.PATCH` without a `v` prefix | `1.0.1` |
| **Git tag** | `v` + the same semver | `v1.0.1` |
| **GitHub Release asset (ZIP)** | `Bidscube-SDK-Unity-{version}.zip` | `Bidscube-SDK-Unity-1.0.1.zip` |
| **GitHub Release title** (workflow) | `com.bidscube.sdk {version}` | `com.bidscube.sdk 1.0.1` |
| **GitHub repository** (recommended) | `AppLovin-SDK-Unity` | `github.com/BidsCube/AppLovin-SDK-Unity` |
| **`Constants.SdkVersion`** | Native Bidscube SDK / AAR string | Usually matches **`bidscube-sdk-*.aar`**; may match UPM `1.0.1` when releases are aligned |

If **another commit** already points to tag **`v1.0.0`** on the remote (e.g. old Level Play line), **do not** force-push a new commit to that tag without coordination: delete the old tag on GitHub (**Releases / Tags** or `git push origin :refs/tags/v1.0.0`) and only then create **`v1.0.0`** on the current commit, or pick a new version (`1.0.1`, `1.1.0`).

## Version sources

1. **`package.json`** — UPM package version; must equal the tag **without** `v`.
2. **`Runtime/BidscubeSDK/Core/Constants.cs` → `SdkVersion`** — native / network SDK version (align with `bidscube-sdk-*.aar`).

## Pre-release check

```bash
./tools/verify-release-ready.sh
```

## Pre-release checklist

- [ ] `CHANGELOG.md` includes an entry for this version.
- [ ] `README.md` / `Documentation~/` — `#vX.Y.Z` examples match `package.json`.
- [ ] `package.json` → `repository.url` points to the **actual** GitHub repository.
- [ ] Bundled **`bidscube-sdk-*.aar`** and version docs match the native release (if applicable).

## Create a release on GitHub

1. Commit changes on `main` (or your release branch).
2. Ensure **`package.json` → `version`** equals the upcoming tag **without** `v`.
3. Create an annotated tag:

```bash
git tag -a "v1.0.1" -m "com.bidscube.sdk 1.0.1"
git push origin "v1.0.1"
```

4. The **Release (GitHub)** workflow (`.github/workflows/release.yml`) validates `package.json`, builds a ZIP without `.git`, and creates a GitHub Release with **`Bidscube-SDK-Unity-1.0.1.zip`**.

**UPM consumers:**

```json
"com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.1"
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
  1. **Via tag:** after `git push origin v1.0.1` (tag `v*`). The value in `package.json` must match the tag without `v`.
  2. **Manual:** **Actions → “Release (GitHub)” → Run workflow** → select a branch (e.g. `main`). Version is read from `package.json`; a GitHub Release with the ZIP is created. If a release for that tag already exists, the step fails.

Quick local check before tagging: `./tools/verify-release-ready.sh`.

