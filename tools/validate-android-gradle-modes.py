#!/usr/bin/env python3
"""Static checks: shared Gradle patcher (sibling monorepo) + thin AppLovin postprocessor."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
POST = ROOT / "Editor/Android/BidscubeAndroidGradlePostprocessor.cs"
text = POST.read_text(encoding="utf-8")
assert "BidscubeAndroidGradleProjectPatcher" in text
assert "appendAppLovinSdkDependency: true" in text
assert "NativeAndroidBundledCoreAarWebViewVideoFileName" in text
assert "NativeAndroidBundledCoreAarLegacyMediaVideoFileName" in text

PATCHER = ROOT.parent / "bidscube-sdk-unity" / "Editor/Android/BidscubeAndroidGradleProjectPatcher.cs"
if not PATCHER.is_file():
    print("validate-android-gradle-modes: skip (no sibling bidscube-sdk-unity patcher)")
else:
    pt = PATCHER.read_text(encoding="utf-8")
    assert "Skipping Media3 and Google IMA dependencies" in pt
    assert "Including Media3 and Google IMA dependencies" in pt
    assert "DescribeFeatureSet" in pt
    assert '"LiteNoVideo"' in pt
    assert '"WebViewVideoNoDesugar"' in pt
    assert '"LegacyMediaVideoNoDesugar"' in pt
    assert '"FullWithVideo"' in pt
    assert "androidx.media3:media3-common" in pt
    assert "interactivemedia" in pt
    assert "sdk-webview-video" in pt
    assert "sdk-legacy-media-video" in pt
    assert "sdk-full-video" in pt
    assert "ApplyDesugaringPolicyLite" in pt
    assert "RequiresFullVideoDeps" in pt
    assert "GetSelectedBundledFileName" in pt
    idx_skip = pt.index("Skipping Media3 and Google IMA dependencies")
    idx_append = pt.index("static void AppendVideoDeps")
    assert idx_skip < idx_append, "Lite skip log must appear before AppendVideoDeps in source order"
    assert "if (fs == BidscubeAndroidFeatureSet.FullWithVideo)" in pt
    assert "else\n                    ApplyDesugaringPolicyLite" in pt
    print("Gradle postprocessor structure OK (Lite/WebView/Legacy/Full video deps)")
