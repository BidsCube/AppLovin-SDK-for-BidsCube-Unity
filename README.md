# Bidscube AppLovin MAX adapter (`com.bidscube.applovin.max`)

**Версія UPM:** **1.0.20** · тег **`v1.0.20`** · peer **`com.bidscube.sdk` [1.2.9](https://github.com/BidsCube/bidscube-sdk-unity)**.

Це **додатковий** пакет до ядра Bidscube: MAX-адаптер (AAR), lite/full core AAR для Android і міст до **`MaxSdk`**. Ядро C# — у **`com.bidscube.sdk`**.

## Встановлення

Повна покрокова інструкція (manifest, AppLovin MAX SDK, Android/iOS, MAX dashboard): **[`Documentation~/INSTALL.md`](Documentation~/INSTALL.md)**.

Коротко — додайте в **`Packages/manifest.json`**:

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.9",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.20"
  }
}
```

Окремо встановіть **офіційний AppLovin MAX Unity SDK** (UPM або `.unitypackage` з сайту AppLovin) — без нього **`MaxSdk`** недоступний.

## Після встановлення

- **Android:** за замовчуванням **LiteNoVideo** (без Media3/IMA у Gradle). Режим **FullWithVideo** — **Tools → Bidscube SDK → Android Build Features** або ScriptableObject **Android Export Settings** (деталі в `INSTALL.md`).
- **Медіація MAX:** у режимі **`BidscubeIntegrationMode.AppLovinMaxMediation`** рекламу вантажте лише через MAX, не через C# `ShowVideoAd` / банерні API ядра — див. `INSTALL.md`.

## Інше

- Зміни по версіях: [`CHANGELOG.md`](CHANGELOG.md)  
- Реліз для мейнтейнерів: [`RELEASE.md`](RELEASE.md)  
- Ліцензія: [`LICENSE.md`](LICENSE.md)  
- Демо: у **Package Manager** → **Samples** → **SDK Demo** (потребує **`com.bidscube.sdk`** та за потреби **UGUI** / **TMP** у хості).
