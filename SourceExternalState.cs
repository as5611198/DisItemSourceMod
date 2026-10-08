using System.Threading;
using DuckovCoreAPI;
namespace DisplayItemSourceMod
{
    internal sealed class SourceExternalState
    {
        private int typeID, scene, thread;
        private long token, revision = -1, epoch = -1, catalog = -1;
        private bool active, retry;
        private ExternalAcquisitionItemResult? result;
        public void Begin(int id, long generation, int currentScene)
        { Clear(); typeID = id; token = generation; scene = currentScene; thread = Thread.CurrentThread.ManagedThreadId; active = id > 0; }
        private bool Current(long generation, int currentScene) => active && token == generation && scene == currentScene && Thread.CurrentThread.ManagedThreadId == thread;
        public bool NeedsRefresh(long generation, int currentScene, long currentRevision, long currentEpoch, long currentCatalog) =>
            Current(generation, currentScene) && (retry || revision != currentRevision || epoch != currentEpoch || catalog != currentCatalog);
        public ExternalAcquisitionItemResult? Get(long generation, int currentScene, long currentRevision, long currentEpoch, long currentCatalog) =>
            Current(generation, currentScene) && revision == currentRevision && epoch == currentEpoch && catalog == currentCatalog ? result : null;
        public void Accept(long generation, int currentScene, long currentRevision, long currentEpoch, long currentCatalog,
            string nameKey, ItemSourceInfo source, ExternalAcquisitionItemResult value)
        {
            if (!Current(generation, currentScene)) return;
            if (value.TypeID != typeID || value.Revision != currentRevision || value.Epoch != currentEpoch || value.Revision < revision || value.Epoch < epoch || currentCatalog < catalog ||
                (value.Status == ExternalAcquisitionStatus.Success && value.CatalogRevision != currentCatalog)) { Invalidate(generation, currentScene); return; }
            if (value.Status == ExternalAcquisitionStatus.Success)
                foreach (var entry in value.Entries)
                    if (source.TypeID != typeID || entry.Definition.TypeID != typeID || string.IsNullOrEmpty(nameKey) || entry.ItemNameKey != nameKey ||
                        entry.ItemOwnerID != source.ModId || entry.ItemResourceID != source.ResourceIdentifier || entry.ItemEvidence != source.Evidence)
                    { Invalidate(generation, currentScene); return; }
            revision = currentRevision; epoch = currentEpoch; catalog = currentCatalog; retry = false; result = value;
        }
        public void Invalidate(long generation, int currentScene)
        { if (!Current(generation, currentScene)) return; result = null; retry = true; }
        public void Fail(long generation, int currentScene) => Invalidate(generation, currentScene);
        public void Clear()
        { active = retry = false; typeID = scene = thread = 0; token = 0; revision = epoch = catalog = -1; result = null; }
    }
}
