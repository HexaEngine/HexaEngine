namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;

    public struct ShadowAtlasDescription
    {
        public Format Format;
        public int Size;
        public int Layers;
        public int ArraySize;

        public ShadowAtlasDescription(Format format, int size, int layers, int arraySize)
        {
            Format = format;
            Size = size;
            Layers = layers;
            ArraySize = arraySize;
        }
    }
}