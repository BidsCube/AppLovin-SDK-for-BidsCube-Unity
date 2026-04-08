#!/usr/bin/env bash
# Run from repo root before tagging. Does not create tags.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

VER="$(python3 -c "import json; print(json.load(open('package.json'))['version'])")"
echo "== Bidscube Unity UPM release check =="
echo "package.json version: $VER"
echo ""

if [[ ! "$VER" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  echo "ERROR: version must look like semver (e.g. 1.0.0)" >&2
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
if [[ "$POD_VER" != "$VER" ]]; then
  echo "ERROR: BidscubeAppLovinPodVersion is \"$POD_VER\" but package.json version is \"$VER\" (keep iOS pod pin in sync with UPM)" >&2
  exit 1
fi

echo "Constants.SdkVersion: $SDK_CS_VER (matches package.json)"
echo "BidscubeAppLovinPodVersion: $POD_VER (matches package.json)"
echo ""

echo "Suggested git tag (must match package.json): v$VER"
echo "  git tag -a \"v$VER\" -m \"com.bidscube.sdk $VER\""
echo "  git push origin \"v$VER\""
echo ""
echo "GitHub Actions will produce: Bidscube-SDK-Unity-${VER}.zip"
echo "Release title pattern: com.bidscube.sdk ${VER}"
echo ""
echo "Optional: ensure CHANGELOG.md documents this version before tagging."
