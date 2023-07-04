namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Graphics;

    public class TerrainLayer
    {
        public int Index;
        public int[] Materials;
        public Texture2D Mask;

        public TerrainLayer(IGraphicsDevice device, int index)
        {
            Materials = new int[index];
            Mask = new(device, Format.R16G16B16A16Float, 1024, 1024, 1, 1, CpuAccessFlags.None);
        }
    }
}