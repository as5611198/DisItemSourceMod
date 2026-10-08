using System;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceV3Connection : IDisposable
    {
        private const string Owner = "DisplayItemSourceMod";
        private readonly SourceRevisionListener planning;
        private readonly SourceRevisionListener hotkeys;
        private bool active;
        public bool OwnsPresenter { get; private set; }
        public SourceV3Connection(Action<long> changed)
        {
            planning = new SourceRevisionListener(changed, h => PlanningApi.Changed += h, h => PlanningApi.Changed -= h, () => PlanningApi.Revision);
            hotkeys = new SourceRevisionListener(changed, h => HotkeyApi.Changed += h, h => HotkeyApi.Changed -= h, () => HotkeyApi.Revision);
        }
        public void Enable()
        {
            if (active) return;
            active = true;
            planning.Enable();
            hotkeys.Enable();
        }
        public void SetCanPresent(bool available)
        {
            if (!active) return;
            if (available && !OwnsPresenter) OwnsPresenter = HotkeyApi.RegisterHintPresenter(Owner);
            else if (!available && OwnsPresenter)
            {
                OwnsPresenter = false;
                HotkeyApi.UnregisterHintPresenter(Owner);
            }
        }
        public void Disable()
        {
            if (!active) return;
            active = false;
            planning.Disable();
            hotkeys.Disable();
            OwnsPresenter = false;
            HotkeyApi.UnregisterHintPresenter(Owner);
        }
        public void Dispose() => Disable();
    }
}
