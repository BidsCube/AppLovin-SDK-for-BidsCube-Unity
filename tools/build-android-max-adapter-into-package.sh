#!/usr/bin/env bash
# Rebuild applovin-adapter AAR from bidscube-sdk-android and copy into this Unity package.
# Usage: from repo root: ./tools/build-android-max-adapter-into-package.sh [path-to-bidscube-sdk-android]
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ANDROID_ROOT="${1:-$ROOT/../../../../bidscube-sdk-android}"
DEST="$ROOT/Runtime/Plugins/Android/applovin-bidscube-max-adapter-1.0.3.aar"

if [[ ! -f "$ANDROID_ROOT/gradlew" ]]; then
  echo "ERROR: Android repo not found at: $ANDROID_ROOT" >&2
  echo "Pass the path to bidscube-sdk-android as the first argument." >&2
  exit 1
fi

(cd "$ANDROID_ROOT" && ./gradlew :applovin-adapter:assembleRelease)
cp "$ANDROID_ROOT/applovin-adapter/build/outputs/aar/applovin-adapter-release.aar" "$DEST"
echo "Wrote $DEST"
