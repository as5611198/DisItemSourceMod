using System.Collections.Generic;
using System.Globalization;
using DuckovCoreAPI;
using UnityEngine;

namespace DisplayItemSourceMod
{
    internal static class SourceDiagnosticsPresentation
    {
        public static string Format(DiagnosticQueryResult? result, int typeID, SystemLanguage language)
        {
            if (result == null || typeID < 0 || result.FilterTypeID != typeID) return "";
            var t = SourceDiagnosticsText.For(language);
            if (result.Status == DiagnosticQueryStatus.NotReady) return t[0] + ": " + t[7];
            if (result.Status != DiagnosticQueryStatus.Success) return "";
            int errors = 0, warnings = 0, info = 0, history = 0;
            foreach (var record in result.Records)
            {
                if (record.TypeID != typeID) continue;
                // Historical observations are information, never proof of two
                // currently active conflicting mods, even with a future DTO variant.
                if (record.Evidence == DiagnosticEvidence.Historical || record.Kind == DiagnosticKind.HistoricalIdentityChange || record.Kind == DiagnosticKind.HistoricalItemRemoved)
                    history++;
                else if (record.Severity == DiagnosticSeverity.Error) errors++;
                else if (record.Severity == DiagnosticSeverity.Warning) warnings++;
                else info++;
            }
            var parts = new List<string>();
            void Count(int value, int label)
            {
                if (value > 0) parts.Add(t[label] + " " + value.ToString(CultureInfo.InvariantCulture));
            }
            Count(errors, 1); Count(warnings, 2); Count(info, 3); Count(history, 4);
            if (!result.CatalogComplete) parts.Add(t[5]);
            // This is a global coverage warning, not a hovered-item hit count.
            if (result.OmittedRecordCount > 0) parts.Add(t[6]);
            return parts.Count == 0 ? "" : t[0] + ": " + string.Join(" · ", parts);
        }
    }
}
