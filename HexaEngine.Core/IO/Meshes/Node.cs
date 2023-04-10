namespace HexaEngine.Core.IO.Meshes
{
    public struct Node
    {
        public string Name;
        public List<Node> Children;
        public Dictionary<string, string> Properties;
        public NodeFlags Flags;
        public List<string> Meshes;

        public Node(string name, NodeFlags flags)
        {
            Name = name;
            Children = new();
            Properties = new();
            Flags = flags;
            Meshes = new();
        }
    }
}