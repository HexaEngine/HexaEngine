namespace HexaEngine.DirectXTex.Tests
{
    using HexaEngine.Core.Graphics.Textures;

    public unsafe class DDSHelperFunctions
    {
        [Fact]
        public void EncodeDDSHeader()
        {
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            byte[] data = new byte[8192];
            ulong required;
            fixed (byte* ptr = data)
            {
                DirectXTex.EncodeDDSHeader(&metadata, DDSFlags.None, ptr, 8192, &required);
            }
        }
    }
}