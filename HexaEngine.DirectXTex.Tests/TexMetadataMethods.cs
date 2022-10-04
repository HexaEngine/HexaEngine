namespace HexaEngine.DirectXTex.Tests
{
    public unsafe class TexMetadataMethods
    {
        private TexMetadata texVol = new()
        {
            ArraySize = 1,
            Depth = 6,
            Dimension = TexDimension.TEXTURE3D,
            Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
            Height = 64,
            Width = 64,
            MipLevels = 4,
            MiscFlags = 0,
            MiscFlags2 = 0,
        };

        private TexMetadata texCube = new()
        {
            ArraySize = 6,
            Depth = 1,
            Dimension = TexDimension.TEXTURE2D,
            Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
            Height = 64,
            Width = 64,
            MipLevels = 4,
            MiscFlags = TexMiscFlags.TEXTURECUBE,
            MiscFlags2 = 0,
        };

        private TexMetadata texArray = new()
        {
            ArraySize = 6,
            Depth = 1,
            Dimension = TexDimension.TEXTURE2D,
            Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
            Height = 64,
            Width = 64,
            MipLevels = 4,
            MiscFlags = 0,
            MiscFlags2 = 0,
        };

        private TexMetadata texSingle = new()
        {
            ArraySize = 1,
            Depth = 1,
            Dimension = TexDimension.TEXTURE2D,
            Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
            Height = 64,
            Width = 64,
            MipLevels = 4,
            MiscFlags = 0,
            MiscFlags2 = 0,
        };

        private TexMetadata texPMAlpha = new()
        {
            ArraySize = 1,
            Depth = 1,
            Dimension = TexDimension.TEXTURE2D,
            Format = Silk.NET.DXGI.Format.FormatBC7Unorm,
            Height = 64,
            Width = 64,
            MipLevels = 4,
            MiscFlags = 0,
            MiscFlags2 = (TexMiscFlags2)2,
        };

        [Fact]
        public void ComputeIndex()
        {
            Assert.Equal(5u, texArray.ComputeIndex(1, 1, 0));
        }

        [Fact]
        public void IsCubemap()
        {
            Assert.False(texVol.IsCubemap());
            Assert.False(!texCube.IsCubemap());
            Assert.False(texArray.IsCubemap());
            Assert.False(texSingle.IsCubemap());
            Assert.False(texPMAlpha.IsCubemap());
        }

        [Fact]
        public void IsPMAlpha()
        {
            Assert.False(texVol.IsPMAlpha());
            Assert.False(texCube.IsPMAlpha());
            Assert.False(texArray.IsPMAlpha());
            Assert.False(texSingle.IsPMAlpha());
            Assert.False(!texPMAlpha.IsPMAlpha());
        }

        [Fact]
        public void GetAndSetAlphaMode()
        {
            TexMetadata meta = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.TEXTURE2D,
                Format = Silk.NET.DXGI.Format.FormatBC7Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            meta.SetAlphaMode(TexAlphaMode.PREMULTIPLIED);
            Assert.Equal(TexAlphaMode.PREMULTIPLIED, meta.GetAlphaMode());
        }

        [Fact]
        public void IsVolumemap()
        {
            Assert.False(!texVol.IsVolumemap());
            Assert.False(texCube.IsVolumemap());
            Assert.False(texArray.IsVolumemap());
            Assert.False(texSingle.IsVolumemap());
            Assert.False(texPMAlpha.IsVolumemap());
        }
    }
}
