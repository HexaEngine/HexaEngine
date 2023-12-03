namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;

    public struct ShadowAtlasDescription
    {
        public Format Format;
        public int Size;
        public int Layers;

        public ShadowAtlasDescription(Format format, int size, int layers)
        {
            Format = format;
            Size = size;
            Layers = layers;
        }
    }
}