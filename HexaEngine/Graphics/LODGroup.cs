namespace HexaEngine.Graphics
{
    public class LODGroup
    {
        private LODFadeMode fadeMode;

        public LODGroup(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public LODFadeMode FadeMode { get => fadeMode; set => fadeMode = value; }

        public List<ILODRendererComponent> Renderers { get; } = [];
    }
}