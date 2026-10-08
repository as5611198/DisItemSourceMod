using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DuckovCoreAPI;
using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceV3Presentation
    {
        public static string Source(SourceHintState hint, bool detailed, bool modOnly, SystemLanguage language)
        {
            if (!hint.IsActive || (modOnly && hint.Kind == SourceHintKind.Native)) return "";
            string text = hint.Text;
            // The saved detailed preference is retained, but raw ownership and
            // resource identifiers belong in diagnostics, never the hover row.
            return text;
        }
        public static string Planning(int typeID, bool ready, bool favorite, bool pinned, TrackingSummary summary, SystemLanguage language)
            => PlanningRows(typeID, ready, favorite, pinned, summary, language, true);
        public static string PlanningWithoutMaterials(int typeID, bool ready, bool favorite, bool pinned, TrackingSummary summary, SystemLanguage language)
            => PlanningRows(typeID, ready, favorite, pinned, summary, language, false);
        private static string PlanningRows(int typeID, bool ready, bool favorite, bool pinned, TrackingSummary summary, SystemLanguage language, bool includeMaterials)
        {
            if (!ready || typeID <= 0) return "";
            var lines = new List<string>();
            string L(string key) => SourceV3Text.Label(key, language);
            if (favorite) lines.Add("★ " + L("favorite"));
            if (pinned) lines.Add("◆ " + L("pinned"));
            if (summary.Status != PlanStatus.Success || summary.Scope != InventoryScope.Carried)
            {
                // A failed unrelated plan is not a hovered-item fact.
            }
            else
            {
                foreach (var row in summary.Materials)
                {
                    if (row.TypeID != typeID || row.Required <= 0) continue;
                    string values = L("required") + " " + Number(row.Required);
                    if (summary.HasInventory && row.HasInventory)
                    {
                        if (row.Available > 0) values += ", " + L("available") + " " + Number(row.Available);
                        if (row.Missing > 0) values += ", " + L("missing") + " " + Number(row.Missing);
                    }
                    else values += ", " + L("unknownInventory");
                    if (includeMaterials) lines.Add("▸ " + L("material") + " (" + L("carried") + "): " + values);
                    break; // Core already merged the rows; never sum in Source.
                }
            }
            return string.Join("\n", lines);
        }
        public static string Hotkeys(bool ownsPresenter, IEnumerable<QueryBinding> bindings, SystemLanguage language)
        {
            if (!ownsPresenter) return "";
            var labels = new List<string>();
            foreach (var binding in bindings)
            {
                if (!binding.Enabled || !binding.IsAvailable || binding.Context != BindingContext.HoveredItem) continue;
                string key = binding.Action == QueryAction.QueryAll ? "all" : binding.Action == QueryAction.QueryRecipes ? "recipes" : "usages";
                labels.Add("[" + Safe(binding.KeyName) + "] " + SourceV3Text.Label(key, language));
            }
            return string.Join("  ", labels);
        }
        private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);
        public static string Safe(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            value = SourceV3PlainText.Readable(value!);
            var result = new StringBuilder(); bool space = false;
            foreach (char c in value!)
            {
                if (char.IsWhiteSpace(c) || char.IsControl(c)) { space = result.Length > 0; continue; }
                if (space) result.Append(' ');
                space = false; result.Append(c);
                if (result.Length >= 120)
                { if (char.IsHighSurrogate(result[result.Length-1])) result.Length--; result.Append('…'); break; }
            }
            return result.ToString();
        }
    }
}
