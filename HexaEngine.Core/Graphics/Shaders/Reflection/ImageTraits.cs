namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    public struct ImageTraits
    {
        public Dimension Dim;
        public uint Depth;
        public bool Arrayed;
        public bool Ms;
        public bool Sampled;
        public ImageFormat ImageFormat;
    }
}