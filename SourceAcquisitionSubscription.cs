using System;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceAcquisitionSubscription : IDisposable
    {
        private readonly SourceRevisionListener listener;
        public SourceAcquisitionSubscription(Action<long> changed) => listener = new SourceRevisionListener(changed,
            h => AcquisitionApi.Changed += h, h => AcquisitionApi.Changed -= h, () => AcquisitionApi.Revision);
        public void Enable()
        {
            listener.Enable();
        }
        public void Disable()
        {
            listener.Disable();
        }
        public void Dispose() => Disable();
    }
}
