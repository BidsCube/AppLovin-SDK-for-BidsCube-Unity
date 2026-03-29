using UnityEngine;
using UnityEngine.UI;
using BidscubeSDK;

namespace BidscubeSDK.Controllers
{
    /// <summary>
    /// Comprehensive test scene for Bidscube Unity SDK
    /// Demonstrates all SDK functionality with proper UI hierarchy
    /// </summary>
    public class BidscubeExampleScene : MonoBehaviour, IAdCallback, IConsentCallback
    {
        [Header("SDK Configuration")]
        [SerializeField] private string _placementId = "test_placement_123";
        [SerializeField] private string _baseURL = Constants.BaseURL;
        [Tooltip("Optional: SSP host only (e.g. ssp-bcc-ads.com or 127.0.0.1:8787). Overrides Base URL when non-empty.")]
        [SerializeField] private string _adRequestAuthority = "";
        [SerializeField] private bool _enableDebugMode = true;
        [SerializeField] private bool _enableLogging = true;
        [SerializeField] private bool _enableTestMode = false;
        [Tooltip("Direct SDK: use sample ad buttons. AppLovin MAX: init only — see Documentation~/APPLOVIN_MAX.md")]
        [SerializeField] private BidscubeIntegrationMode _integrationMode = BidscubeIntegrationMode.DirectSdk;
        [SerializeField] private bool _loadIntegrationModeFromPlayerPrefs = false;
        [SerializeField] private string _integrationModePlayerPrefsKey = "bidscube_integration_mode";

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

        private void Start()
        {
            SetupUI();
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

            if (_videoAdButton != null)
                _videoAdButton.onClick.AddListener(ShowVideoAd);

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

        private void InitializeSDK()
        {
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
                    LogMessage(" AppLovin MAX mode: do not use C# creative APIs here; load/show via MAX. See Documentation~/APPLOVIN_MAX.md");
                    SetDirectSdkDemoButtonsInteractable(false);
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

        private void SetDirectSdkDemoButtonsInteractable(bool value)
        {
            void Set(Button b)
            {
                if (b != null) b.interactable = value;
            }
            Set(_imageAdButton);
            Set(_videoAdButton);
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
            LogMessage($"📱 Install button clicked: {placementId} ({buttonText})");
        }

        // Added missing OnAdRenderOverride (IAdCallback)
        public bool OnAdRenderOverride(string adm, int position)
        {
            int admLen = adm != null ? adm.Length : 0;
            LogMessage($"OnAdRenderOverride called: position={position}, admLength={admLen}");
            return false; // Let SDK render by default
        }

        #endregion

        #region IConsentCallback Implementation

        public void OnConsentInfoUpdated()
        {
            LogMessage("🔒 Consent info updated");
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
