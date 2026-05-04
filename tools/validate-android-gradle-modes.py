#!/usr/bin/env python3
"""Static checks: Lite bundled path has no Maven bidscube core; Full includes Media3/IMA."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
POST = ROOT / "Editor/Android/BidscubeAndroidGradlePostprocessor.cs"
text = POST.read_text(encoding="utf-8")

assert "Skipping Media3 and Google IMA dependencies" in text
assert "Including Media3 and Google IMA dependencies" in text
assert "Android feature set: LiteNoVideo" in text
assert "Android feature set: FullWithVideo" in text
assert "androidx.media3:media3-common" in text
assert "interactivemedia" in text
assert "useBundledLiteAar" in text

idx_skip = text.index("Skipping Media3 and Google IMA dependencies")
idx_append = text.index("AppendVideoDeps")
assert idx_skip < idx_append, "Lite skip log must appear before AppendVideoDeps in source order"

# Lite bundled branch must only reference lite AAR filename, not Maven coordinate for core SDK
lite_block = text.split("else if (useBundledLiteAar)", 1)[1].split("else if (fullCoreFromMaven)", 1)[0]
assert "NativeAndroidBundledCoreAarLiteFileName" in lite_block
assert "com.bidscube:bidscube-sdk" not in lite_block, "LiteNoVideo bundled path must not inject Maven bidscube-sdk"

print("Gradle postprocessor structure OK (Lite vs Full video deps)")
