using System;
using System.Collections.Generic;
using UnityEngine;

namespace DisplayItemSourceMod
{
    // Source-owned keys only; queued removals finish before translated replacements are added.
    internal sealed class SourceSettingsRegistration
    {
        private static readonly string[] Keys = { SourceV3Preferences.FontKey, SourceV3Preferences.DetailKey, SourceV3Preferences.ModOnlyKey };
        private readonly SourceV3Preferences preferences;
        private readonly Func<SystemLanguage> language;
        private readonly Func<string, Type, (bool Found, object? Value)> saved;
        private readonly Func<string, string, object, Action<object>, bool> add;
        private readonly Func<string, Action<bool>, bool> remove;
        private readonly Action changed;
        private readonly HashSet<string> restored = new HashSet<string>(), registered = new HashSet<string>(),
            dirty = new HashSet<string>(), pending = new HashSet<string>();
        private bool active = true;
        private int epoch;

        public SourceSettingsRegistration(SourceV3Preferences preferences, Func<SystemLanguage> language,
            Func<string, Type, (bool, object?)> saved, Func<string, string, object, Action<object>, bool> add,
            Func<string, Action<bool>, bool> remove, Action changed)
        { this.preferences=preferences; this.language=language; this.saved=saved; this.add=add; this.remove=remove; this.changed=changed; }

        public void TryRegister()
        {
            if (!active) return;
            foreach (string key in Keys)
            {
                if (pending.Contains(key)) continue;
                if (dirty.Contains(key))
                {
                    pending.Add(key); int token = epoch;
                    if (!remove(key, _ =>
                    {
                        if (!active || token != epoch) return;
                        pending.Remove(key); dirty.Remove(key); registered.Remove(key); TryRegister();
                    })) pending.Remove(key);
                    continue;
                }
                if (registered.Contains(key)) continue;
                if (!restored.Contains(key))
                {
                    var value = saved(key, key == SourceV3Preferences.FontKey ? typeof(int) : typeof(bool));
                    if (value.Found) { preferences.Set(key, value.Value); changed(); }
                    restored.Add(key);
                }
                int callbackEpoch = epoch;
                if (add(key, SourceV3Text.Label(key, language()), preferences.Value(key), value =>
                {
                    if (!active || callbackEpoch != epoch) return;
                    preferences.Set(key, value); changed();
                })) registered.Add(key);
            }
        }
        public void RefreshLanguage()
        { if (!active) return; foreach (string key in registered) dirty.Add(key); TryRegister(); }
        public void ProviderLost()
        { epoch++; registered.Clear(); pending.Clear(); dirty.Clear(); restored.Clear(); }
        public void Deactivate()
        {
            if (!active) return;
            active = false; epoch++;
            foreach (string key in registered) remove(key, _ => { });
            registered.Clear(); pending.Clear(); dirty.Clear();
        }
    }
}
