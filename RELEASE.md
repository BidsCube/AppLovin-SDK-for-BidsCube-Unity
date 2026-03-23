# Releasing `com.bidscube.sdk` (Bidscube Unity / AppLovin MAX)

## Узгоджені імена (конгруентність)

| Що | Формат | Приклад (1.0.0) |
|----|--------|------------------|
| **UPM `package.json` → `version`** | `MAJOR.MINOR.PATCH` без префікса `v` | `1.0.0` |
| **Git tag** | `v` + той самий semver | `v1.0.0` |
| **GitHub Release asset (ZIP)** | `Bidscube-SDK-Unity-{version}.zip` | `Bidscube-SDK-Unity-1.0.0.zip` |
| **GitHub Release title** (workflow) | `com.bidscube.sdk {version}` | `com.bidscube.sdk 1.0.0` |
| **Репозиторій GitHub** (рекомендовано) | `AppLovin-SDK-Unity` | `github.com/BidsCube/AppLovin-SDK-Unity` |
| **`Constants.SdkVersion`** | Рядок **нативного** Bidscube SDK / AAR | зазвичай збігається з **`bidscube-sdk-*.aar`**; може збігатися з UPM `1.0.0` при узгодженому релізі |

Якщо на remote уже є **інший** коміт під тегом **`v1.0.0`** (наприклад, стара лінія Level Play), **не** пуште новий коміт на той самий тег без узгодження: видаліть старий тег на GitHub (**Releases / Tags** або `git push origin :refs/tags/v1.0.0`) і лише потім створіть **`v1.0.0`** на актуальному коміті, або оберіть новий номер (`1.0.1`, `1.1.0`).

## Version sources

1. **`package.json`** — версія пакета для UPM; має дорівнювати тегу без `v`.
2. **`Runtime/BidscubeSDK/Core/Constants.cs` → `SdkVersion`** — версія мережевого / нативного SDK (узгоджуйте з `bidscube-sdk-*.aar`).

## Перевірка перед релізом

```bash
./tools/verify-release-ready.sh
```

## Pre-release checklist

- [ ] `CHANGELOG.md` містить запис для цієї версії.
- [ ] `README.md` / `Documentation~/` — приклади `#vX.Y.Z` узгоджені з `package.json`.
- [ ] `package.json` → `repository.url` вказує на **фактичний** GitHub-репозиторій.
- [ ] Bundled **`bidscube-sdk-*.aar`** і документація версій узгоджені з релізом нативки (за потреби).

## Створити реліз на GitHub

1. Закомітити зміни на `main` (або релізну гілку).
2. Переконатися, що **`package.json` → `version`** = майбутній тег **без** `v`.
3. Створити анотований тег:

```bash
git tag -a "v1.0.0" -m "com.bidscube.sdk 1.0.0"
git push origin "v1.0.0"
```

4. Workflow **Release (GitHub)** (`.github/workflows/release.yml`) перевірить `package.json`, збере ZIP без `.git` і створить GitHub Release з файлом **`Bidscube-SDK-Unity-1.0.0.zip`**.

**Споживачі UPM:**

```json
"com.bidscube.sdk": "https://github.com/BidsCube/AppLovin-SDK-Unity.git#v1.0.0"
```

## Перейменування репозиторію з LevelPlay

Якщо `origin` ще вказує на старе ім’я (наприклад `LevelPlay-SDK-for-BidsCube-Unity`):

```bash
git remote set-url origin git@github.com:BidsCube/AppLovin-SDK-Unity.git
```

На GitHub: **Settings → General → Repository name** → `AppLovin-SDK-Unity` (або ваше узгоджене ім’я), потім оновіть `package.json` → `repository.url` і посилання в README.

## GitHub Actions

- **CI** (`.github/workflows/ci.yml`) — валідація `package.json` на push/PR.
- **Release** — лише на push тегів `v*`; `package.json` **має** дорівнювати тегу без `v`.
