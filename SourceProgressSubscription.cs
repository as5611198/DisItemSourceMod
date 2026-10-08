using System;
using System.Threading;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceEnableGuard
    {
        private long generation;
        private int thread;
        private bool enabled;
        public long Enable() { thread = Thread.CurrentThread.ManagedThreadId; enabled = true; return ++generation; }
        public bool IsCurrent(long owner) => enabled && generation == owner && Thread.CurrentThread.ManagedThreadId == thread;
        public void Disable() { enabled = false; generation++; }
    }

    /// <summary>A fresh callback identity for each enable, including captured notification batches.</summary>
    internal sealed class SourceRevisionListener : IDisposable
    {
        private readonly Action<long> changed;
        private readonly Action<Action<long>> add;
        private readonly Action<Action<long>> remove;
        private readonly Func<long> currentRevision;
        private bool enabled;
        private long generation;
        private Action<long>? handler;

        public SourceRevisionListener(Action<long> changed, Action<Action<long>> add, Action<Action<long>> remove, Func<long> currentRevision)
        { this.changed = changed; this.add = add; this.remove = remove; this.currentRevision = currentRevision; }

        public void Enable()
        {
            if (enabled) return;
            long owner = ++generation;
            int thread = Thread.CurrentThread.ManagedThreadId;
            enabled = true;
            handler = revision =>
            {
                if (enabled && owner == generation && Thread.CurrentThread.ManagedThreadId == thread && revision == currentRevision()) changed(revision);
            };
            add(handler);
        }
        public void Notify(long revision) => handler?.Invoke(revision);
        public void Disable()
        {
            if (!enabled) return;
            enabled = false;
            generation++;
            var previous = handler;
            handler = null;
            if (previous != null) remove(previous);
        }
        public void Dispose() => Disable();
    }

    internal sealed class SourceProgressSubscription : IDisposable
    {
        private readonly SourceRevisionListener wishlist;
        private readonly SourceRevisionListener discovery;
        public SourceProgressSubscription(Action<long> changed)
        {
            wishlist = new SourceRevisionListener(changed, h => WishlistApi.Changed += h, h => WishlistApi.Changed -= h, () => WishlistApi.Revision);
            discovery = new SourceRevisionListener(changed, h => DiscoveryApi.Changed += h, h => DiscoveryApi.Changed -= h, () => DiscoveryApi.Revision);
        }
        public void Enable() { wishlist.Enable(); discovery.Enable(); }
        public void Disable() { wishlist.Disable(); discovery.Disable(); }
        public void Dispose() => Disable();
    }
}
