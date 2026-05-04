#!/usr/bin/env bash
# Packages the UPM tree for integrators defaulting to LiteNoVideo (same sources as full ZIP).
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

PKG_NAME="$(python3 -c "import json; print(json.load(open('package.json'))['name'])")"
VER="$(python3 -c "import json; print(json.load(open('package.json'))['version'])")"
if [[ "$PKG_NAME" != "com.bidscube.applovin.max" ]]; then
  echo "ERROR: package.json name must be com.bidscube.applovin.max (got $PKG_NAME)" >&2
  exit 1
fi

NATIVE_VER="$(sed -n 's/.*public const string NativeAndroidBidscubeSdkVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs | head -n1)"
LITE_AAR="Runtime/Plugins/Android/bidscube-sdk-lite-${NATIVE_VER}.aar"
ADAPTER_VER="$(sed -n 's/.*public const string BundledMaxAdapterAarVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs | head -n1)"
ADAPTER_AAR="Runtime/Plugins/Android/applovin-bidscube-max-adapter-${ADAPTER_VER}.aar"

[[ -f "$LITE_AAR" ]] || { echo "ERROR: missing $LITE_AAR" >&2; exit 1; }
[[ -f "$ADAPTER_AAR" ]] || { echo "ERROR: missing $ADAPTER_AAR" >&2; exit 1; }

# Lite variant must not be merged twice by Unity — PluginImporter Android disabled on lite core AAR.
if grep -q "Android: Android" "$LITE_AAR.meta" && grep -A2 "Android: Android" "$LITE_AAR.meta" | grep -q "enabled: 1"; then
  echo "ERROR: lite core AAR must have PluginImporter Android disabled (enabled: 0)" >&2
  exit 1
fi

OUT="com.bidscube.applovin.max-${VER}-lite-no-player.zip"
rm -f "$OUT"
zip -r -q "$OUT" . \
  -x "*.git/*" -x "*__MACOSX*" -x "*.DS_Store" -x "._*" -x "*.apk" -x "*.aab" -x "*.ipa"
echo "Created $OUT"
