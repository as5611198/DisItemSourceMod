using System.Threading;
using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    internal sealed class SourceProgressState
    {
        private int typeID = -1, scene, thread;
        private long generation, wishlistRevision = -1, discoveryRevision = -1, planningRevision = -1, catalogRevision = -1;
        private long wishlistMinimum, discoveryMinimum;
        private bool active, wishlistRetry, discoveryRetry;
        private InventoryScope scope;
        private WishlistItemResult? wishlist;
        private DiscoveryItemResult? discovery;

        public void Begin(int id, long token, int currentScene)
        {
            Clear(); typeID = id; generation = token; scene = currentScene;
            thread = Thread.CurrentThread.ManagedThreadId; active = id >= 0;
        }
        private bool Current(long token, int currentScene) => active && token == generation && scene == currentScene && Thread.CurrentThread.ManagedThreadId == thread;
        public WishlistItemResult? GetWishlist(long token, int currentScene, InventoryScope expectedScope) => Current(token, currentScene) && scope == expectedScope ? wishlist : null;
        public DiscoveryItemResult? GetDiscovery(long token, int currentScene) => Current(token, currentScene) ? discovery : null;
        public bool NeedsWishlistRefresh(long token, int currentScene, InventoryScope expectedScope, long revision, long currentPlanning) =>
            Current(token, currentScene) && (wishlistRetry || scope != expectedScope || wishlistRevision != revision || planningRevision != currentPlanning);
        public bool NeedsDiscoveryRefresh(long token, int currentScene, long revision, long currentCatalog) =>
            Current(token, currentScene) && (discoveryRetry || discoveryRevision != revision || catalogRevision != currentCatalog);

        public void AcceptWishlist(long token, int currentScene, InventoryScope expectedScope, long currentPlanning, WishlistItemResult value)
        {
            if (!Current(token, currentScene) || value.TypeID != typeID || value.Scope != expectedScope || value.Revision < wishlistRevision || value.Revision < wishlistMinimum) return;
            wishlistRevision = value.Revision; planningRevision = currentPlanning; scope = expectedScope; wishlistRetry = false;
            wishlist = value.Status == WishlistQueryStatus.Success || value.Status == WishlistQueryStatus.NotReady || value.Status == WishlistQueryStatus.ContextChanged ? value : null;
            if (value.Status == WishlistQueryStatus.ContextChanged)
            {
                // Either service's world gate invalidates both rows. The other
                // service must advance before its old success can return.
                discovery = null; discoveryRetry = true;
                discoveryMinimum = System.Math.Max(discoveryMinimum, discoveryRevision + 1);
            }
        }
        public void AcceptDiscovery(long token, int currentScene, long currentCatalog, DiscoveryItemResult value)
        {
            if (!Current(token, currentScene) || value.TypeID != typeID || value.Revision < discoveryRevision || value.Revision < discoveryMinimum) return;
            discoveryRevision = value.Revision; catalogRevision = currentCatalog; discoveryRetry = false;
            discovery = value.Status == DiscoveryQueryStatus.WrongThread || value.Status == DiscoveryQueryStatus.InvalidRequest ? null : value;
            if (value.Status == DiscoveryQueryStatus.ContextChanged)
            {
                wishlist = null; wishlistRetry = true;
                wishlistMinimum = System.Math.Max(wishlistMinimum, wishlistRevision + 1);
            }
        }
        public void FailWishlist(long token, int currentScene)
        { if (!Current(token, currentScene)) return; wishlist = null; wishlistRetry = true; }
        public void FailDiscovery(long token, int currentScene)
        { if (!Current(token, currentScene)) return; discovery = null; discoveryRetry = true; }
        public void Clear()
        {
            active = wishlistRetry = discoveryRetry = false; typeID = -1; scene = thread = 0; generation = 0;
            wishlistRevision = discoveryRevision = planningRevision = catalogRevision = -1;
            wishlistMinimum = discoveryMinimum = 0; scope = InventoryScope.Carried; wishlist = null; discovery = null;
        }
    }
}
