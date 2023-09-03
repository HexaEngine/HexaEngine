namespace HexaEngine.Rendering.Graph
{
    public class RenderPassMetadata
    {
        public RenderPassMetadata(string name, RenderPassType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public RenderPassType Type { get; }
    }
}