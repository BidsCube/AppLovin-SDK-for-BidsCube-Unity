#!/usr/bin/env bash
# Rebuild applovin-adapter AAR and all four core SDK AARs from bidscube-sdk-android and copy into this Unity package.
# Usage: from repo root: ./tools/build-android-max-adapter-into-package.sh [path-to-bidscube-sdk-android]
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ANDROID_ROOT="${1:-$ROOT/../bidscube-sdk-android}"
ADAPTER_DEST="$ROOT/Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.2.6.aar"
CORE_DEST="$ROOT/Runtime/Plugins/Android"

if [[ ! -f "$ANDROID_ROOT/gradlew" ]]; then
  echo "ERROR: Android repo not found at: $ANDROID_ROOT" >&2
  echo "Pass the path to bidscube-sdk-android as the first argument." >&2
  exit 1
fi

(cd "$ANDROID_ROOT" && ./gradlew :sdk:stageReleaseAars :applovin-adapter:assembleRelease -PskipSigning=true)
for f in bidscube-sdk-lite-no-video-1.2.5.aar bidscube-sdk-webview-video-1.2.5.aar \
  bidscube-sdk-legacy-media-video-1.2.5.aar bidscube-sdk-full-video-1.2.5.aar; do
  cp "$ANDROID_ROOT/sdk/build/staged-aars/$f" "$CORE_DEST/$f"
  echo "Wrote $CORE_DEST/$f"
done
cp "$ANDROID_ROOT/applovin-adapter/build/outputs/aar/applovin-adapter-release.aar" "$ADAPTER_DEST"
echo "Wrote $ADAPTER_DEST"
