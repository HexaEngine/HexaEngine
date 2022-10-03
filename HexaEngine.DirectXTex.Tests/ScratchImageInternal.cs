namespace HexaEngine.DirectXTex.Tests
{
    public unsafe class ScratchImageInternal
    {
        [Fact]
        public void CreateAndFree()
        {
            ScratchImage image = new();
            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void Init()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
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

            HResult result = image.Initialize(metadata, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            Assert.Equal(metadata, image.GetMetadata());

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void Init1D()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.TEXTURE1D,
                Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
                Height = 1,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            HResult result = image.Initialize1D(Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, 64, 1, 4, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void Init2D()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
            {
                Dimension = TexDimension.TEXTURE2D,
                Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
                Width = 64,
                Height = 32,
                Depth = 1,
                ArraySize = 4,
                MipLevels = 2,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            HResult result = image.Initialize2D(Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, 64, 32, 4, 2, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void Init3D()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
            {
                Dimension = TexDimension.TEXTURE3D,
                Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
                Width = 64,
                Height = 32,
                Depth = 4,
                ArraySize = 1,
                MipLevels = 2,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            HResult result = image.Initialize3D(Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, 64, 32, 4, 2, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void InitCube()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
            {
                Dimension = TexDimension.TEXTURE2D,
                Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
                Width = 64,
                Height = 32,
                Depth = 1,
                ArraySize = 6,
                MipLevels = 2,
                MiscFlags = TexMiscFlags.TEXTURECUBE,
                MiscFlags2 = 0,
            };

            HResult result = image.InitializeCube(Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, 64, 32, 1, 2, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void OverrrideFormat()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
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

            {
                HResult result = image.Initialize(metadata, CPFlags.NONE);
                if (!result.IsSuccess)
                    result.Throw();
            }

            {
                bool result = image.OverrideFormat(Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm);
                if (!result)
                    throw new Exception();
            }

            metadata.Format = Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm;
            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void GetMetadata()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
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

            HResult result = image.Initialize(metadata, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void GetImage()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
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

            HResult result = image.Initialize(metadata, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var img = image.GetImage(0, 0, 0);

            if (img.Width != metadata.Width &&
                img.Height != metadata.Height &&
                img.Format != metadata.Format)
                Trace.Fail("img doesn't match");

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void GetImagesAndGetImageCount()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
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

            HResult result = image.Initialize(metadata, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var imgs = image.GetImages();

            for (int i = 0; i < (int)image.GetImageCount(); i++)
            {
                var img = imgs[i];
                if (img.Width != metadata.Width &&
                img.Height != metadata.Height &&
                img.Format != metadata.Format)
                    Trace.Fail("img doesn't match");
            }

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void GetPixelsAndGetPixelsSizeAndModify()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
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

            HResult result = image.Initialize(metadata, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            var pixels = image.GetPixels();
            var count = image.GetPixelsSize();
            Span<byte> data = new(pixels, (int)count);
            data.Fill(1);

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }

        [Fact]
        public void IsAlphaAllOpaque()
        {
            ScratchImage image = new();
            TexMetadata metadata = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.TEXTURE2D,
                Format = Silk.NET.DXGI.Format.FormatR16G16Float,
                Height = 64,
                Width = 64,
                MipLevels = 4,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };

            HResult result = image.Initialize(metadata, CPFlags.NONE);
            if (!result.IsSuccess)
                result.Throw();

            // Result should be true because the format contains no alpha information.
            if (!image.IsAlphaAllOpaque())
                throw new();

            var meta = image.GetMetadata();
            Assert.Equal(metadata, meta);

            image.Release();
            if (image.pScratchImage != null)
                Trace.Fail("Mem leak");
        }
    }
}