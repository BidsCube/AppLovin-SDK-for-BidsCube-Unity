using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BidscubeSDK;
using BidscubeSDK.Mediation;

namespace BidscubeSDK.Controllers
{
    /// <summary>
    /// Comprehensive test scene for Bidscube Unity SDK
    /// Demonstrates all SDK functionality with proper UI hierarchy
    /// </summary>
    public class BidscubeExampleScene : MonoBehaviour, IAdCallback, IConsentCallback, IAdRenderOverride
    {
        [Header("SDK Configuration")]
        [SerializeField] private string _placementId = "test_placement_123";
        [SerializeField] private string _baseURL = Constants.BaseURL;
        [Tooltip("Optional: SSP host only (e.g. ssp-bcc-ads.com or 127.0.0.1:8787). Overrides Base URL when non-empty.")]
        [SerializeField] private string _adRequestAuthority = "";
        [SerializeField] private bool _enableDebugMode = true;
        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private bool _enableTestMode = false;
        [Tooltip("Direct SDK: use sample ad buttons. AppLovin MAX: init only — see Documentation~/INSTALL.md")]
        [SerializeField] private BidscubeIntegrationMode _integrationMode = BidscubeIntegrationMode.DirectSdk;
        [SerializeField] private bool _loadIntegrationModeFromPlayerPrefs = false;
        [SerializeField] private string _integrationModePlayerPrefsKey = "bidscube_integration_mode";

        [Header("AppLovin MAX Unity (after Bidscube init in mediation mode)")]
        [Tooltip("Optional; MAX sample also reads the key from AppLovin Integration Manager.")]
        [SerializeField] private string _appLovinSdkKey = "";
        [SerializeField] private string _maxInterstitialAdUnitId = "";
        [SerializeField] private string _maxRewardedAdUnitId = "";
        [SerializeField] private string _maxBannerAdUnitId = "";
        [SerializeField] private bool _showIntegrationModeBar = true;

        [Header("UI References")]
        [SerializeField] private Button _initButton;
        [SerializeField] private Button _imageAdButton;
        [SerializeField] private Button _videoAdButton;
        [SerializeField] private Button _nativeAdButton;
        [SerializeField] private Button _headerBannerButton;
        [SerializeField] private Button _footerBannerButton;
        [SerializeField] private Button _sidebarBannerButton;
        [SerializeField] private Button _customBannerButton;
        [SerializeField] private Button _consentButton;
        [SerializeField] private Button _removeAllBannersButton;

        [Header("Status Display")]
        [SerializeField] private Text _statusText;
        [SerializeField] private ScrollRect _logScrollRect;
        [SerializeField] private Text _logText;

        [Header("Banner Display Areas")]
        [SerializeField] private RectTransform _headerBannerArea;
        [SerializeField] private RectTransform _footerBannerArea;
        [SerializeField] private RectTransform _sidebarBannerArea;

        [Header("Navigation")]
        [SerializeField] private Button _sdkTestButton;
        [SerializeField] private Button _consentTestButton;
        [SerializeField] private Button _windowedAdButton;

        private string _logContent = "";
        private Text _integrationModeHintText;
        private bool _maxBannerVisible;

        private void Awake()
        {
            if (_loadIntegrationModeFromPlayerPrefs &&
                !string.IsNullOrEmpty(_integrationModePlayerPrefsKey) &&
                PlayerPrefs.HasKey(_integrationModePlayerPrefsKey))
            {
                _integrationMode = BidscubeIntegrationModeWire.FromWire(PlayerPrefs.GetString(_integrationModePlayerPrefsKey));
            }
        }

        private void Start()
        {
            SetupUI();
            BuildIntegrationModeBar();
            UpdateStatus("Ready to initialize SDK");
        }

        private void SetupUI()
        {
            // Initialize SDK button
            if (_initButton != null)
                _initButton.onClick.AddListener(InitializeSDK);

            // Ad type buttons
            if (_imageAdButton != null)
                _imageAdButton.onClick.AddListener(ShowImageAd);

#if !BIDSCUBE_ANDROID_LITE_NO_VIDEO
            if (_videoAdButton != null)
                _videoAdButton.onClick.AddListener(ShowVideoAd);
#else
            if (_videoAdButton != null)
                _videoAdButton.gameObject.SetActive(false);
#endif

            if (_nativeAdButton != null)
                _nativeAdButton.onClick.AddListener(ShowNativeAd);

            // Banner buttons
            if (_headerBannerButton != null)
                _headerBannerButton.onClick.AddListener(ShowHeaderBanner);

            if (_footerBannerButton != null)
                _footerBannerButton.onClick.AddListener(ShowFooterBanner);

            if (_sidebarBannerButton != null)
                _sidebarBannerButton.onClick.AddListener(ShowSidebarBanner);

            if (_customBannerButton != null)
                _customBannerButton.onClick.AddListener(ShowCustomBanner);

            // Other buttons
            if (_consentButton != null)
                _consentButton.onClick.AddListener(ShowConsentForm);

            if (_removeAllBannersButton != null)
                _removeAllBannersButton.onClick.AddListener(RemoveAllBanners);

            // Navigation buttons
            if (_sdkTestButton != null)
                _sdkTestButton.onClick.AddListener(() => GetComponent<SceneManager>()?.LoadSDKTestScene());

            if (_consentTestButton != null)
                _consentTestButton.onClick.AddListener(() => GetComponent<SceneManager>()?.LoadConsentTestScene());

            if (_windowedAdButton != null)
                _windowedAdButton.onClick.AddListener(() => GetComponent<SceneManager>()?.LoadWindowedAdScene());
        }

        private void BuildIntegrationModeBar()
        {
            if (!_showIntegrationModeBar)
                return;
            if (GameObject.Find("BccIntegrationModeBar") != null)
                return;
            var canvas = ResolveHostCanvas();
            if (canvas == null)
                return;

            var root = new GameObject("BccIntegrationModeBar");
            var rootRt = root.AddComponent<RectTransform>();
            root.transform.SetParent(canvas.transform, false);
            rootRt.SetAsFirstSibling();
            rootRt.anchorMin = new Vector2(0f, 1f);
            rootRt.anchorMax = new Vector2(1f, 1f);
            rootRt.pivot = new Vector2(0.5f, 1f);
            rootRt.anchoredPosition = Vector2.zero;
            rootRt.sizeDelta = new Vector2(0f, 96f);

            root.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.97f);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 6, 6);
            vlg.spacing = 4;
            vlg.childForceExpandWidth = true;

            CreateBarLabel(root.transform, "Bidscube integration", 12, FontStyle.Bold);
            var row = new GameObject("ModeRow");
            row.transform.SetParent(root.transform, false);
            row.AddComponent<LayoutElement>().preferredHeight = 36f;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childForceExpandWidth = true;

            CreateModeButton(row.transform, "Direct Unity SDK", BidscubeIntegrationMode.DirectSdk);
            CreateModeButton(row.transform, "AppLovin MAX (adapter)", BidscubeIntegrationMode.AppLovinMaxMediation);

            _integrationModeHintText = CreateBarLabel(root.transform, DescribeIntegrationMode(_integrationMode), 11, FontStyle.Normal);
        }

        private static string DescribeIntegrationMode(BidscubeIntegrationMode mode)
        {
            return mode.IsMediationMode()
                ? "MAX mode: Bidscube.Initialize (mediation) then MaxSdk — load/show via MAX; Bidscube creatives from C# stay off."
                : "Direct mode: use sample buttons; native adapter not used for these creatives.";
        }

        private Canvas ResolveHostCanvas()
        {
            var t = transform;
            while (t != null)
            {
                var c = t.GetComponent<Canvas>();
                if (c != null)
                    return c;
                t = t.parent;
            }

#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<Canvas>();
#else
            return FindObjectOfType<Canvas>();
#endif
        }

        private static Font BuiltinBarFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Text CreateBarLabel(Transform parent, string text, int fontSize, FontStyle style)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.font = BuiltinBarFont();
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = Color.white;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = fontSize + 6;
            le.flexibleWidth = 1f;
            return txt;
        }

        private void CreateModeButton(Transform parent, string label, BidscubeIntegrationMode mode)
        {
            var go = new GameObject("Btn_" + mode);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.38f, 0.65f, 1f);
            var btn = go.AddComponent<Button>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 34f;
            le.flexibleWidth = 1f;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(4f, 2f);
            trt.offsetMax = new Vector2(-4f, -2f);
            var txt = textGo.AddComponent<Text>();
            txt.text = label;
            txt.font = BuiltinBarFont();
            txt.fontSize = 11;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;

            btn.onClick.AddListener(() => OnIntegrationModeBarClicked(mode));
        }

        private void OnIntegrationModeBarClicked(BidscubeIntegrationMode mode)
        {
            _integrationMode = mode;
            if (!string.IsNullOrEmpty(_integrationModePlayerPrefsKey))
            {
                PlayerPrefs.SetString(_integrationModePlayerPrefsKey, mode.ToWireString());
                PlayerPrefs.Save();
            }

            if (_integrationModeHintText != null)
                _integrationModeHintText.text = DescribeIntegrationMode(mode);
            LogMessage("[Integration] Mode: " + mode.ToWireString() + " — " + DescribeIntegrationMode(mode));
            UpdateStatus("Mode: " + mode.ToWireString() + " (tap Init SDK)");
        }

        private void InitializeSDK()
        {
            var oldToolbar = GameObject.Find("BccMaxMediationToolbar");
            if (oldToolbar != null)
            {
                Destroy(oldToolbar);
                _maxBannerVisible = false;
            }

            LogMessage("Initializing Bidscube SDK...");

            var builder = new SDKConfig.Builder()
                .EnableLogging(_enableLogging)
                .EnableDebugMode(_enableDebugMode)
                .EnableTestMode(_enableTestMode)
                .DefaultAdTimeout(30000)
                .DefaultAdPosition(AdPosition.Unknown);

            SampleSspAuthorityConfig.ApplyTo(builder, _baseURL, _adRequestAuthority);

            if (_loadIntegrationModeFromPlayerPrefs &&
                !string.IsNullOrEmpty(_integrationModePlayerPrefsKey) &&
                PlayerPrefs.HasKey(_integrationModePlayerPrefsKey))
            {
                builder.IntegrationModeFromWire(PlayerPrefs.GetString(_integrationModePlayerPrefsKey));
                LogMessage($" Integration mode from PlayerPrefs ({_integrationModePlayerPrefsKey})");
            }
            else
            {
                builder.IntegrationMode(_integrationMode);
            }

            var config = builder.Build();

            BidscubeSDK.Initialize(config);

            if (BidscubeSDK.IsInitialized())
            {
                UpdateStatus("SDK Initialized Successfully");
                LogMessage(" SDK initialized with config:");
                LogMessage($"   - Base URL: {_baseURL}");
                LogMessage($"   - Debug Mode: {_enableDebugMode}");
                LogMessage($"   - Logging: {_enableLogging}");
                LogMessage($"   - Integration mode: {config.IntegrationMode.ToWireString()}");
                if (config.IntegrationMode.IsMediationMode())
                {
                    LogMessage(" AppLovin MAX mode: do not use C# creative APIs here; load/show via MAX. See Documentation~/INSTALL.md");
                    SetDirectSdkDemoButtonsInteractable(false);
                    StartCoroutine(InitializeMaxSdkAfterBidscubeCoroutine());
                }
                else
                {
                    SetDirectSdkDemoButtonsInteractable(true);
                }
            }
            else
            {
                UpdateStatus("SDK Initialization Failed");
                LogMessage(" SDK initialization failed");
            }
        }

        private IEnumerator InitializeMaxSdkAfterBidscubeCoroutine()
        {
            if (!AppLovinMaxUnityReflection.IsMaxSdkAvailable)
            {
                LogMessage("[MAX] AppLovin MAX Unity plugin not found. Add the official MAX plugin (MaxSdk) to this project — see Documentation~/INSTALL.md.");
                yield break;
            }

            LogMessage("[MAX] Bidscube initialized in mediation mode. Setting MAX SDK key (if provided) and calling MaxSdk.InitializeSdk() …");
            AppLovinMaxUnityReflection.TrySetSdkKey(_appLovinSdkKey);
            if (string.IsNullOrWhiteSpace(_appLovinSdkKey))
                LogMessage("[MAX] Tip: set _appLovinSdkKey here or set the key in AppLovin > Integration Manager.");

            if (!AppLovinMaxUnityReflection.TryInitializeSdk())
            {
                LogMessage("[MAX] MaxSdk.InitializeSdk() could not be invoked.");
                yield break;
            }

            const float timeoutSec = 45f;
            var waited = 0f;
            while (!AppLovinMaxUnityReflection.TryIsInitialized() && waited < timeoutSec)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!AppLovinMaxUnityReflection.TryIsInitialized())
            {
                LogMessage("[MAX] Timed out waiting for MaxSdk.IsInitialized(). Check SDK key and network.");
                yield break;
            }

            LogMessage("[MAX] MAX Unity SDK is ready. Use the toolbar for Load/Show — mediation uses the native Bidscube adapter.");
            BuildMaxMediationToolbar();
        }

        private void BuildMaxMediationToolbar()
        {
            if (!BidscubeSDK.IsInitialized() || !_integrationMode.IsMediationMode())
                return;
            if (GameObject.Find("BccMaxMediationToolbar") != null)
                return;
            var canvas = ResolveHostCanvas();
            if (canvas == null)
                return;

            var root = new GameObject("BccMaxMediationToolbar");
            var rootRt = root.AddComponent<RectTransform>();
            root.transform.SetParent(canvas.transform, false);
            rootRt.anchorMin = new Vector2(0f, 1f);
            rootRt.anchorMax = new Vector2(1f, 1f);
            rootRt.pivot = new Vector2(0.5f, 1f);
            rootRt.anchoredPosition = new Vector2(0f, -100f);
            rootRt.sizeDelta = new Vector2(0f, 152f);

            root.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.96f);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 6, 6);
            vlg.spacing = 4;
            vlg.childForceExpandWidth = true;

            CreateBarLabel(root.transform, "AppLovin MAX (Unity) — same flow as native MAX + Bidscube adapter", 11, FontStyle.Bold);

            var row1 = new GameObject("MaxRow1");
            row1.transform.SetParent(root.transform, false);
            row1.AddComponent<LayoutElement>().preferredHeight = 34f;
            var h1 = row1.AddComponent<HorizontalLayoutGroup>();
            h1.spacing = 6;
            h1.childForceExpandWidth = true;
            CreateMaxActionButton(h1.transform, "Load interstitial", () => TryMaxLoadInterstitial());
            CreateMaxActionButton(h1.transform, "Show interstitial", () => TryMaxShowInterstitial());

            var row2 = new GameObject("MaxRow2");
            row2.transform.SetParent(root.transform, false);
            row2.AddComponent<LayoutElement>().preferredHeight = 34f;
            var h2 = row2.AddComponent<HorizontalLayoutGroup>();
            h2.spacing = 6;
            h2.childForceExpandWidth = true;
            CreateMaxActionButton(h2.transform, "Load rewarded", () => TryMaxLoadRewarded());
            CreateMaxActionButton(h2.transform, "Show rewarded", () => TryMaxShowRewarded());

            var row3 = new GameObject("MaxRow3");
            row3.transform.SetParent(root.transform, false);
            row3.AddComponent<LayoutElement>().preferredHeight = 34f;
            var h3 = row3.AddComponent<HorizontalLayoutGroup>();
            h3.spacing = 6;
            h3.childForceExpandWidth = true;
            CreateMaxActionButton(h3.transform, "Banner create / show", () => TryMaxToggleBanner());
            CreateMaxActionButton(h3.transform, "Mediation debugger", () =>
            {
                AppLovinMaxUnityReflection.TryShowMediationDebugger();
                LogMessage("[MAX] ShowMediationDebugger()");
            });
        }

        private void CreateMaxActionButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("MaxBtn_" + label.GetHashCode());
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = new Color(0.25f, 0.5f, 0.35f, 1f);
            var btn = go.AddComponent<Button>();
            go.AddComponent<LayoutElement>().preferredHeight = 32f;
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(3f, 2f);
            trt.offsetMax = new Vector2(-3f, -2f);
            var txt = textGo.AddComponent<Text>();
            txt.text = label;
            txt.font = BuiltinBarFont();
            txt.fontSize = 10;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            btn.onClick.AddListener(onClick);
        }

        private void TryMaxLoadInterstitial()
        {
            if (string.IsNullOrWhiteSpace(_maxInterstitialAdUnitId))
            {
                LogMessage("[MAX] Set _maxInterstitialAdUnitId (MAX dashboard ad unit).");
                return;
            }

            AppLovinMaxUnityReflection.TryLoadInterstitial(_maxInterstitialAdUnitId);
            LogMessage("[MAX] LoadInterstitial(" + _maxInterstitialAdUnitId + ")");
        }

        private void TryMaxShowInterstitial()
        {
            if (string.IsNullOrWhiteSpace(_maxInterstitialAdUnitId))
            {
                LogMessage("[MAX] Set _maxInterstitialAdUnitId.");
                return;
            }

            if (!AppLovinMaxUnityReflection.TryIsInterstitialReady(_maxInterstitialAdUnitId))
            {
                LogMessage("[MAX] Interstitial not ready yet.");
                return;
            }

            AppLovinMaxUnityReflection.TryShowInterstitial(_maxInterstitialAdUnitId);
            LogMessage("[MAX] ShowInterstitial(" + _maxInterstitialAdUnitId + ")");
        }

        private void TryMaxLoadRewarded()
        {
            if (string.IsNullOrWhiteSpace(_maxRewardedAdUnitId))
            {
                LogMessage("[MAX] Set _maxRewardedAdUnitId.");
                return;
            }

            AppLovinMaxUnityReflection.TryLoadRewardedAd(_maxRewardedAdUnitId);
            LogMessage("[MAX] LoadRewardedAd(" + _maxRewardedAdUnitId + ")");
        }

        private void TryMaxShowRewarded()
        {
            if (string.IsNullOrWhiteSpace(_maxRewardedAdUnitId))
            {
                LogMessage("[MAX] Set _maxRewardedAdUnitId.");
                return;
            }

            if (!AppLovinMaxUnityReflection.TryIsRewardedAdReady(_maxRewardedAdUnitId))
            {
                LogMessage("[MAX] Rewarded not ready yet.");
                return;
            }

            AppLovinMaxUnityReflection.TryShowRewardedAd(_maxRewardedAdUnitId);
            LogMessage("[MAX] ShowRewardedAd(" + _maxRewardedAdUnitId + ")");
        }

        private void TryMaxToggleBanner()
        {
            if (string.IsNullOrWhiteSpace(_maxBannerAdUnitId))
            {
                LogMessage("[MAX] Set _maxBannerAdUnitId.");
                return;
            }

            if (!_maxBannerVisible)
            {
                if (AppLovinMaxUnityReflection.TryCreateBannerBottomCenter(_maxBannerAdUnitId))
                {
                    AppLovinMaxUnityReflection.TryShowBanner(_maxBannerAdUnitId);
                    _maxBannerVisible = true;
                    LogMessage("[MAX] CreateBanner + ShowBanner(" + _maxBannerAdUnitId + ")");
                }
                else
                    LogMessage("[MAX] CreateBanner failed (check MAX plugin version / API).");
            }
            else
            {
                AppLovinMaxUnityReflection.TryHideBanner(_maxBannerAdUnitId);
                AppLovinMaxUnityReflection.TryDestroyBanner(_maxBannerAdUnitId);
                _maxBannerVisible = false;
                LogMessage("[MAX] Banner hidden and destroyed.");
            }
        }

        private void SetDirectSdkDemoButtonsInteractable(bool value)
        {
            void Set(Button b)
            {
                if (b != null) b.interactable = value;
            }
            Set(_imageAdButton);
#if !BIDSCUBE_ANDROID_LITE_NO_VIDEO
            Set(_videoAdButton);
#endif
            Set(_nativeAdButton);
            Set(_headerBannerButton);
            Set(_footerBannerButton);
            Set(_sidebarBannerButton);
            Set(_customBannerButton);
            Set(_removeAllBannersButton);
        }

        private void ShowImageAd()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage(" Showing Image Ad...");
            var adViewControllerObj = new GameObject("AdViewController");
            var adViewController = adViewControllerObj.AddComponent<AdViewController>();
            adViewController.Initialize(_placementId, AdType.Image, this);
        }

#if !BIDSCUBE_ANDROID_LITE_NO_VIDEO
        private void ShowVideoAd()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage("🎥 Showing Video Ad...");
            BidscubeSDK.ShowVideoAd(_placementId, this);
        }
#else
        private void ShowVideoAd()
        {
            LogMessage("Video ads are disabled in LiteNoVideo build. Switch export settings to FullWithVideo for video.");
        }
#endif

        private void ShowNativeAd()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage("📱 Showing Native Ad...");
            BidscubeSDK.ShowNativeAd(_placementId, this);
        }

        private void ShowHeaderBanner()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage("📊 Showing Header Banner...");
            BidscubeSDK.ShowHeaderBanner(_placementId, this);
        }

        private void ShowFooterBanner()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage("📊 Showing Footer Banner...");
            BidscubeSDK.ShowFooterBanner(_placementId, this);
        }

        private void ShowSidebarBanner()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage("📊 Showing Sidebar Banner...");
            BidscubeSDK.ShowSidebarBanner(_placementId, this);
        }

        private void ShowCustomBanner()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage("📊 Showing Custom Banner (320x50)...");
            BidscubeSDK.ShowCustomBanner(_placementId, AdPosition.Header, 320, 50, this);
        }

        private void ShowConsentForm()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage("🔒 Showing Consent Form...");
            BidscubeSDK.ShowConsentForm(this);
        }

        private void RemoveAllBanners()
        {
            if (!BidscubeSDK.IsInitialized())
            {
                LogMessage(" SDK not initialized. Please initialize first.");
                return;
            }

            LogMessage(" Removing all banners...");
            BidscubeSDK.RemoveAllBanners();
        }

        private void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = $"Status: {status}";
            }
        }

        private void LogMessage(string message)
        {
            _logContent += $"[{System.DateTime.Now:HH:mm:ss}] {message}\n";

            if (_logText != null)
            {
                _logText.text = _logContent;

                // Auto-scroll to bottom
                if (_logScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    _logScrollRect.verticalNormalizedPosition = 0f;
                }
            }

            Logger.Info($"[BidscubeExample] {message}");
        }

        #region IAdCallback Implementation

        public void OnAdLoading(string placementId)
        {
            LogMessage($"⏳ Ad loading: {placementId}");
            UpdateStatus($"Loading ad: {placementId}");
        }

        public void OnAdLoaded(string placementId)
        {
            LogMessage($" Ad loaded: {placementId}");
            UpdateStatus($"Ad loaded: {placementId}");
        }

        public void OnAdDisplayed(string placementId)
        {
            LogMessage($" Ad displayed: {placementId}");
            UpdateStatus($"Ad displayed: {placementId}");
        }

        public void OnAdClicked(string placementId)
        {
            LogMessage($"👆 Ad clicked: {placementId}");
            UpdateStatus($"Ad clicked: {placementId}");
        }

        public void OnAdClosed(string placementId)
        {
            LogMessage($" Ad closed: {placementId}");
            UpdateStatus($"Ad closed: {placementId}");
        }

        public void OnAdFailed(string placementId, int errorCode, string errorMessage)
        {
            LogMessage($" Ad failed: {placementId} (Code: {errorCode}, Message: {errorMessage})");
            UpdateStatus($"Ad failed: {placementId}");
        }

        public void OnVideoAdStarted(string placementId)
        {
            LogMessage($" Video ad started: {placementId}");
        }

        public void OnVideoAdCompleted(string placementId)
        {
            LogMessage($"🏁 Video ad completed: {placementId}");
        }

        public void OnVideoAdSkipped(string placementId)
        {
            LogMessage($" Video ad skipped: {placementId}");
        }

        public void OnVideoAdSkippable(string placementId)
        {
            LogMessage($" Video ad skippable: {placementId}");
        }

        public void OnInstallButtonClicked(string placementId, string buttonText)
        {
            LogMessage($"Install button clicked: {placementId} ({buttonText})");
        }

        /// <inheritdoc />
        public bool OnAdRenderOverride(string placementId, string adm, AdType adType, int position)
        {
            int admLen = adm != null ? adm.Length : 0;
            LogMessage($"OnAdRenderOverride: placementId={placementId}, adType={adType}, position={position}, admLength={admLen}");
            return false;
        }

        #endregion

        #region IConsentCallback Implementation

        public void OnConsentInfoUpdated()
        {
            LogMessage("Consent info updated");
        }

        public void OnConsentInfoUpdateFailed(System.Exception error)
        {
            LogMessage($" Consent info update failed: {error.Message}");
        }

        public void OnConsentFormShown()
        {
            LogMessage(" Consent form shown");
        }

        public void OnConsentFormError(System.Exception error)
        {
            LogMessage($" Consent form error: {error.Message}");
        }

        public void OnConsentGranted()
        {
            LogMessage(" Consent granted");
            UpdateStatus("Consent granted");
        }

        public void OnConsentDenied()
        {
            LogMessage(" Consent denied");
            UpdateStatus("Consent denied");
        }

        public void OnConsentNotRequired()
        {
            LogMessage(" Consent not required");
            UpdateStatus("Consent not required");
        }

        public void OnConsentStatusChanged(bool hasConsent)
        {
            LogMessage($" Consent status changed: {hasConsent}");
            UpdateStatus($"Consent: {hasConsent}");
        }

        #endregion
    }
}
