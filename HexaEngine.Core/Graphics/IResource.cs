namespace HexaEngine.Core.Graphics
{
    using System;

    public unsafe interface IResource : IDeviceChild
    {
        public ResourceDimension Dimension { get; }

        public const int MaximumMipLevels = unchecked((int)15);
        public const int ResourceSizeInMegabytes = unchecked((int)128);
        public const int MaximumTexture1DArraySize = unchecked((int)2048);
        public const int MaximumTexture2DArraySize = unchecked((int)2048);
        public const int MaximumTexture1DSize = unchecked((int)16384);
        public const int MaximumTexture2DSize = unchecked((int)16384);
        public const int MaximumTexture3DSize = unchecked((int)2048);
        public const int MaximumTextureCubeSize = unchecked((int)16384);
    }

    public enum ResourceDimension : int
    {
        Unknown = unchecked((int)0),
        Buffer = unchecked((int)1),
        Texture1D = unchecked((int)2),
        Texture2D = unchecked((int)3),
        Texture3D = unchecked((int)4)
    }

    public enum TextureDimension : int
    {
        Unknown = unchecked((int)0),
        Texture1D = unchecked((int)1),
        Texture2D = unchecked((int)2),
        Texture3D = unchecked((int)3),
        TextureCube = unchecked((int)4)
    }
}