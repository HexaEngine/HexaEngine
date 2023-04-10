﻿namespace HexaEngine.DirectXTex.Tests
{
    using HexaEngine.Core.Graphics.Textures;

    public unsafe class TextureUtils
    {
        [Fact]
        public void FlipRotate()
        {
            ScratchImage image = new();
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

            image.Initialize(metadata, CPFlags.None);

            ScratchImage image1 = new();
            DirectXTex.FlipRotate(&image, TexFrFlags.ROTATE90, &image1);

            Assert.Equal(metadata, image.GetMetadata());
            Assert.Equal(metadata, image1.GetMetadata());

            image1.Release();
            image.Release();
        }

        [Fact]
        public void Resize()
        {
            ScratchImage image = new();
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

            TexMetadata metadata3 = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 1024,
                Width = 1024,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            image.Initialize(metadata, CPFlags.None);

            ScratchImage image1 = new();
            DirectXTex.Resize(&image, 1024, 1024, TexFilterFlags.Default, &image1);

            Assert.Equal(metadata, image.GetMetadata());
            var metadata2 = image1.GetMetadata();
            Assert.Equal(metadata3, metadata2);

            image1.Release();
            image.Release();
        }

        [Fact]
        public void Convert()
        {
            ScratchImage image = new();
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

            TexMetadata metadata3 = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR16G16B16A16Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            image.Initialize(metadata, CPFlags.None);

            ScratchImage image1 = new();
            DirectXTex.Convert(&image, Format.FormatR16G16B16A16Unorm, TexFilterFlags.Default, 0.5f, &image1);

            Assert.Equal(metadata, image.GetMetadata());
            var metadata2 = image1.GetMetadata();
            Assert.Equal(metadata3, metadata2);

            image1.Release();
            image.Release();
        }

        [Fact]
        public void ConvertToSinglePlane()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatP010,
                Height = 64,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            TexMetadata metadata3 = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatY210,
                Height = 64,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            image.Initialize(metadata, CPFlags.None);

            ScratchImage image1 = new();
            DirectXTex.ConvertToSinglePlane(&image, &image1);

            Assert.Equal(metadata, image.GetMetadata());
            var metadata2 = image1.GetMetadata();
            Assert.Equal(metadata3, metadata2);

            image1.Release();
            image.Release();
        }

        [Fact]
        public void GenerateMipMaps()
        {
            ScratchImage image = new();
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

            TexMetadata metadata3 = new()
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

            image.Initialize(metadata, CPFlags.None);

            ScratchImage image1 = new();
            DirectXTex.GenerateMipMaps(&image, TexFilterFlags.Default, 4, &image1);

            Assert.Equal(metadata, image.GetMetadata());
            var metadata2 = image1.GetMetadata();
            Assert.Equal(metadata3, metadata2);

            image1.Release();
            image.Release();
        }

        [Fact]
        public void GenerateMipMaps3D()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 6,
                Dimension = TexDimension.Texture3D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            TexMetadata metadata3 = new()
            {
                ArraySize = 1,
                Depth = 6,
                Dimension = TexDimension.Texture3D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            image.Initialize(metadata, CPFlags.None);

            ScratchImage image1 = new();
            DirectXTex.GenerateMipMaps3D(&image, TexFilterFlags.Default, 4, &image1);

            Assert.Equal(metadata, image.GetMetadata());
            var metadata2 = image1.GetMetadata();
            Assert.Equal(metadata3, metadata2);

            image1.Release();
            image.Release();
        }

        [Fact]
        public void ScaleMipMapsAlphaForCoverage()
        {
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 2048,
                Width = 2048,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            ScratchImage mipChain = new();
            mipChain.Initialize(metadata, CPFlags.None);

            var info = mipChain.GetMetadata();
            ScratchImage coverageMipChain = new();
            coverageMipChain.Initialize(info, CPFlags.None);

            for (ulong item = 0; item < info.ArraySize; ++item)
            {
                var img = mipChain.GetImage(0, item, 0);

                DirectXTex.ScaleMipMapsAlphaForCoverage(img, info.MipLevels, &info, item, 0.5f, &coverageMipChain);
            }

            coverageMipChain.Release();
            mipChain.Release();
        }

        [Fact]
        public void PremultiplyAlpha()
        {
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 2048,
                Width = 2048,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            ScratchImage srcImage = new();
            srcImage.Initialize(metadata, CPFlags.None);

            ScratchImage destImage = new();

            DirectXTex.PremultiplyAlpha(&srcImage, TexPmAlphaFlags.DEFAULT, &destImage);

            srcImage.Release();
            destImage.Release();
        }

        [Fact]
        public void Compress()
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

            ScratchImage srcImage = new();
            srcImage.Initialize(metadata, CPFlags.None);

            ScratchImage destImage = new();

            DirectXTex.Compress(&srcImage, Format.FormatBC7Unorm, TexCompressFlags.BC7Quick | TexCompressFlags.Parallel, 0.5f, &destImage);

            srcImage.Release();
            destImage.Release();
        }

        [Fact]
        public void Decompress()
        {
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatBC7Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            ScratchImage srcImage = new();
            srcImage.Initialize(metadata, CPFlags.None);

            ScratchImage destImage = new();

            DirectXTex.Decompress(&srcImage, Format.FormatR8G8B8A8Unorm, &destImage);

            srcImage.Release();
            destImage.Release();
        }
    }
}