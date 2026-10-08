using System;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceNativeAcquisitionSubscription : IDisposable
    {
        private readonly SourceRevisionListener loot;
        private readonly SourceRevisionListener presentation;

        public SourceNativeAcquisitionSubscription(Action<long> changed)
        {
            loot = new SourceRevisionListener(changed, h => NativeLootApi.Changed += h,
                h => NativeLootApi.Changed -= h, () => NativeLootApi.Revision);
            presentation = new SourceRevisionListener(changed, h => AcquisitionPresentationApi.Changed += h,
                h => AcquisitionPresentationApi.Changed -= h, () => AcquisitionPresentationApi.GetSettings().Revision);
        }

        public void Enable() { loot.Enable(); presentation.Enable(); }
        public void Disable() { loot.Disable(); presentation.Disable(); }
        public void Dispose() => Disable();
    }
}
