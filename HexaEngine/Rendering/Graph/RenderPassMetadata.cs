namespace HexaEngine.Rendering.Graph
{
    public struct RenderPassMetadata
    {
        public RenderPassMetadata(Name name, RenderPassPurpose purpose)
        {
            Name = name;
            Purpose = purpose;
        }

        public Name Name;
        public RenderPassPurpose Purpose;
    }
}