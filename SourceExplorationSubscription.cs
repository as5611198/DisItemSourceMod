using System;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceExplorationSubscription : IDisposable
    {
        private readonly SourceRevisionListener settings;
        private readonly SourceRevisionListener discovery;
        public SourceExplorationSubscription(Action<long> changed)
        {
            settings = new SourceRevisionListener(changed, h => ExplorationApi.Changed += h, h => ExplorationApi.Changed -= h,
                () => ExplorationApi.GetSettings().Revision);
            discovery = new SourceRevisionListener(changed, h => DiscoveryApi.Changed += h, h => DiscoveryApi.Changed -= h,
                () => DiscoveryApi.Revision);
        }
        public void Enable() { settings.Enable(); discovery.Enable(); }
        public void Disable() { settings.Disable(); discovery.Disable(); }
        public void Dispose() => Disable();
    }
}
