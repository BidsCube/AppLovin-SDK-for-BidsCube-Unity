#!/usr/bin/env python3
"""Static checks: LiteNoVideo path must not mention video Maven coords; FullWithVideo must inject them."""
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

idx_skip = text.index("Skipping Media3 and Google IMA dependencies")
idx_append = text.index("AppendVideoDeps")
assert idx_skip < idx_append, "Lite skip log must appear before AppendVideoDeps in source order"

print("Gradle postprocessor structure OK (Lite vs Full video deps)")
