using System;
using System.Collections.Generic;
using BidscubeSDK;
using UnityEditor;

namespace BidscubeSDK.Editor
{
    internal static class BidscubeDefineApplicator
    {
        public static void ApplyFromStoredFeatureSet()
        {
            Apply(BidscubeFeatureSetStore.Load());
        }

        public static void Apply(BidscubeFeatureSet featureSet)
        {
            var enableVideo = featureSet == BidscubeFeatureSet.FullWithVideo;
            ApplyEnableVideoDefine(enableVideo);
        }

        static void ApplyEnableVideoDefine(bool enableVideo)
        {
            var symbol = VideoBuildDefines.EnableVideoSymbol;
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown)
                    continue;
                try
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                    var list = new List<string>();
                    foreach (var p in defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (p != symbol)
                            list.Add(p);
                    }

                    if (enableVideo)
                        list.Add(symbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", list));
                }
                catch
                {
                    // Some groups are not applicable on this Unity license/platform module.
                }
            }
        }
    }
}
