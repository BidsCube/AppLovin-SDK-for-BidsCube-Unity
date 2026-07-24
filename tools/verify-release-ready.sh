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
if deps.get("com.bidscube.sdk") != "1.2.16":
    raise SystemExit(
        "ERROR: package.json must depend on com.bidscube.sdk 1.2.16 exactly "
        f"(got {deps.get('com.bidscube.sdk')!r})"
    )
for k in deps:
    if k == "com.bidscube.sdk":
        continue
    raise SystemExit(f"ERROR: unexpected package.json dependency {k!r} — keep only com.bidscube.sdk for this adapter")
print("package.json peer dependency OK: com.bidscube.sdk 1.2.16 only")
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

python3 << 'PY' || exit 1
import io
import re
import zipfile
from pathlib import Path

info = Path("Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs").read_text(encoding="utf-8")
m = re.search(r'BundledMaxAdapterAarVersion = "([^"]+)"', info)
if not m:
    raise SystemExit("ERROR: cannot parse BundledMaxAdapterAarVersion")

ver = m.group(1)
aar_path = Path(f"Runtime/Plugins/Android/applovin-bidscube-max-adapter-{ver}.aar")
if not aar_path.is_file():
    raise SystemExit(f"ERROR: missing {aar_path}")

with zipfile.ZipFile(aar_path) as aar:
    classes_jar = aar.read("classes.jar")

forbidden = [
    b"bidscube_test_signal",
    b"Bidscube Native Ad",
    b"Native ad from Bidscube",
    b"Learn More",
]

for item in forbidden:
    if item in classes_jar:
        raise SystemExit(f"ERROR: forbidden string found in {aar_path}: {item.decode(errors='ignore')}")

with zipfile.ZipFile(io.BytesIO(classes_jar)) as jar:
    classes = jar.namelist()

if any("MaxNativeAdAdapter" in c or "loadNativeAd" in c for c in classes):
    raise SystemExit("ERROR: bundled Android adapter must not expose Native MAX unless real native mapping is implemented.")

print("Android MAX adapter AAR forbidden-string check OK")
PY

python3 << 'PY' || exit 1
import io
import re
import shutil
import subprocess
import tempfile
import zipfile
from pathlib import Path

info = Path("Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs").read_text(encoding="utf-8")
native_ver = re.search(r'NativeAndroidBidscubeSdkVersion = "([^"]+)"', info).group(1)
adapter_ver = re.search(r'BundledMaxAdapterAarVersion = "([^"]+)"', info).group(1)

core_aar = Path(f"Runtime/Plugins/Android/bidscube-sdk-full-video-{native_ver}.aar")
adapter_aar = Path(f"Runtime/Plugins/Android/applovin-bidscube-max-adapter-{adapter_ver}.aar")
if not core_aar.is_file():
    raise SystemExit(f"ERROR: missing {core_aar}")
if not adapter_aar.is_file():
    raise SystemExit(f"ERROR: missing {adapter_aar}")

required = [
    "public static java.lang.String collectSignal()",
    "public static void clearPreloadCache()",
    "public static void setMediationAdapterVersion(java.lang.String)",
    "public static void initialize(android.content.Context, com.bidscube.sdk.config.SDKConfig, com.bidscube.sdk.interfaces.InitializationCallback)",
    "public static void preloadImageAd(java.lang.String, com.bidscube.sdk.interfaces.AdCallback)",
    "public static void preloadInterstitialVideoAd(java.lang.String, com.bidscube.sdk.interfaces.AdCallback)",
    "public static void preloadRewardedVideoAd(java.lang.String, com.bidscube.sdk.interfaces.AdCallback)",
    "public static void showInterstitialVideoAd(java.lang.String, com.bidscube.sdk.interfaces.AdCallback)",
    "public static void showRewardedVideoAd(java.lang.String, com.bidscube.sdk.interfaces.AdCallback)",
]

with tempfile.TemporaryDirectory() as td:
    td = Path(td)
    core_jar = td / "core.jar"
    with zipfile.ZipFile(core_aar) as z:
        core_jar.write_bytes(z.read("classes.jar"))

    if shutil.which("javap"):
        out = subprocess.check_output(
            ["javap", "-classpath", str(core_jar), "-p", "com.bidscube.sdk.BidscubeSDK"],
            text=True,
            stderr=subprocess.STDOUT,
        )
        missing = [m for m in required if m not in out]
        if missing:
            raise SystemExit(
                "ERROR: bundled core SDK AAR is not compatible with bundled AppLovin adapter AAR. Missing methods:\n"
                + "\n".join(" - " + m for m in missing)
            )
    else:
        sdk_class = zipfile.ZipFile(io.BytesIO(core_jar.read_bytes())).read("com/bidscube/sdk/BidscubeSDK.class")
        need_names = [
            "collectSignal", "clearPreloadCache", "setMediationAdapterVersion",
            "preloadImageAd", "preloadInterstitialVideoAd", "preloadRewardedVideoAd",
            "showInterstitialVideoAd", "showRewardedVideoAd",
        ]
        missing = [n for n in need_names if n.encode() not in sdk_class]
        if missing:
            raise SystemExit(
                "ERROR: javap unavailable; classfile fallback missing BidscubeSDK methods: "
                + ", ".join(missing)
            )

print("Android adapter/core method compatibility OK")
PY

python3 << 'PY' || exit 1
import io
import re
import zipfile
from pathlib import Path

info_text = Path("Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs").read_text(encoding="utf-8")

def parse_bool(name: str) -> bool:
    m = re.search(rf"public const bool {name} = (true|false)", info_text)
    if not m:
        raise SystemExit(f"ERROR: cannot parse {name} from AdapterPackageInfo.cs")
    return m.group(1) == "true"

android_supported = parse_bool("OpenRtb26AndroidResponseParsingSupported")
ios_supported = parse_bool("OpenRtb26IosResponseParsingSupported")

m = re.search(r'BundledMaxAdapterAarVersion = "([^"]+)"', info_text)
if not m:
    raise SystemExit("ERROR: cannot parse BundledMaxAdapterAarVersion")
adapter_ver = m.group(1)
aar_path = Path(f"Runtime/Plugins/Android/applovin-bidscube-max-adapter-{adapter_ver}.aar")
with zipfile.ZipFile(aar_path) as aar:
    classes_jar = aar.read("classes.jar")

# Adapter signal must not advertise OpenRTB 2.6 parsing unless we explicitly claim support.
if b"openrtb_2_6_response_parsing" in classes_jar:
    signals_true = (
        b'"openrtb_2_6_response_parsing",true' in classes_jar
        or b'"openrtb_2_6_response_parsing":true' in classes_jar
        or b"openrtb_2_6_response_parsing\", true" in classes_jar
    )
    if android_supported and not signals_true:
        raise SystemExit(
            "ERROR: AdapterPackageInfo claims Android OpenRTB 2.6 support but adapter AAR signal is false/missing"
        )
    if not android_supported and signals_true:
        raise SystemExit(
            "ERROR: bundled adapter AAR advertises openrtb_2_6_response_parsing=true "
            "but AdapterPackageInfo.OpenRtb26AndroidResponseParsingSupported is false"
        )
else:
    if android_supported:
        raise SystemExit(
            "ERROR: AdapterPackageInfo claims Android OpenRTB 2.6 support but adapter AAR has no openrtb signal"
        )

disclaimer = (
    "OpenRTB 2.6 support, when available, is provided by the native Bidscube SDKs used by the "
    "native AppLovin MAX adapters. The Unity package does not parse OpenRTB responses and does not "
    "build or POST OpenRTB bid requests."
)
docs = ["README.md", "Documentation~/INSTALL.md", "RELEASE.md", "CHANGELOG.md"]
for doc in docs:
    p = Path(doc)
    if not p.is_file():
        raise SystemExit(f"ERROR: missing {doc}")
    text = p.read_text(encoding="utf-8")
    if disclaimer not in text:
        raise SystemExit(f"ERROR: {doc} must include the OpenRTB Unity-delegation disclaimer")

forbidden_claims = [
    re.compile(r"OpenRTB 2\.6 supported(?!\s+by)", re.I),
    re.compile(r"OpenRTB 2\.6-style response parsing is supported(?!\s+on)", re.I),
    re.compile(r"full OpenRTB 2\.6 support", re.I),
]
if not android_supported and not ios_supported:
    required_option_c = "OpenRTB 2.6-style response parsing is not implemented yet"
    for doc in docs:
        text = Path(doc).read_text(encoding="utf-8")
        if required_option_c not in text:
            raise SystemExit(
                f"ERROR: {doc} must state Option C OpenRTB status when both platform flags are false"
            )
        for pat in forbidden_claims:
            if pat.search(text):
                raise SystemExit(
                    f"ERROR: {doc} contains ambiguous OpenRTB support claim: {pat.pattern}"
                )
elif android_supported and not ios_supported:
    for doc in docs:
        text = Path(doc).read_text(encoding="utf-8")
        if "Android:" not in text or "iOS:" not in text:
            raise SystemExit(f"ERROR: {doc} must document Android/iOS OpenRTB status separately (Option A)")
elif android_supported and ios_supported:
    required = "OpenRTB 2.6-style response parsing is supported on Android and iOS"
    for doc in docs:
        if required not in Path(doc).read_text(encoding="utf-8"):
            raise SystemExit(f"ERROR: {doc} must state Option B OpenRTB status")

print(
    "OpenRTB release status OK — Android:",
    "supported" if android_supported else "not implemented",
    "| iOS:",
    "supported" if ios_supported else "not implemented",
)
PY

if [[ ! -f "Runtime/BidscubeSDK/Mediation/AppLovinMaxUnityReflection.cs" ]]; then
  echo "ERROR: AppLovinMaxUnityReflection.cs is required" >&2
  exit 1
fi

IOS_POST="Editor/iOS/BidscubeIosPodfilePostprocessor.cs"
if [[ ! -f "$IOS_POST" ]]; then
  echo "ERROR: $IOS_POST is required for iOS BidscubeSDKAppLovin pod injection" >&2
  exit 1
fi
grep -q "AdapterPackageInfo.IosBidscubeAppLovinPodVersion" "$IOS_POST" || {
  echo "ERROR: $IOS_POST must use AdapterPackageInfo.IosBidscubeAppLovinPodVersion" >&2
  exit 1
}

if ! grep -q "EnableDirectSdkFallback { get; set; } = false" Runtime/BidscubeSDK/Mediation/AppLovinMaxRewardedBridge.cs; then
  echo "ERROR: AppLovinMaxRewardedBridge.EnableDirectSdkFallback must default to false" >&2
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
