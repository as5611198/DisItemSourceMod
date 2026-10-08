using System;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceDiagnosticsSubscription : IDisposable
    {
        private readonly SourceRevisionListener listener;
        public SourceDiagnosticsSubscription(Action<long> changed) => listener = new SourceRevisionListener(changed,
            h => DiagnosticsApi.Changed += h, h => DiagnosticsApi.Changed -= h, () => DiagnosticsApi.Revision);
        public void Enable()
        {
            listener.Enable();
        }
        public void OnChanged(long revision)
        {
            // Never touch Unity from a worker or a notification batch after stop.
            listener.Notify(revision);
        }
        public void Disable()
        {
            listener.Disable();
        }
        public void Dispose() => Disable();
    }
}
