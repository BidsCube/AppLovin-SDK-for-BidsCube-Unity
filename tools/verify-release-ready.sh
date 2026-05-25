#!/usr/bin/env bash
# Run from repo root before tagging. Does not create tags.
# Package: com.bidscube.applovin.max (companion to com.bidscube.sdk).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

VER="$(python3 -c "import json; print(json.load(open('package.json'))['version'])")"
echo "== Bidscube AppLovin MAX adapter UPM release check =="
echo "package.json version: $VER"
echo ""

# Unity UPM: MAJOR.MINOR.PATCH (three numeric parts); optional prerelease (-suffix).
if [[ ! "$VER" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  echo "ERROR: version must be MAJOR.MINOR.PATCH with optional prerelease (e.g. 1.0.6 or 1.0.0-rc.1). Not 1.0.3.1 — Unity rejects it." >&2
  exit 1
fi

PKG_NAME="$(python3 -c "import json; print(json.load(open('package.json'))['name'])")"
if [[ "$PKG_NAME" != "com.bidscube.applovin.max" ]]; then
  echo "ERROR: expected package name com.bidscube.applovin.max, got: $PKG_NAME" >&2
  exit 1
fi

python3 << 'PY' || exit 1
import json
pkg = json.load(open("package.json"))
deps = pkg.get("dependencies") or {}
if deps.get("com.bidscube.sdk") != "1.2.12":
    raise SystemExit(
        "ERROR: package.json must depend on com.bidscube.sdk 1.2.12 exactly "
        f"(got {deps.get('com.bidscube.sdk')!r})"
    )
for k in deps:
    if k == "com.bidscube.sdk":
        continue
    raise SystemExit(f"ERROR: unexpected package.json dependency {k!r} — keep only com.bidscube.sdk for this adapter")
print("package.json peer dependency OK: com.bidscube.sdk 1.2.12 only")
PY

if ! grep -q "public const string UpmVersion" Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs; then
  echo "ERROR: AdapterPackageInfo.UpmVersion not found" >&2
  exit 1
fi
INFO_VER="$(sed -n 's/.*public const string UpmVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs | head -n1)"
if [[ -z "$INFO_VER" ]]; then
  echo "ERROR: could not parse UpmVersion from AdapterPackageInfo.cs" >&2
  exit 1
fi
if [[ "$INFO_VER" != "$VER" ]]; then
  echo "ERROR: AdapterPackageInfo.UpmVersion is \"$INFO_VER\" but package.json version is \"$VER\"" >&2
  exit 1
fi

NATIVE_VER="$(sed -n 's/.*public const string NativeAndroidBidscubeSdkVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs | head -n1)"
if [[ -z "$NATIVE_VER" ]]; then
  echo "ERROR: could not parse NativeAndroidBidscubeSdkVersion from AdapterPackageInfo.cs" >&2
  exit 1
fi
MAXA_VER="$(sed -n 's/.*public const string BundledMaxAdapterAarVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs | head -n1)"
if [[ -z "$MAXA_VER" ]]; then
  echo "ERROR: could not parse BundledMaxAdapterAarVersion from AdapterPackageInfo.cs" >&2
  exit 1
fi

echo "AdapterPackageInfo.UpmVersion: $INFO_VER (matches package.json)"
echo "NativeAndroidBidscubeSdkVersion: $NATIVE_VER (bundled core AAR filenames)"
echo "BundledMaxAdapterAarVersion: $MAXA_VER"
echo ""

shopt -s nullglob
BIDSCUBE_ADAPTER_AARS=(Runtime/Plugins/Android/applovin-bidscube-max-adapter-*.aar)
if ((${#BIDSCUBE_ADAPTER_AARS[@]} == 0)); then
  echo "ERROR: expected bundled MAX adapter AAR at Runtime/Plugins/Android/applovin-bidscube-max-adapter-*.aar" >&2
  exit 1
fi
echo "Bundled MAX adapter AAR: ${BIDSCUBE_ADAPTER_AARS[*]}"

LITE_AAR="Runtime/Plugins/Android/bidscube-sdk-lite-no-video-${NATIVE_VER}.aar"
if [[ ! -f "$LITE_AAR" ]]; then
  echo "ERROR: expected bundled lite core AAR at $LITE_AAR" >&2
  exit 1
fi
echo "Bundled lite core AAR: $LITE_AAR"
WEBVIEW_AAR="Runtime/Plugins/Android/bidscube-sdk-webview-video-${NATIVE_VER}.aar"
if [[ ! -f "$WEBVIEW_AAR" ]]; then
  echo "ERROR: expected bundled webview-video core AAR at $WEBVIEW_AAR" >&2
  exit 1
fi
echo "Bundled webview-video core AAR: $WEBVIEW_AAR"
LEGACY_AAR="Runtime/Plugins/Android/bidscube-sdk-legacy-media-video-${NATIVE_VER}.aar"
if [[ ! -f "$LEGACY_AAR" ]]; then
  echo "ERROR: expected bundled legacy-media-video core AAR at $LEGACY_AAR" >&2
  exit 1
fi
echo "Bundled legacy-media-video core AAR: $LEGACY_AAR"
FULL_AAR="Runtime/Plugins/Android/bidscube-sdk-full-video-${NATIVE_VER}.aar"
if [[ ! -f "$FULL_AAR" ]]; then
  echo "ERROR: expected bundled full-video core AAR at $FULL_AAR" >&2
  exit 1
fi
echo "Bundled full-video core AAR: $FULL_AAR"

python3 << 'PY' || exit 1
import io
import zipfile
from pathlib import Path

native_ver = Path("Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs").read_text(encoding="utf-8")
import re
m = re.search(r'NativeAndroidBidscubeSdkVersion = "([^"]+)"', native_ver)
if not m:
    raise SystemExit("ERROR: could not parse NativeAndroidBidscubeSdkVersion")
ver = m.group(1)
core_aars = [
    f"Runtime/Plugins/Android/bidscube-sdk-lite-no-video-{ver}.aar",
    f"Runtime/Plugins/Android/bidscube-sdk-webview-video-{ver}.aar",
    f"Runtime/Plugins/Android/bidscube-sdk-legacy-media-video-{ver}.aar",
    f"Runtime/Plugins/Android/bidscube-sdk-full-video-{ver}.aar",
]
need = [b"showInterstitialVideoAd", b"showRewardedVideoAd"]
sdk_class = "com/bidscube/sdk/BidscubeSDK.class"
for aar_path in core_aars:
    p = Path(aar_path)
    if not p.is_file():
        raise SystemExit(f"ERROR: missing core AAR {aar_path}")
    with zipfile.ZipFile(p) as outer:
        inner = zipfile.ZipFile(io.BytesIO(outer.read("classes.jar")))
        sdk = inner.read(sdk_class)
    missing = [s.decode() for s in need if s not in sdk]
    if missing:
        raise SystemExit(f"ERROR: {aar_path} missing BidscubeSDK methods: {missing}")
print("All four bundled core AARs expose showInterstitialVideoAd / showRewardedVideoAd")
PY

# Filename on disk must match our declared MAX adapter version.
if [[ ! -f "Runtime/Plugins/Android/applovin-bidscube-max-adapter-${MAXA_VER}.aar" ]]; then
  echo "ERROR: MAX adapter AAR on disk must match BundledMaxAdapterAarVersion ($MAXA_VER)" >&2
  exit 1
fi

if [[ ! -f "Runtime/BidscubeSDK/Mediation/AppLovinMaxUnityReflection.cs" ]]; then
  echo "ERROR: AppLovinMaxUnityReflection.cs is required" >&2
  exit 1
fi

if ! grep -q "v${VER}" README.md; then
  echo "ERROR: README.md should mention git tag v${VER}" >&2
  exit 1
fi

if ! grep -qF "## [${VER}]" CHANGELOG.md; then
  echo "ERROR: CHANGELOG.md missing section ## [${VER}]" >&2
  exit 1
fi

POST="Editor/Android/BidscubeAndroidGradlePostprocessor.cs"
grep -q "BidscubeAndroidGradleProjectPatcher" "$POST" || { echo "ERROR: $POST must delegate to BidscubeAndroidGradleProjectPatcher" >&2; exit 1; }
PATCHER="$ROOT/../bidscube-sdk-unity/Editor/Android/BidscubeAndroidGradleProjectPatcher.cs"
if [[ -f "$PATCHER" ]]; then
  for need in "DescribeFeatureSet" "\"FullWithVideo\"" "\"LiteNoVideo\"" \
    "\"WebViewVideoNoDesugar\"" "\"LegacyMediaVideoNoDesugar\"" \
    "Skipping Media3 and Google IMA dependencies" "Including Media3 and Google IMA dependencies" \
    "androidx.media3:media3-common" "interactivemedia" "sdk-webview-video" \
    "sdk-legacy-media-video" "sdk-full-video" "ApplyDesugaringPolicyLite"; do
    if ! grep -qF "$need" "$PATCHER"; then
      echo "ERROR: $PATCHER must contain: $need" >&2
      exit 1
    fi
  done
  python3 -c "from pathlib import Path; t=Path(r'$PATCHER').read_text(encoding='utf-8'); assert t.index('Skipping Media3 and Google IMA dependencies') < t.index('static void AppendVideoDeps')"
else
  echo "WARN: sibling bidscube-sdk-unity not found at $PATCHER — skipping deep Gradle patcher string checks"
fi

python3 -c 'import json; d=json.load(open("package.json"))["dependencies"]; assert "com.applovin.mediation.ads" not in d, "package.json must not list com.applovin.mediation.ads"'

if git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  if git ls-files | grep -E '\.(apk|aab|ipa)$' | grep -q .; then
    echo "ERROR: APK/AAB/IPA files must not be tracked" >&2
    exit 1
  fi
  if git ls-files | grep -qE '^(Library/|Temp/|Obj/|Logs/)'; then
    echo "ERROR: Unity-generated folders must not be tracked (Library/, Temp/, Obj/, Logs/)" >&2
    exit 1
  fi
fi

python3 tools/validate-android-gradle-modes.py

python3 << 'PY' || exit 1
import os
from pathlib import Path
root = Path(".")
roots = [root / "Editor", root / "Runtime", root / "tools"]
missing = []
for base in roots:
    if not base.is_dir():
        continue
    for dirpath, _, files in os.walk(base):
        for name in files:
            if name.startswith(".") or name.endswith(".meta"):
                continue
            p = Path(dirpath) / name
            if "WebView.bundle" in p.parts:
                continue
            meta = Path(str(p) + ".meta")
            if not meta.exists():
                missing.append(str(p))
extra = root / "RELEASE_CHECKLIST.md"
if extra.is_file() and not Path(str(extra) + ".meta").exists():
    missing.append(str(extra))
if missing:
    import sys
    print("ERROR: missing .meta for:", file=sys.stderr)
    for m in missing:
        print(f"  {m}", file=sys.stderr)
    raise SystemExit(1)
print("Unity .meta pairing OK (Editor, Runtime, tools, RELEASE_CHECKLIST.md)")
PY

python3 << 'PY' || exit 1
from pathlib import Path
stale = []
for meta in Path(".").rglob("*.meta"):
    if ".git" in meta.parts:
        continue
    asset = Path(str(meta)[:-5])
    if not asset.exists():
        stale.append(str(meta))
if stale:
    print("ERROR: stale .meta (target missing):", file=__import__("sys").stderr)
    for s in stale:
        print(f"  {s}", file=__import__("sys").stderr)
    raise SystemExit(1)
print("No stale .meta files")
PY

echo "Suggested git tag (must match package.json): v$VER"
echo "  git tag -a \"v$VER\" -m \"com.bidscube.applovin.max $VER\""
echo "  git push origin \"v$VER\""
echo ""
echo "GitHub Actions will produce: Bidscube-SDK-Unity-${VER}.zip"
echo "Release title pattern: com.bidscube.applovin.max ${VER}"
echo ""
echo "Optional: ensure CHANGELOG.md documents this version before tagging."
