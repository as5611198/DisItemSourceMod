using System.Threading;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceDiagnosticsState
    {
        private int typeID = -1;
        private int scene;
        private int thread;
        private long generation;
        private long revision = -1;
        private bool active;
        private bool retry;
        private DiagnosticQueryResult? result;

        public void Begin(int id, long token, int currentScene)
        {
            Clear();
            typeID = id;
            generation = token;
            scene = currentScene;
            thread = Thread.CurrentThread.ManagedThreadId;
            active = id >= 0;
        }

        private bool IsCurrent(long token, int currentScene) => active && token == generation && currentScene == scene && Thread.CurrentThread.ManagedThreadId == thread;
        public DiagnosticQueryResult? GetResult(long token, int currentScene) => IsCurrent(token, currentScene) ? result : null;
        public bool NeedsRefresh(long token, int currentScene, long currentRevision) => IsCurrent(token, currentScene) && (retry || revision != currentRevision);

        public void Accept(long token, int currentScene, DiagnosticQueryResult value)
        {
            if (!IsCurrent(token, currentScene) || value.FilterTypeID != typeID || value.Revision < revision) return;
            revision = value.Revision;
            result = value.Status == DiagnosticQueryStatus.Success || value.Status == DiagnosticQueryStatus.NotReady ? value : null;
            retry = false;
        }

        public void Fail(long token, int currentScene)
        {
            if (!IsCurrent(token, currentScene)) return;
            result = null;
            retry = true;
        }

        public void Clear()
        {
            active = false;
            typeID = -1;
            generation = 0;
            scene = thread = 0;
            revision = -1;
            result = null;
            retry = false;
        }
    }
}
