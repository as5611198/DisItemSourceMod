using System;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    /// <summary>Reuses the immutable Core planning snapshot until its revision or scope changes.</summary>
    internal sealed class SourceTrackingSummaryCache
    {
        private long revision = long.MinValue;
        private InventoryScope scope;
        private TrackingSummary? value;

        public bool TryGet(long currentRevision, InventoryScope currentScope, out TrackingSummary summary)
        {
            if (value != null && revision == currentRevision && scope == currentScope)
            {
                summary = value;
                return true;
            }
            summary = null!;
            return false;
        }

        public void Store(long currentRevision, InventoryScope currentScope, TrackingSummary summary)
        {
            revision = currentRevision;
            scope = currentScope;
            value = summary;
        }

        public void Clear()
        {
            revision = long.MinValue;
            value = null;
        }
    }

    /// <summary>Reports whether the visible row text changed since the previous render.</summary>
    internal sealed class SourceLayoutCache
    {
        private string[]? values;
        private bool[]? active;
        private int fontSize = int.MinValue;

        public bool Changed(int currentFontSize, bool[] currentActive, params string[] current)
        {
            if (values != null && active != null && values.Length == current.Length && active.Length == currentActive.Length && fontSize == currentFontSize)
            {
                bool same = true;
                for (int i = 0; i < current.Length; i++)
                    if (!string.Equals(values[i], current[i], StringComparison.Ordinal)) { same = false; break; }
                for (int i = 0; same && i < currentActive.Length; i++)
                    if (active[i] != currentActive[i]) same = false;
                if (same) return false;
            }
            values = (string[])current.Clone();
            active = (bool[])currentActive.Clone();
            fontSize = currentFontSize;
            return true;
        }

        public void Clear() { values = null; active = null; fontSize = int.MinValue; }
    }
}
