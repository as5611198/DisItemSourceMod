using System;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    // Own only this module's listener; never reset the shared Core event.
    internal sealed class SourceDataSubscription : IDisposable
    {
        private readonly SourceRevisionListener listener;

        public SourceDataSubscription(Action<long> handler) => listener = new SourceRevisionListener(handler,
            h => QueryApi.DataChanged += h, h => QueryApi.DataChanged -= h, () => QueryApi.DataRevision);

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
