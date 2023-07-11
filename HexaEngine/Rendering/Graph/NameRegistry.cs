namespace HexaEngine.Rendering.Graph
{
    public class NameRegistry
    {
        public const uint InvalidID = uint.MaxValue;
        private readonly List<string> idToName = new();
        private readonly Dictionary<string, uint> nameToId = new();

        public static NameRegistry Shared { get; } = new();

        public uint ToId(string name)
        {
            if (nameToId.TryGetValue(name, out uint id))
            {
                return id;
            }

            idToName.Add(name);
            nameToId.Add(name, (uint)(idToName.Count - 1));

            return (uint)(idToName.Count - 1);
        }

        public string GetName(uint id)
        {
            return idToName[(int)id];
        }
    }
}