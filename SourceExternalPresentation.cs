using System.Collections.Generic;
using System.Globalization;
using DuckovCoreAPI;
using UnityEngine;
namespace DisplayItemSourceMod
{
    internal static class SourceExternalPresentation
    {
        private static string Safe(string value, int limit, ref bool omitted)
        {
            string normalized = SourceV3Presentation.Safe(value);
            if (normalized.Length <= limit) return normalized;
            omitted = true;
            int length = limit;
            if (char.IsHighSurrogate(normalized[length - 1])) length--;
            return normalized.Substring(0, length) + "…";
        }
        public static string Format(string native, ExternalAcquisitionItemResult? result, SystemLanguage language)
        {
            var settings = AcquisitionPresentationApi.GetSettings();
            return FormatWithNativeClues(native, result, null, null, null, settings, language);
        }

        public static string FormatWithNativeClues(string native, ExternalAcquisitionItemResult? result,
            NativeLootItemResult? loot, MerchantClueResult? merchants, NativeLootDetailResult? details,
            AcquisitionPresentationSettings? settings, SystemLanguage language)
        {
            string compact = SourceNativeText.Compact(loot, merchants, language);
            var parts = new List<string>();
            if (native.Length > 0) parts.Add(native.Replace("\n", " · "));
            if (compact.Length > 0) parts.Add(compact);
            // Hover stays grouped even when detailed query pages are enabled.
            var kinds = new List<string>();
            if (result?.Status == ExternalAcquisitionStatus.Success)
                foreach (var entry in result.Entries)
                    if (entry.Definition.TypeID == result.TypeID && (entry.Provider.State == AcquisitionProviderState.Ready || entry.Provider.State == AcquisitionProviderState.Incomplete))
                    {
                        string kind = SourceExternalText.For(language)[entry.Definition.Kind == ExternalAcquisitionKind.Loot ? 10 : 11];
                        if (!kinds.Contains(kind)) kinds.Add(kind);
                    }
            if (kinds.Count > 0) parts.Add(string.Join(" · ", kinds));
            if (parts.Count == 0) return "";
            if (parts.Count == 1) return native.Length > 0 ? native : parts[0];
            return string.Join("\n", new[] { parts[0], string.Join(" · ", parts.GetRange(1, parts.Count - 1)) }).TrimEnd('\n');
        }

        // Raw definition formatter is diagnostic-only; production hover never calls it.
        public static string FormatDefinitionsWithNativeClues(string native, ExternalAcquisitionItemResult? result,
            NativeLootItemResult? loot, MerchantClueResult? merchants, NativeLootDetailResult? details,
            AcquisitionPresentationSettings? settings, SystemLanguage language)
            => FormatCore(native, result, loot, merchants, details, settings?.IsReady == true && settings.DetailedLoot, language);

        private static string FormatCore(string native, ExternalAcquisitionItemResult? result,
            NativeLootItemResult? loot, MerchantClueResult? merchants, NativeLootDetailResult? details,
            bool detailedLoot, SystemLanguage language)
        {
            var t = SourceExternalText.For(language);
            string compact = SourceNativeText.Compact(loot, merchants, language);
            string nativeWithClues = compact.Length == 0 ? native : native.Length == 0 ? compact : native.Replace("\n", " · ") + " · " + compact;
            if (result == null || (result.Status == ExternalAcquisitionStatus.Success && result.Providers.Count == 0)) return nativeWithClues;
            string Number(long value) => value.ToString(CultureInfo.InvariantCulture);
            string external;
            if (result.Status != ExternalAcquisitionStatus.Success)
            {
                if (result.Status != ExternalAcquisitionStatus.NotReady && result.Status != ExternalAcquisitionStatus.ContextChanged && result.Status != ExternalAcquisitionStatus.StaleCatalog) return native;
                external = t[1] + ": " + t[result.Status == ExternalAcquisitionStatus.NotReady ? 5 : 8];
            }
            else
            {
                var counts = new int[5]; bool known = false;
                foreach (var provider in result.Providers)
                {
                    int index = provider.State == AcquisitionProviderState.Ready ? 0 : provider.State == AcquisitionProviderState.Incomplete ? 1 :
                        provider.State == AcquisitionProviderState.NotReady ? 2 : provider.State == AcquisitionProviderState.NotSupported ? 4 : 3;
                    counts[index]++; if (index < 2) known = true;
                }
                var entries = new List<ExternalAcquisitionEntry>();
                foreach (var entry in result.Entries)
                    if (entry.Definition.TypeID == result.TypeID && (entry.Provider.State == AcquisitionProviderState.Ready || entry.Provider.State == AcquisitionProviderState.Incomplete)) entries.Add(entry);
                bool omitted = entries.Count > 1 || result.Providers.Count > 1;
                string sample = "";
                if (entries.Count > 0)
                {
                    var row = entries[0]; var d = row.Definition;
                    string display = Safe(row.Provider.DisplayName, 24, ref omitted), owner = Safe(row.Provider.OwnerID, 24, ref omitted), provider = Safe(row.Provider.ProviderID, 24, ref omitted);
                    string source = Safe(d.SourceLabel, 32, ref omitted), conditions = Safe(d.Conditions, 32, ref omitted);
                    sample = display + " [" + owner + "/" + provider + "] · " + t[d.Kind == ExternalAcquisitionKind.Loot ? 10 : 11] + " " + source + " " + Number(d.MinAmount) + "–" + Number(d.MaxAmount) +
                        " · " + t[13] + " " + (d.Chance.HasValue ? d.Chance.Value.ToString("G17", CultureInfo.InvariantCulture) : t[12]);
                    if (conditions.Length > 0) sample += " · " + t[14] + " " + conditions;
                }
                var parts = new List<string>();
                if (omitted) parts.Add(t[9]); // Before fields that TMP may ellipsize.
                if (known) parts.Add(detailedLoot ? t[2] + " " + Number(entries.Count) : t[2]);
                for (int i = 0; i < counts.Length; i++) if (counts[i] > 0) parts.Add(t[3 + i] + " " + Number(counts[i]));
                if (detailedLoot && sample.Length > 0) parts.Add(sample);
                external = t[1] + ": " + string.Join(" · ", parts);
            }
            return nativeWithClues.Length == 0 ? external : t[0] + " · " + nativeWithClues.Replace("\n", " · ") + "\n" + external;
        }
    }
}
