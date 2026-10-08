using System.Globalization;
using DuckovCoreAPI;
using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceAcquisitionPresentation
    {
        public static string Format(AcquisitionItemResult? result, SystemLanguage language)
            => FormatDefinitions(result, language, true);

        public static string FormatWithShopDetail(AcquisitionItemResult? result, SystemLanguage language, bool detailedShops)
        {
            if (result == null) return "";
            var t = SourceAcquisitionText.For(language);
            if (result.Status == AcquisitionQueryStatus.NotReady) return t[12];
            if (result.Status != AcquisitionQueryStatus.Success) return "";
            var acquisition = new System.Collections.Generic.List<string>();
            var usage = new System.Collections.Generic.List<string>();
            void Add(System.Collections.Generic.List<string> target, AcquisitionProviderKind kind, int count, int label, bool includeCount)
            {
                if (count <= 0) return;
                foreach (var provider in result.Providers)
                {
                    if (provider.Kind != kind || (provider.State != AcquisitionProviderState.Ready && provider.State != AcquisitionProviderState.Incomplete)) continue;
                    target.Add(t[label] + (includeCount ? " " + count.ToString(CultureInfo.InvariantCulture) : ""));
                    break;
                }
            }
            Add(acquisition, AcquisitionProviderKind.Decomposition, result.ProducedByDecomposition.Count, 2, true);
            Add(acquisition, AcquisitionProviderKind.Shops, result.Shops.Count, 3, detailedShops);
            Add(acquisition, AcquisitionProviderKind.Quests, result.QuestRewards.Count, 4, true);
            Add(usage, AcquisitionProviderKind.Decomposition, result.DecomposesInto.Count, 2, false);
            Add(usage, AcquisitionProviderKind.Quests, result.QuestTurnIns.Count, 5, true);
            return string.Join(" · ", acquisition) + (acquisition.Count > 0 && usage.Count > 0 ? "\n" : "") + string.Join(" · ", usage);
        }

        // Retained definition/status formatter for diagnostic consumers and regression checks.
        private static string FormatDefinitions(AcquisitionItemResult? result, SystemLanguage language, bool detailedShops)
        {
            if (result == null) return "";
            var t = SourceAcquisitionText.For(language);
            if (result.Status == AcquisitionQueryStatus.NotReady)
                return t[0] + ": " + t[12] + "\n" + t[1] + ": " + t[12];
            if (result.Status != AcquisitionQueryStatus.Success) return "";
            string Value(AcquisitionProviderKind kind, int count, bool presence = false)
            {
                var state = AcquisitionProviderState.NotReady;
                foreach (var provider in result.Providers)
                    if (provider.Kind == kind) { state = provider.State; break; }
                string value = presence ? t[count > 0 ? 6 : 7] : count.ToString(CultureInfo.InvariantCulture);
                return state switch
                {
                    AcquisitionProviderState.Ready => value,
                    AcquisitionProviderState.Incomplete => t[9] + " " + value,
                    AcquisitionProviderState.Unavailable => t[10],
                    AcquisitionProviderState.NotSupported => t[11],
                    _ => t[8]
                };
            }
            // Counts refer only to this item's definition rows. Global provider
            // diagnostics, stock, reward amounts and decomposition quantities are not counts.
            string shops = detailedShops ? Value(AcquisitionProviderKind.Shops, result.Shops.Count) :
                Value(AcquisitionProviderKind.Shops, result.Shops.Count > 0 ? 1 : 0, true);
            return t[0] + ": " + t[2] + " " + Value(AcquisitionProviderKind.Decomposition, result.ProducedByDecomposition.Count) +
                " · " + t[3] + " " + shops +
                " · " + t[4] + " " + Value(AcquisitionProviderKind.Quests, result.QuestRewards.Count) + "\n" +
                t[1] + ": " + t[2] + " " + Value(AcquisitionProviderKind.Decomposition, result.DecomposesInto.Count, true) +
                " · " + t[5] + " " + Value(AcquisitionProviderKind.Quests, result.QuestTurnIns.Count);
        }
    }
}
