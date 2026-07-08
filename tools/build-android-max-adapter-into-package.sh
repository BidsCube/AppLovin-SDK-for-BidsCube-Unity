#!/usr/bin/env bash
# Rebuild applovin-adapter AAR and all four core SDK AARs from AppLovin-SDK-for-BidsCube-Android and copy into this Unity package.
# Usage: from repo root: ./tools/build-android-max-adapter-into-package.sh [path-to-AppLovin-SDK-for-BidsCube-Android]
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ANDROID_ROOT="${1:-$ROOT/../AppLovin-SDK-for-BidsCube-Android}"
ADAPTER_VER="$(sed -n 's/.*public const string BundledMaxAdapterAarVersion = "\([^"]*\)".*/\1/p' "$ROOT/Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs" | head -n1)"
NATIVE_VER="$(sed -n 's/.*public const string NativeAndroidBidscubeSdkVersion = "\([^"]*\)".*/\1/p' "$ROOT/Runtime/BidscubeSDK/Properties/AdapterPackageInfo.cs" | head -n1)"
if [[ -z "$ADAPTER_VER" ]]; then
  echo "ERROR: could not parse BundledMaxAdapterAarVersion from AdapterPackageInfo.cs" >&2
  exit 1
fi
if [[ -z "$NATIVE_VER" ]]; then
  echo "ERROR: could not parse NativeAndroidBidscubeSdkVersion from AdapterPackageInfo.cs" >&2
  exit 1
fi
ADAPTER_DEST="$ROOT/Runtime/Plugins/Android/applovin-bidscube-max-adapter-${ADAPTER_VER}.aar"
CORE_DEST="$ROOT/Runtime/Plugins/Android"

if [[ ! -f "$ANDROID_ROOT/gradlew" ]]; then
  echo "ERROR: Android repo not found at: $ANDROID_ROOT" >&2
  echo "Pass the path to AppLovin-SDK-for-BidsCube-Android as the first argument." >&2
  exit 1
fi

(cd "$ANDROID_ROOT" && ./gradlew :sdk:stageReleaseAars :applovin-adapter:stageReleaseAars -PskipSigning=true)

STAGED_SDK_DIR="$ANDROID_ROOT/sdk/build/staged-aars"
if [[ ! -d "$STAGED_SDK_DIR" ]]; then
  echo "ERROR: missing staged core AAR directory: $STAGED_SDK_DIR" >&2
  exit 1
fi

for f in "bidscube-sdk-lite-no-video-${NATIVE_VER}.aar" "bidscube-sdk-webview-video-${NATIVE_VER}.aar" \
  "bidscube-sdk-legacy-media-video-${NATIVE_VER}.aar" "bidscube-sdk-full-video-${NATIVE_VER}.aar"; do
  src="$STAGED_SDK_DIR/$f"
  if [[ ! -f "$src" ]]; then
    echo "ERROR: Android repo did not stage $f — expected core SDK version $NATIVE_VER to match AdapterPackageInfo" >&2
    echo "Staged core AARs:" >&2
    ls -1 "$STAGED_SDK_DIR"/bidscube-sdk-*.aar 2>/dev/null || true
    exit 1
  fi
  cp "$src" "$CORE_DEST/$f"
  echo "Wrote $CORE_DEST/$f"
done

# Adapter bytecode is identical across flavors; copy full-video staged artifact to the bundled generic filename.
SRC_ADAPTER="$ANDROID_ROOT/applovin-adapter/build/staged-aars/applovin-bidscube-max-adapter-full-video-${ADAPTER_VER}.aar"
if [[ ! -f "$SRC_ADAPTER" ]]; then
  echo "ERROR: missing staged adapter AAR at $SRC_ADAPTER (adapter version $ADAPTER_VER)" >&2
  ls -1 "$ANDROID_ROOT/applovin-adapter/build/staged-aars/" 2>/dev/null || true
  exit 1
fi
cp "$SRC_ADAPTER" "$ADAPTER_DEST"
echo "Wrote $ADAPTER_DEST"

# Remove stale AARs (core + adapter) with version suffixes that no longer match AdapterPackageInfo.
shopt -s nullglob
for stale in "$CORE_DEST"/bidscube-sdk-*-*.aar "$CORE_DEST"/applovin-bidscube-max-adapter-*.aar; do
  base="$(basename "$stale")"
  keep_core=(
    "bidscube-sdk-lite-no-video-${NATIVE_VER}.aar"
    "bidscube-sdk-webview-video-${NATIVE_VER}.aar"
    "bidscube-sdk-legacy-media-video-${NATIVE_VER}.aar"
    "bidscube-sdk-full-video-${NATIVE_VER}.aar"
  )
  keep_adapter="applovin-bidscube-max-adapter-${ADAPTER_VER}.aar"
  skip=false
  if [[ "$base" == "$keep_adapter" ]]; then skip=true; fi
  for k in "${keep_core[@]}"; do
    if [[ "$base" == "$k" ]]; then skip=true; break; fi
  done
  if [[ "$skip" == true ]]; then continue; fi
  rm -f "$stale" "${stale}.meta"
  echo "Removed stale $stale"
done

echo "Done: core SDK ${NATIVE_VER} + MAX adapter ${ADAPTER_VER}"
