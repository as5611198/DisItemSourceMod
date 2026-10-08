using System;
using SodaCraft.Localizations;
using UnityEngine;

namespace DisplayItemSourceMod
{
    internal sealed class SourceLanguageSubscription : IDisposable
    {
        private readonly Action<SystemLanguage> changed;
        private bool subscribed;
        private readonly SourceEnableGuard guard = new SourceEnableGuard();
        private Action<SystemLanguage>? handler;

        public SourceLanguageSubscription(Action<SystemLanguage> changed) => this.changed = changed;

        public void Enable()
        {
            if (subscribed) return;
            long owner = guard.Enable();
            handler = requested => { if (guard.IsCurrent(owner)) OnSetLanguage(requested); };
            LocalizationManager.OnSetLanguage += handler;
            subscribed = true;
            changed(LocalizationManager.CurrentLanguage);
        }

        private void OnSetLanguage(SystemLanguage requested)
        {
            // SetLanguage can select a fallback entry. Read the effective language.
            changed(LocalizationManager.CurrentLanguage);
        }

        public void Disable()
        {
            if (!subscribed) return;
            guard.Disable();
            LocalizationManager.OnSetLanguage -= handler;
            handler = null;
            subscribed = false;
        }

        public void Dispose() => Disable();
    }
}
