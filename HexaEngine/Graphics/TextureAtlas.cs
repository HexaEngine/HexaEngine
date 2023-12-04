namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Lights;
    using System;

    public class TextureAtlas
    {
        private BinaryTreeBinPacking binPacking;

        public TextureAtlas(IGraphicsDevice device, int width, int height, Format format, CpuAccessFlags cpuAccessFlags)
        {
            binPacking = new(width, height);
        }

        public TextureAtlasHandle Alloc()
        {
            throw new NotImplementedException();
        }

        public void Free(ref TextureAtlasHandle handle)
        {
            throw new NotImplementedException();
        }
    }
}