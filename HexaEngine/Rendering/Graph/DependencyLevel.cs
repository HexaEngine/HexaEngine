namespace HexaEngine.Rendering.Graph
{
    public class DependencyLevel
    {
        public List<Node> Nodes { get; } = new();

        public List<List<Node>> NodesPerQueue { get; } = new();

        public ulong LevelIndex { get; internal set; } = 0;

        public HashSet<ulong> QueuesInvoledInCrossQueueResourceReads { get; } = new();

        public HashSet<SubresourceName> SubresourcesReadByMultipleQueues { get; } = new();

        public void AddNode(Node node)
        {
            Nodes.Add(node);
        }

        public Node RemoveNode(Node node)
        {
            Nodes.Remove(node);
            return node;
        }
    }
}