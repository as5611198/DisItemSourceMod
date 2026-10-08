using System.Threading;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    // Main-thread hover cache for the V14 native clue APIs. Detailed rows are
    // deliberately held separately so turning the Core presentation toggle off
    // immediately drops them without affecting the compact clue summary.
    internal sealed class SourceNativeAcquisitionState
    {
        private int typeID, scene, thread;
        private long token, lootRevision = -1, acquisitionRevision = -1, presentationRevision = -1;
        private long epoch = -1, catalog = -1;
        private bool active, retry, detailedLoot, detailedShops;
        private NativeLootItemResult? loot;
        private MerchantClueResult? merchants;
        private NativeLootDetailResult? details;
        private AcquisitionPresentationSettings? settings;

        public void Begin(int id, long generation, int currentScene)
        {
            Clear(); typeID = id; token = generation; scene = currentScene;
            thread = Thread.CurrentThread.ManagedThreadId; active = id > 0;
        }

        private bool Current(long generation, int currentScene) => active && token == generation &&
            scene == currentScene && Thread.CurrentThread.ManagedThreadId == thread;

        public bool NeedsRefresh(long generation, int currentScene, long currentLootRevision,
            long currentAcquisitionRevision, long currentPresentationRevision, long currentEpoch,
            long currentCatalog, bool currentDetailedLoot, bool currentDetailedShops) =>
            Current(generation, currentScene) && (retry || lootRevision != currentLootRevision ||
                acquisitionRevision != currentAcquisitionRevision || presentationRevision != currentPresentationRevision ||
                epoch != currentEpoch || catalog != currentCatalog || detailedLoot != currentDetailedLoot ||
                detailedShops != currentDetailedShops);

        public NativeLootItemResult? Loot => loot;
        public MerchantClueResult? Merchants => merchants;
        public NativeLootDetailResult? Details => details;
        public AcquisitionPresentationSettings? Settings => settings;

        public void Accept(long generation, int currentScene, long currentLootRevision,
            long currentAcquisitionRevision, long currentPresentationRevision, long currentEpoch,
            long currentCatalog, bool currentDetailedLoot, bool currentDetailedShops,
            NativeLootItemResult currentLoot, MerchantClueResult currentMerchants,
            NativeLootDetailResult? currentDetails, AcquisitionPresentationSettings currentSettings)
        {
            if (!Current(generation, currentScene)) return;
            if (currentLoot.TypeID != typeID || currentMerchants.TypeID != typeID ||
                currentLoot.Revision != currentLootRevision || currentLoot.Epoch != currentEpoch ||
                currentLoot.CatalogRevision != currentCatalog || currentMerchants.Revision != currentLootRevision ||
                currentMerchants.AcquisitionRevision != currentAcquisitionRevision ||
                currentSettings.Revision != currentPresentationRevision || currentCatalog < catalog || currentEpoch < epoch ||
                (currentDetails != null && (currentDetails.TypeID != typeID || currentDetails.Revision != currentLootRevision ||
                    currentDetails.Epoch != currentEpoch || currentDetails.CatalogRevision != currentCatalog ||
                    currentDetails.PresentationRevision != currentPresentationRevision)))
            { Invalidate(generation, currentScene); return; }
            lootRevision = currentLootRevision; acquisitionRevision = currentAcquisitionRevision;
            presentationRevision = currentPresentationRevision; epoch = currentEpoch; catalog = currentCatalog;
            detailedLoot = currentDetailedLoot; detailedShops = currentDetailedShops; retry = false;
            loot = currentLoot; merchants = currentMerchants; settings = currentSettings;
            details = currentDetailedLoot ? currentDetails : null;
        }

        public void Invalidate(long generation, int currentScene)
        {
            if (!Current(generation, currentScene)) return;
            loot = null; merchants = null; details = null; settings = null; retry = true;
        }

        public void Fail(long generation, int currentScene) => Invalidate(generation, currentScene);

        public void Clear()
        {
            active = retry = detailedLoot = detailedShops = false;
            typeID = scene = thread = 0; token = 0;
            lootRevision = acquisitionRevision = presentationRevision = epoch = catalog = -1;
            loot = null; merchants = null; details = null; settings = null;
        }
    }
}
