# Ad request endpoint (Unity)

This document describes how the Unity SDK builds **HTTPS GET** requests to the Bidscube SSP. Behavior matches the Android reference implementation (`com.bidscube.sdk`).

## Configuration: `adRequestAuthority`

Use **`SDKConfig.Builder.AdRequestAuthority(string)`** (or the alias **`BaseURL(string)`**, same normalization) to set the **host and optional port only** — not a full URL with path and query.

- **Default** (if you do not call these): `ssp-bcc-ads.com` (same as Android `DeviceInfo.DEFAULT_AD_REQUEST_AUTHORITY`).
- **Do not** pass a complete URL with query parameters. The SDK always uses path **`/sdk`** and appends query parameters per ad type.
- **Accepted input** (after normalization): `host`, `host:port`, IPv6 `[addr]:port`, or a browser-style prefix such as `https://edge.example.com/sdk` (scheme and path are stripped).

### Normalization (parity with Android `SDKConfig.Builder.normalizeAdRequestAuthority`)

1. Trim; empty → default authority.
2. Up to **3** UTF-8 percent-decode passes while the string changes (e.g. `%3A` → `:`).
3. Strip leading `https://` or `http://` (case-insensitive).
4. Cut at the first `/` (path).
5. Cut at the first `?` (query).
6. Trim; empty → default authority.

### HTTPS base URL: `https://<authority>/sdk`

- Scheme is always **`https`**.
- Path is always **`/sdk`** (result: `https://host/sdk` or `https://host:port/sdk`).

**Port / IPv6:** Standard `Uri` builders may encode `:` inside the authority incorrectly and break TCP ports. The SDK uses the same rules as Android `SspAdUriHelper`:

- **`[IPv6]:port`** with port in `0`–`65535` → host + port without broken encoding.
- **`host:port`** when the suffix after the **last** `:` is 1–5 digits and parses to a valid port, and the host part does not contain `:` or `]` → split host and port.
- Otherwise the whole string is the host (e.g. IPv6 without port in brackets, or hostnames with no port).

`SDKConfig.BaseURL` returns this full base URL string (no trailing slash before `?`).

## Query parameters by ad type

All values are **percent-encoded** like `Uri.appendQueryParameter` on Android.

### Image (banner) — `ImageAdUrlBuilder`

`c=b`, `m=api`, `res=js`, `app=1`, plus:

`placementId`, `bundle`, `name`, `app_store_url`, `language`, `deviceWidth`, `deviceHeight`, `ua`, `ifa`, `dnt`.

(No GDPR/CCPA query params on image in the Android builder.)

### Video — `VideoAdUrlBuilder`

`c=v`, `m=xml`, `id`, `app=1`, `w` and `h` (screen dimensions from device info), then:

`bundle`, `name`, `app_version`, `ifa`, `dnt`, `app_store_url`, `ua`, `language`, `deviceWidth`, `deviceHeight`.

### Native — `NativeAdUrlBuilder`

`c=n`, `m=s`, `id`, `app=1`, then:

`bundle`, `name`, `app_version`, `ifa`, `dnt`, `app_store_url`, `ua`, `gdpr`, `gdpr_consent`, `us_privacy`, `ccpa`, `coppa`, `language`, `deviceWidth`, `deviceHeight`, `w`, `h` (logical ad size from `AdSizeSettings` for native, or default **1080×800** if unset).

String literals **`null`** are used for some consent fields when matching Android null-handling (`gdpr_consent`, `us_privacy` / `ccpa`).

## SSP response

- **Method:** GET  
- **Body:** JSON, UTF-8  
- **Fields used by the SDK:** `adm` (string), `position` (int) — same idea as Android `BidscubeResponseParser`.

## AppLovin MAX / mediation

On Android the adapter may read `request_authority` or `ssp_host` from server parameters and pass them into `adRequestAuthority`. Publishers should supply **host**, **`host:port`**, or a short **https://…** prefix **without** query — same normalization as above.

## Testing

- **Editor menu:** `Bidscube → Validate SSP URL parity (console)` runs quick checks (normalization, `127.0.0.1:8787` port not `%3A`-encoded).
- **Samples:** optional inspector field **Ad Request Authority**; in Editor, environment variable **`BIDSCUBE_SSP_AUTHORITY`** overrides (useful with a local SSP or HTTPS tunnel).

## Reference (Android)

- `sdk/.../config/SDKConfig.java` — normalization  
- `sdk/.../network/SspAdUriHelper.java` — HTTPS + `/sdk`  
- `sdk/.../network/ImageAdUrlBuilder.java`, `VideoAdUrlBuilder.java`, `NativeAdUrlBuilder.java` — query layout  
