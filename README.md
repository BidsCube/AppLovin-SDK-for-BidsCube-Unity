# Bidscube AppLovin MAX adapter (`com.bidscube.applovin.max`)

**UPM:** `com.bidscube.applovin.max` **1.0.18** · тег **`v1.0.18`** · peer **`com.bidscube.sdk` [1.2.8](https://github.com/BidsCube/bidscube-sdk-unity)**.

Цей пакет додає **Android MAX adapter AAR**, **core Bidscube AAR** (lite за замовчуванням) і **`AppLovinMaxUnityReflection`**. Повний C# SDK — у **`com.bidscube.sdk`**.

## Встановлення

Кроки, `manifest.json`, Android/iOS, MAX dashboard — у **[`Documentation~/INSTALL.md`](Documentation~/INSTALL.md)**.

Коротко:

1. Додай у **`Packages/manifest.json`** (заміни URL на свої репо, теги мають збігатися з релізами):

```json
{
  "dependencies": {
    "com.bidscube.sdk": "https://github.com/BidsCube/bidscube-sdk-unity.git#v1.2.8",
    "com.bidscube.applovin.max": "https://github.com/BidsCube/AppLovin-SDK-for-BidsCube-Unity.git#v1.0.18"
  }
}
```

2. Окремо встанови **офіційний AppLovin MAX Unity SDK** (UPM або `.unitypackage` з сайту AppLovin). Без **`MaxSdk`** адаптер не зможе ініціалізувати MAX.

3. Для **ugui / TMP** дивись залежності хост-проєкту або **`com.bidscube.sdk`**.

Після встановлення — **ініціалізація**, режими **LiteNoVideo / FullWithVideo**, мінімум по **MAX mediation** — у **`Documentation~/INSTALL.md`**.

## Інше

- Зміни по версіях: [`CHANGELOG.md`](CHANGELOG.md)  
- Реліз для мейнтейнерів: [`RELEASE.md`](RELEASE.md)  
- Ліцензія: [`LICENSE.md`](LICENSE.md)
