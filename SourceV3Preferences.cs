using System;

namespace DisplayItemSourceMod
{
    internal sealed class SourceV3Preferences
    {
        public const string FontKey = "source_font_size";
        public const string DetailKey = "source_detailed";
        public const string ModOnlyKey = "source_mod_items_only";
        public int FontSize { get; private set; } = 20;
        public bool Detailed { get; private set; }
        public bool ModOnly { get; private set; }
        public object Value(string key) => key == FontKey ? (object)FontSize : key == DetailKey ? Detailed : ModOnly;
        public void Set(string key, object? value)
        {
            if (key == FontKey && value is int size) FontSize = Math.Max(14, Math.Min(32, size));
            else if (key == DetailKey && value is bool detailed) Detailed = detailed;
            else if (key == ModOnlyKey && value is bool only) ModOnly = only;
        }
    }
}
