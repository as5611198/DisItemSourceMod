using System;
using System.Text;
using DuckovCoreAPI;
using UnityEngine;

namespace DisplayItemSourceMod
{
    // Private presentation states; these are not replacements for Core's public DTOs.
    internal enum SourceHintKind
    {
        Hidden, Scanning, ScanTimedOut, Native, Inferred, Registered, Unknown, Failed, Masked
    }

    internal sealed class SourceHintState
    {
        private long generation;
        private double scanStarted;
        private string name = "";

        public int TypeID { get; private set; } = -1;
        public SourceHintKind Kind { get; private set; } = SourceHintKind.Hidden;
        public ItemSourceInfo? Source { get; private set; }
        public bool IsActive => TypeID >= 0;
        private SystemLanguage language = SystemLanguage.English;

        public void SetLanguage(SystemLanguage value) => language = value;

        public long Begin(int typeID, double realtime)
        {
            Clear();
            if (typeID >= 0)
            {
                TypeID = typeID;
                scanStarted = realtime;
                Kind = SourceHintKind.Scanning;
            }
            return generation;
        }

        public bool IsCurrent(long token) => IsActive && token == generation;

        public void Clear()
        {
            generation++;
            TypeID = -1;
            Kind = SourceHintKind.Hidden;
            name = "";
            Source = null;
        }

        public void ObserveHover(bool shown, int displayedTypeID)
        {
            if (IsActive && (!shown || displayedTypeID != TypeID)) Clear();
        }

        public void Wait(long token, double realtime)
        {
            if (!IsCurrent(token)) return;
            if (Kind != SourceHintKind.Scanning && Kind != SourceHintKind.ScanTimedOut)
                scanStarted = realtime;
            Kind = realtime - scanStarted >= 120 ? SourceHintKind.ScanTimedOut : SourceHintKind.Scanning;
            name = "";
            Source = null;
        }

        public void Refresh(long token, bool ready, double realtime)
        {
            if (!IsCurrent(token)) return;
            if (!ready) Wait(token, realtime);
            else
            {
                bool found = QueryApi.TryGetItemSource(TypeID, out var source);
                ResolveSource(token, found, source);
            }
        }

        public void ResolveSource(long token, bool found, ItemSourceInfo source)
        {
            if (!IsCurrent(token)) return;
            if (!found || source.TypeID != TypeID)
                SetResult(token, SourceHintKind.Unknown, "");
            else
            {
                var kind = source.Evidence switch
                {
                    SourceEvidence.Native => SourceHintKind.Native,
                    SourceEvidence.Inferred => SourceHintKind.Inferred,
                    SourceEvidence.FeatherRegistered => SourceHintKind.Registered,
                    _ => SourceHintKind.Unknown
                };
                string displayName = source.DisplayName;
                SetResult(token, kind, displayName);
                if (Kind == SourceHintKind.Inferred || Kind == SourceHintKind.Registered || Kind == SourceHintKind.Native)
                    Source = source;
            }
        }

        public void SetResult(long token, SourceHintKind kind, string? displayName)
        {
            if (!IsCurrent(token)) return;
            Source = null;
            name = CompactName(displayName);
            Kind = (kind == SourceHintKind.Inferred || kind == SourceHintKind.Registered) && name.Length == 0
                ? SourceHintKind.Unknown : kind;
        }

        public void Fail(long token)
        {
            if (IsCurrent(token)) SetResult(token, SourceHintKind.Failed, "");
        }

        public void Redact(long token)
        {
            if (IsCurrent(token)) SetResult(token, SourceHintKind.Masked, "");
        }

        public string Text => Kind == SourceHintKind.Masked ? "？？？" : SourceLocalization.Format(Kind, name, language);

        private static string CompactName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            value = SourceV3PlainText.Readable(value!);
            var builder = new StringBuilder();
            bool space = false;
            foreach (char character in value!)
            {
                if (char.IsWhiteSpace(character) || char.IsControl(character))
                {
                    space = builder.Length > 0;
                    continue;
                }
                if (space) builder.Append(' ');
                space = false;
                builder.Append(character);
                if (builder.Length >= 80)
                {
                    // Do not leave half a UTF-16 surrogate pair at the truncation boundary.
                    if (char.IsHighSurrogate(builder[builder.Length - 1])) builder.Length--;
                    builder.Append('…');
                    break;
                }
            }
            return builder.ToString();
        }
    }
}
