using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace BidscubeSDK.Mediation
{
    /// <summary>
    /// Apple TV (and Android TV direct-SDK) remote for <see cref="VideoAdView"/>.
    /// Menu follows the same Back rules as the Android TV activity. Select uses the focused Skip or Close button.
    /// Android MAX ads play in the native activity and are ignored here.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class BidscubeTvRemoteDriver : MonoBehaviour
    {
        private const float TvSkipWidth = 240f;
        private const float TvSkipHeight = 80f;
        private const float TvCloseSize = 96f;

        private static readonly FieldInfo FormatField =
            typeof(VideoAdView).GetField("_videoAdFormat", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CompletedField =
            typeof(VideoAdView).GetField("_hasCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo EndCardField =
            typeof(VideoAdView).GetField("_endCardShown", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo AwaitingCloseField =
            typeof(VideoAdView).GetField("_awaitingManualClose", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ClosedField =
            typeof(VideoAdView).GetField("_hasClosed", BindingFlags.Instance | BindingFlags.NonPublic);

        private bool _confirmOpen;
        private GameObject _confirmRoot;
        private VideoPlayer _pausedPlayer;
        private bool _resumeAfterConfirm;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!BidscubeTvPlatform.IsTelevision)
                return;
            var go = new GameObject("BidscubeTvRemoteDriver");
            DontDestroyOnLoad(go);
            go.AddComponent<BidscubeTvRemoteDriver>();
        }

        private void Update()
        {
            if (!BidscubeTvPlatform.IsTelevision)
                return;

            EnsureEventSystem();
            var views = Object.FindObjectsOfType<VideoAdView>();
            for (var i = 0; i < views.Length; i++)
                ApplyChrome(views[i]);

            if (_confirmOpen || !BidscubeTvPlatform.WasMenuPressed() || views.Length == 0)
                return;

            var view = views[0];
            if (IsClosed(view))
                return;

            var skip = FindChildButton(view, "SkipButton");
            var close = FindChildButton(view, "CloseButton");
            bool rewarded = IsRewarded(view);
            bool canClose = CanClose(view);
            bool canSkip = !rewarded && skip != null && skip.gameObject.activeInHierarchy && skip.interactable
                && ButtonLabel(skip) == "Skip";

            switch (BidscubeTvVideoBackPolicy.OnMenu(rewarded, canClose, canSkip))
            {
                case BidscubeTvVideoBackPolicy.Action.Skip:
                    skip.onClick.Invoke();
                    break;
                case BidscubeTvVideoBackPolicy.Action.Close:
                    if (close != null)
                        close.onClick.Invoke();
                    break;
                case BidscubeTvVideoBackPolicy.Action.ConfirmRewardForfeit:
                    ShowConfirm(view, close);
                    break;
            }
        }

        private void ApplyChrome(VideoAdView view)
        {
            if (view == null || IsClosed(view))
                return;

            var skip = FindChildButton(view, "SkipButton");
            var close = FindChildButton(view, "CloseButton");
            bool rewarded = IsRewarded(view);
            bool canClose = CanClose(view);

            if (rewarded && skip != null && !canClose)
            {
                skip.interactable = false;
                skip.gameObject.SetActive(false);
            }

            if (close != null)
            {
                close.interactable = canClose;
                Grow(close, TvCloseSize, TvCloseSize);
            }

            if (skip != null)
                Grow(skip, TvSkipWidth, TvSkipHeight);

            WireNavigation(skip, close);

            if (_confirmOpen)
                return;

            var selected = canClose ? close : (skip != null && skip.interactable && skip.gameObject.activeInHierarchy ? skip : null);
            if (selected == null)
                return;

            var eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject != selected.gameObject)
                eventSystem.SetSelectedGameObject(selected.gameObject);
        }

        private void ShowConfirm(VideoAdView view, Button close)
        {
            _confirmOpen = true;
            var player = view.GetComponent<VideoPlayer>();
            _pausedPlayer = player;
            _resumeAfterConfirm = player != null && player.isPlaying;
            if (_resumeAfterConfirm)
                player.Pause();

            _confirmRoot = new GameObject(
                "BidscubeTvRewardConfirm",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = _confirmRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_confirmRoot.transform, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.2f, 0.32f);
            panelRt.anchorMax = new Vector2(0.8f, 0.68f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(panel.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0.06f, 0.42f);
            labelRt.anchorMax = new Vector2(0.94f, 0.94f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 32;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = "Leave video?\nIf you leave now, you will not receive a reward.";

            var stay = CreateChoice(panel.transform, "Keep watching", new Vector2(0.06f, 0.08f), new Vector2(0.48f, 0.36f), () =>
            {
                DismissConfirm(resume: true);
            });
            var leave = CreateChoice(panel.transform, "Leave", new Vector2(0.52f, 0.08f), new Vector2(0.94f, 0.36f), () =>
            {
                DismissConfirm(resume: false);
                if (close != null)
                    close.onClick.Invoke();
            });

            var stayNav = stay.navigation;
            stayNav.mode = Navigation.Mode.Explicit;
            stayNav.selectOnRight = leave;
            stay.navigation = stayNav;
            var leaveNav = leave.navigation;
            leaveNav.mode = Navigation.Mode.Explicit;
            leaveNav.selectOnLeft = stay;
            leave.navigation = leaveNav;

            var eventSystem = EventSystem.current;
            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(stay.gameObject);
        }

        private void DismissConfirm(bool resume)
        {
            if (_confirmRoot != null)
                Destroy(_confirmRoot);
            _confirmRoot = null;
            _confirmOpen = false;
            if (resume && _resumeAfterConfirm && _pausedPlayer != null)
                _pausedPlayer.Play();
            _pausedPlayer = null;
            _resumeAfterConfirm = false;
        }

        private static Button CreateChoice(
            Transform parent,
            string caption,
            Vector2 anchorMin,
            Vector2 anchorMax,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f, 1f);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = caption;
            text.raycastTarget = false;
            return button;
        }

        private static void Grow(Button button, float width, float height)
        {
            var rt = button.transform as RectTransform;
            if (rt == null || rt.sizeDelta.x >= width)
                return;
            rt.sizeDelta = new Vector2(width, height);
        }

        private static void WireNavigation(Button skip, Button close)
        {
            if (skip != null && close != null)
            {
                var skipNav = skip.navigation;
                skipNav.mode = Navigation.Mode.Explicit;
                skipNav.selectOnRight = close;
                skip.navigation = skipNav;
                var closeNav = close.navigation;
                closeNav.mode = Navigation.Mode.Explicit;
                closeNav.selectOnLeft = skip;
                close.navigation = closeNav;
            }
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(go);
        }

        private static Button FindChildButton(VideoAdView view, string name)
        {
            var buttons = view.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].name == name)
                    return buttons[i];
            }

            return null;
        }

        private static bool IsRewarded(VideoAdView view)
        {
            return FormatField != null && FormatField.GetValue(view) is VideoAdFormat format
                && format == VideoAdFormat.Rewarded;
        }

        private static bool CanClose(VideoAdView view)
        {
            return ReadBool(CompletedField, view) || ReadBool(EndCardField, view) || ReadBool(AwaitingCloseField, view);
        }

        private static bool IsClosed(VideoAdView view)
        {
            return ReadBool(ClosedField, view);
        }

        private static bool ReadBool(FieldInfo field, VideoAdView view)
        {
            return field != null && field.GetValue(view) is bool value && value;
        }

        private static string ButtonLabel(Button button)
        {
            var text = button.GetComponentInChildren<Text>();
            return text != null ? text.text : null;
        }
    }
}
