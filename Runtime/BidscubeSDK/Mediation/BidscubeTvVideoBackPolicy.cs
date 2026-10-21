namespace BidscubeSDK.Mediation
{
    /// <summary>
    /// Back / Menu for direct Unity video. Same rules as Android SDK 1.2.17
    /// <c>FullscreenBackPolicy</c>: close wins, rewarded never skips, interstitial
    /// skips only when Skip is armed.
    /// </summary>
    public static class BidscubeTvVideoBackPolicy
    {
        public enum Action
        {
            Ignore,
            Skip,
            Close,
            ConfirmRewardForfeit
        }

        /// <summary>
        /// Home or closing the app before the linear video ends. The view is not counted
        /// and a rewarded ad does not grant a reward.
        /// </summary>
        public static bool LeavingBeforeCompleteVoidsView(bool linearCompleted)
        {
            return !linearCompleted;
        }

        public static Action OnMenu(bool rewarded, bool canClose, bool canSkip)
        {
            if (canClose)
                return Action.Close;
            if (rewarded)
                return Action.ConfirmRewardForfeit;
            if (canSkip)
                return Action.Skip;
            return Action.Ignore;
        }
    }
}
