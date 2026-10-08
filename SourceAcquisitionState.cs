using DuckovCoreAPI;

namespace DisplayItemSourceMod
{
    /// <summary>Hover-local immutable definition snapshot, independent of source evidence and Planning.</summary>
    internal sealed class SourceAcquisitionState
    {
        private int typeID;
        private long generation;
        private bool active;
        public long Revision { get; private set; } = -1;
        public AcquisitionItemResult? Result { get; private set; }

        public void Begin(int id, long token)
        {
            Clear();
            typeID = id;
            generation = token;
            active = id > 0;
        }

        public bool NeedsRefresh(long revision) => active && (retry || Revision != revision);

        public void Accept(long token, AcquisitionItemResult result)
        {
            if (!active || token != generation || result.TypeID != typeID || result.Revision < Revision) return;
            Revision = result.Revision;
            Result = result.Status == AcquisitionQueryStatus.Success || result.Status == AcquisitionQueryStatus.NotReady ? result : null;
            retry = false;
        }

        public void Fail(long token)
        {
            if (!active || token != generation) return;
            Result = null;
            // Keep the revision barrier but retry on the next existing hover poll.
            retry = true;
        }

        private bool retry;

        public void Clear()
        {
            active = false;
            typeID = 0;
            generation = 0;
            Revision = -1;
            Result = null;
            retry = false;
        }
    }
}
