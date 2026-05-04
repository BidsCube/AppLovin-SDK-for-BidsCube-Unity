#!/usr/bin/env bash
# Same UPM sources as lite ZIP; validates FullWithVideo prerequisites (full core artifact + Gradle video deps).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

PKG_NAME="$(python3 -c "import json; print(json.load(open('package.json'))['name'])")"
VER="$(python3 -c "import json; print(json.load(open('package.json'))['version'])")"
if [[ "$PKG_NAME" != "com.bidscube.applovin.max" ]]; then
  echo "ERROR: package.json name must be com.bidscube.applovin.max (got $PKG_NAME)" >&2
  exit 1
fi

POST="Editor/Android/BidscubeAndroidGradlePostprocessor.cs"
grep -q "interactivemedia" "$POST" || { echo "ERROR: postprocessor must inject Google IMA for FullWithVideo" >&2; exit 1; }
grep -q "media3-common" "$POST" || { echo "ERROR: postprocessor must inject Media3 for FullWithVideo" >&2; exit 1; }
grep -q "Including video player dependencies for FullWithVideo" "$POST" || { echo "ERROR: missing FullWithVideo log line" >&2; exit 1; }

NATIVE_VER="$(sed -n 's/.*public const string NativeAndroidBidscubeSdkVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs | head -n1)"
FULL_AAR="Runtime/Plugins/Android/bidscube-sdk-${NATIVE_VER}.aar"
ADAPTER_VER="$(sed -n 's/.*public const string BundledMaxAdapterAarVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs | head -n1)"
ADAPTER_AAR="Runtime/Plugins/Android/applovin-bidscube-max-adapter-${ADAPTER_VER}.aar"

[[ -f "$ADAPTER_AAR" ]] || { echo "ERROR: missing $ADAPTER_AAR" >&2; exit 1; }

if [[ ! -f "$FULL_AAR" ]]; then
  echo "WARNING: $FULL_AAR not in repo — FullWithVideo still works via Maven com.bidscube:bidscube-sdk:${NATIVE_VER}@aar when repos are available." >&2
  echo "For an offline / reproducible full ZIP, add the full AAR from the native Android SDK build output." >&2
fi

OUT="com.bidscube.applovin.max-${VER}-full-with-player.zip"
rm -f "$OUT"
zip -r -q "$OUT" . \
  -x "*.git/*" -x "*__MACOSX*" -x "*.DS_Store" -x "._*" -x "*.apk" -x "*.aab" -x "*.ipa"
echo "Created $OUT"
