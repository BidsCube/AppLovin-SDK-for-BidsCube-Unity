#!/usr/bin/env python3
"""Static checks: shared Gradle patcher (sibling monorepo) + thin AppLovin postprocessor."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
POST = ROOT / "Editor/Android/BidscubeAndroidGradlePostprocessor.cs"
text = POST.read_text(encoding="utf-8")
assert "BidscubeAndroidGradleProjectPatcher" in text
assert "appendAppLovinSdkDependency: true" in text

PATCHER = ROOT.parent / "bidscube-sdk-unity" / "Editor/Android/BidscubeAndroidGradleProjectPatcher.cs"
if not PATCHER.is_file():
    print("validate-android-gradle-modes: skip (no sibling bidscube-sdk-unity patcher)")
else:
    pt = PATCHER.read_text(encoding="utf-8")
    assert "Skipping Media3 and Google IMA dependencies" in pt
    assert "Including Media3 and Google IMA dependencies" in pt
    assert "Android feature set: LiteNoVideo" in pt
    assert "Android feature set: FullWithVideo" in pt
    assert "androidx.media3:media3-common" in pt
    assert "interactivemedia" in pt
    assert "useBundledLiteAar" in pt
    assert "sdk-full-video" in pt
    assert "ApplyDesugaringPolicyLite" in pt
    idx_skip = pt.index("Skipping Media3 and Google IMA dependencies")
    idx_append = pt.index("static void AppendVideoDeps")
    assert idx_skip < idx_append, "Lite skip log must appear before AppendVideoDeps in source order"
    lite_block = pt.split("else if (useBundledLiteAar)", 1)[1].split("else if (fullCoreFromMaven)", 1)[0]
    assert "liteFileName" in lite_block
    assert "com.bidscube:bidscube-sdk" not in lite_block, "LiteNoVideo bundled path must not inject Maven bidscube-sdk"
    print("Gradle postprocessor structure OK (Lite vs Full video deps)")
