#!/usr/bin/env bash
# Run from repo root before tagging. Does not create tags.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

VER="$(python3 -c "import json; print(json.load(open('package.json'))['version'])")"
echo "== Bidscube Unity UPM release check =="
echo "package.json version: $VER"
echo ""

# Unity UPM: MAJOR.MINOR.PATCH (three numeric parts); optional prerelease (-suffix). Four-part versions like 1.0.3.1 are rejected.
if [[ ! "$VER" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  echo "ERROR: version must be MAJOR.MINOR.PATCH with optional prerelease (e.g. 1.0.6 or 1.0.0-rc.1). Not 1.0.3.1 — Unity rejects it." >&2
  exit 1
fi

if ! grep -q "public const string SdkVersion" Runtime/BidscubeSDK/Core/Constants.cs; then
  echo "ERROR: Constants.SdkVersion not found" >&2
  exit 1
fi

SDK_CS_VER="$(sed -n 's/.*public const string SdkVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Core/Constants.cs | head -n1)"
if [[ -z "$SDK_CS_VER" ]]; then
  echo "ERROR: could not parse SdkVersion from Constants.cs" >&2
  exit 1
fi
if [[ "$SDK_CS_VER" != "$VER" ]]; then
  echo "ERROR: Constants.SdkVersion is \"$SDK_CS_VER\" but package.json version is \"$VER\"" >&2
  exit 1
fi

if ! grep -q "BidscubeAppLovinPodVersion" Editor/iOS/BidscubeIosPodfilePostprocessor.cs; then
  echo "ERROR: BidscubeIosPodfilePostprocessor.BidscubeAppLovinPodVersion not found" >&2
  exit 1
fi
POD_VER="$(sed -n 's/.*public const string BidscubeAppLovinPodVersion = "\([^"]*\)".*/\1/p' Editor/iOS/BidscubeIosPodfilePostprocessor.cs | head -n1)"
if [[ -z "$POD_VER" ]]; then
  echo "ERROR: could not parse BidscubeAppLovinPodVersion from BidscubeIosPodfilePostprocessor.cs" >&2
  exit 1
fi
echo "Constants.SdkVersion: $SDK_CS_VER (matches package.json)"
echo "BidscubeAppLovinPodVersion: $POD_VER (CocoaPods BidscubeSDKAppLovin; may differ from UPM for Unity-only patches)"
echo ""

shopt -s nullglob
BIDSCUBE_ADAPTER_AARS=(Runtime/Plugins/Android/applovin-bidscube-max-adapter-*.aar)
if ((${#BIDSCUBE_ADAPTER_AARS[@]} == 0)); then
  echo "ERROR: expected bundled MAX adapter AAR at Runtime/Plugins/Android/applovin-bidscube-max-adapter-*.aar" >&2
  exit 1
fi
echo "Bundled MAX adapter AAR: ${BIDSCUBE_ADAPTER_AARS[*]}"

if ! grep -q "NativeAndroidBidscubeSdkVersion" Runtime/BidscubeSDK/Core/Constants.cs; then
  echo "ERROR: Constants.NativeAndroidBidscubeSdkVersion not found" >&2
  exit 1
fi
NATIVE_SDK_VER="$(sed -n 's/.*public const string NativeAndroidBidscubeSdkVersion = "\([^"]*\)".*/\1/p' Runtime/BidscubeSDK/Core/Constants.cs | head -n1)"
if [[ -z "$NATIVE_SDK_VER" ]]; then
  echo "ERROR: could not parse NativeAndroidBidscubeSdkVersion from Constants.cs" >&2
  exit 1
fi
if ! grep -q "BidscubeAndroidCoreDependencyMode" Editor/Android/BidscubeAndroidGradlePostprocessor.cs; then
  echo "ERROR: BidscubeAndroidGradlePostprocessor must define BidscubeAndroidCoreDependencyMode (core SDK resolution)" >&2
  exit 1
fi
if ! grep -q "com.bidscube:bidscube-sdk:" Editor/Android/BidscubeAndroidGradlePostprocessor.cs; then
  echo "ERROR: BidscubeAndroidGradlePostprocessor must still define default Maven coordinate com.bidscube:bidscube-sdk (MavenBidscubeSdkAar mode)" >&2
  exit 1
fi
if ! grep -qF 'bidscube-sdk:{Constants.NativeAndroidBidscubeSdkVersion}@aar' Editor/Android/BidscubeAndroidGradlePostprocessor.cs; then
  echo "ERROR: BidscubeAndroidGradlePostprocessor must inject bidscube-sdk with @aar in default mode (AAR artifact, not POM-only resolution)" >&2
  exit 1
fi
if ! grep -q "Constants.NativeAndroidBidscubeSdkVersion" Editor/Android/BidscubeAndroidGradlePostprocessor.cs; then
  echo "ERROR: Gradle postprocessor should reference Constants.NativeAndroidBidscubeSdkVersion for the default Maven coordinate" >&2
  exit 1
fi
echo "Constants.NativeAndroidBidscubeSdkVersion: $NATIVE_SDK_VER (default Gradle: com.bidscube:bidscube-sdk … @aar; optional CoreDependencyMode in postprocessor)"

echo "Suggested git tag (must match package.json): v$VER"
echo "  git tag -a \"v$VER\" -m \"com.bidscube.sdk $VER\""
echo "  git push origin \"v$VER\""
echo ""
echo "GitHub Actions will produce: Bidscube-SDK-Unity-${VER}.zip"
echo "Release title pattern: com.bidscube.sdk ${VER}"
echo ""
echo "Optional: ensure CHANGELOG.md documents this version before tagging."
