# Release checklist — `com.bidscube.applovin.max`

Run [`tools/verify-release-ready.sh`](tools/verify-release-ready.sh) before tagging.

- [ ] `package.json` **name** is `com.bidscube.applovin.max` (not `com.bidscube.sdk`).
- [ ] `package.json` **version** is **1.0.13** and matches `AdapterPackageInfo.UpmVersion` and [`CHANGELOG.md`](CHANGELOG.md).
- [ ] `package.json` **dependencies** include only **`com.bidscube.sdk` `1.2.5`** (no official AppLovin MAX UPM id).
- [ ] **Official AppLovin MAX Unity SDK** is documented as **external** to this package.
- [ ] **Lite** core AAR present: `Runtime/Plugins/Android/bidscube-sdk-lite-1.2.3.aar` (name matches `AdapterPackageInfo.NativeAndroidBidscubeSdkVersion`).
- [ ] **Full** core AAR: either committed as `bidscube-sdk-1.2.3.aar` **or** document that **FullWithVideo** falls back to Maven `com.bidscube:bidscube-sdk:1.2.3@aar` (offline teams should vendor the full AAR).
- [ ] **MAX adapter** AAR: `applovin-bidscube-max-adapter-1.0.4.aar` matches `BundledMaxAdapterAarVersion`.
- [ ] **LiteNoVideo** Gradle: **no** `androidx.media3` and **no** `interactivemedia` in the managed block (validate with release script / CI).
- [ ] **FullWithVideo** Gradle: **Media3** + **Google IMA** lines present in postprocessor source / generated smoke test.
- [ ] **Only one** Bidscube core AAR variant is on the Android classpath (no lite + full together).
- [ ] No **duplicate class** / DEX merge issues from double core lines.
- [ ] **Unity-Test-App** (or client project) builds **LiteNoVideo** (default).
- [ ] **Unity-Test-App** builds **FullWithVideo** with video/rewarded paths.
- [ ] **Mediation Debugger** opens (official MAX plugin present).
- [ ] **Banner** load/show works.
- [ ] **Rewarded / video** works only in **FullWithVideo**; **LiteNoVideo** shows the documented error / callback failure, not a crash.

Artifacts: `tools/build-release-lite-no-player.sh` → `com.bidscube.applovin.max-1.0.13-lite-no-player.zip`, `tools/build-release-full-with-player.sh` → `com.bidscube.applovin.max-1.0.13-full-with-player.zip`.
