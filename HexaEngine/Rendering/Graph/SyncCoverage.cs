namespace HexaEngine.Rendering.Graph
{
    public struct SyncCoverage
    {
        public Node NodeToSyncWith = null;
        public ulong NodeToSyncWithIndex = 0;
        public List<ulong> SyncedQueueIndices = new();

        public SyncCoverage()
        {
        }

        public SyncCoverage(Node nodeToSyncWith, ulong nodeToSyncWithIndex, List<ulong> syncedQueueIndices)
        {
            NodeToSyncWith = nodeToSyncWith;
            NodeToSyncWithIndex = nodeToSyncWithIndex;
            SyncedQueueIndices = syncedQueueIndices;
        }
    }
}