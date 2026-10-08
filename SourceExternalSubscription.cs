using System;
using System.Threading;
using DuckovCoreAPI;
namespace DisplayItemSourceMod
{
    internal sealed class SourceExternalSubscription : IDisposable
    {
        private readonly SourceRevisionListener listener;
        public SourceExternalSubscription(Action<long> changed) => listener = new SourceRevisionListener(changed,
            h => ExternalAcquisitionApi.Changed += h, h => ExternalAcquisitionApi.Changed -= h, () => ExternalAcquisitionApi.Revision);
        public void Enable() => listener.Enable();
        public void Disable() => listener.Disable();
        public void Dispose() => Disable();
    }
    internal sealed class SourceExternalRefreshQueue
    {
        private int thread, pending;
        private bool enabled;
        public void Enable() { thread = Thread.CurrentThread.ManagedThreadId; pending = 0; enabled = true; }
        public void Disable() { enabled = false; pending = 0; }
        private bool Current => enabled && Thread.CurrentThread.ManagedThreadId == thread;
        public void MarkQuery() { if (Current) pending = 2; }
        public void MarkExternal() { if (Current && pending == 0) pending = 1; }
        public int Take() { if (!Current) return 0; int value = pending; pending = 0; return value; }
        public void Clear() { pending = 0; }
    }
}
