namespace BidscubeSDK.Mediation
{
    /// <summary>
    /// Menu / Back for direct Unity video on TV. Matches native
    /// <c>VideoDialogRemoteUx.dispatchBack</c>: close wins, rewarded never skips,
    /// interstitial skips only when Skip is armed, otherwise the key is consumed.
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
