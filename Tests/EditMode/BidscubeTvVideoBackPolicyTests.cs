using NUnit.Framework;

namespace BidscubeSDK.Mediation.Tests
{
    public sealed class BidscubeTvVideoBackPolicyTests
    {
        [Test]
        public void RewardedBackBeforeEndAsksToForfeit()
        {
            Assert.AreEqual(
                BidscubeTvVideoBackPolicy.Action.ConfirmRewardForfeit,
                BidscubeTvVideoBackPolicy.OnMenu(rewarded: true, canClose: false, canSkip: true));
        }

        [Test]
        public void RewardedBackAfterCompleteCloses()
        {
            Assert.AreEqual(
                BidscubeTvVideoBackPolicy.Action.Close,
                BidscubeTvVideoBackPolicy.OnMenu(rewarded: true, canClose: true, canSkip: false));
        }

        [Test]
        public void InterstitialBackBeforeSkipIsIgnored()
        {
            Assert.AreEqual(
                BidscubeTvVideoBackPolicy.Action.Ignore,
                BidscubeTvVideoBackPolicy.OnMenu(rewarded: false, canClose: false, canSkip: false));
        }

        [Test]
        public void InterstitialBackSkipsWhenSkipIsArmed()
        {
            Assert.AreEqual(
                BidscubeTvVideoBackPolicy.Action.Skip,
                BidscubeTvVideoBackPolicy.OnMenu(rewarded: false, canClose: false, canSkip: true));
        }

        [Test]
        public void LeavingBeforeCompleteVoidsTheView()
        {
            Assert.IsTrue(BidscubeTvVideoBackPolicy.LeavingBeforeCompleteVoidsView(linearCompleted: false));
            Assert.IsFalse(BidscubeTvVideoBackPolicy.LeavingBeforeCompleteVoidsView(linearCompleted: true));
        }
    }
}
