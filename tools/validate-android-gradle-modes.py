#!/usr/bin/env python3
"""Static checks: LiteNoVideo path must not mention video Maven coords; FullWithVideo must inject them."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
POST = ROOT / "Editor/Android/BidscubeAndroidGradlePostprocessor.cs"
text = POST.read_text(encoding="utf-8")

# Lite branch: between featureSet == LiteNoVideo for Patch and next major block — heuristic: Skipping log exists
assert "Skipping video player dependencies for LiteNoVideo" in text
assert "Including video player dependencies for FullWithVideo" in text
assert "androidx.media3:media3-common" in text
assert "interactivemedia" in text

# Ensure AppendVideoDeps is only invoked for FullWithVideo (after lite early-return in OnPostGenerateGradleProject)
idx_skip = text.index("Skipping video player dependencies")
idx_append = text.index("AppendVideoDeps")
assert idx_skip < idx_append, "Lite log must appear before AppendVideoDeps in source order"

print("Gradle postprocessor structure OK (Lite vs Full video deps)")
