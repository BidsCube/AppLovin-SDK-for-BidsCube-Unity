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
echo "NativeAndroidBidscubeSdkVersion: $NATIVE_VER (bundled lite AAR filename)"
echo "BundledMaxAdapterAarVersion: $MAXA_VER"
echo ""

shopt -s nullglob
BIDSCUBE_ADAPTER_AARS=(Runtime/Plugins/Android/applovin-bidscube-max-adapter-*.aar)
if ((${#BIDSCUBE_ADAPTER_AARS[@]} == 0)); then
  echo "ERROR: expected bundled MAX adapter AAR at Runtime/Plugins/Android/applovin-bidscube-max-adapter-*.aar" >&2
  exit 1
fi
echo "Bundled MAX adapter AAR: ${BIDSCUBE_ADAPTER_AARS[*]}"

LITE_AAR="Runtime/Plugins/Android/bidscube-sdk-lite-${NATIVE_VER}.aar"
if [[ ! -f "$LITE_AAR" ]]; then
  echo "ERROR: expected bundled lite core AAR at $LITE_AAR" >&2
  exit 1
fi
echo "Bundled lite core AAR: $LITE_AAR"
FULL_AAR="Runtime/Plugins/Android/bidscube-sdk-${NATIVE_VER}.aar"
if [[ -f "$FULL_AAR" ]]; then
  echo "Optional full core AAR (offline FullWithVideo): $FULL_AAR"
else
  echo "Optional full core AAR not present ($FULL_AAR) — FullWithVideo Gradle uses Maven com.bidscube:bidscube-sdk:${NATIVE_VER}@aar"
fi

# Filename on disk must match our declared MAX adapter version.
if [[ ! -f "Runtime/Plugins/Android/applovin-bidscube-max-adapter-${MAXA_VER}.aar" ]]; then
  echo "ERROR: MAX adapter AAR on disk must match BundledMaxAdapterAarVersion ($MAXA_VER)" >&2
  exit 1
fi

if [[ ! -f "Runtime/BidscubeSDK/Mediation/AppLovinMaxUnityReflection.cs" ]]; then
  echo "ERROR: AppLovinMaxUnityReflection.cs is required" >&2
  exit 1
fi

echo "Suggested git tag (must match package.json): v$VER"
echo "  git tag -a \"v$VER\" -m \"com.bidscube.applovin.max $VER\""
echo "  git push origin \"v$VER\""
echo ""
echo "GitHub Actions will produce: Bidscube-SDK-Unity-${VER}.zip"
echo "Release title pattern: com.bidscube.applovin.max ${VER}"
echo ""
echo "Optional: ensure CHANGELOG.md documents this version before tagging."
