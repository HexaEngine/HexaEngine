namespace HexaEngine.DirectXTex.Tests
{
    using HexaEngine.Core.Graphics.Textures;

    public class NormalMapOperations
    {
        [Fact]
        public unsafe void ComputeNormalMap()
        {
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };
            ScratchImage image = new();
            image.Initialize(metadata, CPFlags.None);

            ScratchImage normalMap = new();
            normalMap.Initialize(metadata, CPFlags.None);

            DirectXTex.ComputeNormalMap(&image, CNMAPFlags.Default, 2, Format.FormatR8G8B8A8Unorm, &normalMap);

            image.Release();
            normalMap.Release();
        }
    }
}