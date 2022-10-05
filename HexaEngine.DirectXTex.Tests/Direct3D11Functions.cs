namespace HexaEngine.DirectXTex.Tests
{
    using HexaEngine.D3D11;
    using Silk.NET.Direct3D11;
    using System;

    public unsafe class Direct3D11Functions : IDisposable
    {
        private D3D11GraphicsDevice graphicsDevice;
        private ID3D11Device* device;
        private ID3D11DeviceContext* context;

        public Direct3D11Functions()
        {
            graphicsDevice = new(new(), null);
            device = (ID3D11Device*)graphicsDevice.Device;
            context = (ID3D11DeviceContext*)graphicsDevice.DeviceContext;
        }

        [Fact]
        public void IsSupportedTexture()
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
            Assert.True(DirectXTex.IsSupportedTexture(device, &metadata));
        }

        [Fact]
        public void CreateTexture()
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
            ScratchImage image = new();
            image.Initialize(metadata, CPFlags.None);
            ID3D11Resource* resource;
            DirectXTex.CreateTexture(device, image.GetImages(), image.GetImageCount(), &metadata, &resource);
            if (resource == null)
                Assert.Fail("Fail");
            resource->Release();
        }

        [Fact]
        public void CreateShaderResourceView()
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
            ScratchImage image = new();
            image.Initialize(metadata, CPFlags.None);
            ID3D11ShaderResourceView* srv;
            DirectXTex.CreateShaderResourceView(device, image.GetImages(), image.GetImageCount(), &metadata, &srv);
            if (srv == null)
                Assert.Fail("Fail");
            srv->Release();
        }

        [Fact]
        public void CreateTextureEx()
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
            ScratchImage image = new();
            image.Initialize(metadata, CPFlags.None);
            ID3D11Resource* resource;
            DirectXTex.CreateTextureEx(device, image.GetImages(), image.GetImageCount(), &metadata, Usage.Immutable, BindFlag.ShaderResource, CpuAccessFlag.None, ResourceMiscFlag.None, false, &resource);
            if (resource == null)
                Assert.Fail("Fail");
            resource->Release();
        }

        [Fact]
        public void CreateShaderResourceViewEx()
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
            ScratchImage image = new();
            image.Initialize(metadata, CPFlags.None);
            ID3D11ShaderResourceView* srv;
            DirectXTex.CreateShaderResourceViewEx(device, image.GetImages(), image.GetImageCount(), &metadata, Usage.Immutable, BindFlag.ShaderResource, CpuAccessFlag.None, ResourceMiscFlag.None, false, &srv);
            if (srv == null)
                Assert.Fail("Fail");
            srv->Release();
        }

        [Fact]
        public void CaptureTexture()
        {
            ID3D11Resource* resource;
            Texture2DDesc desc = new(64, 64, 1, 1, Format.FormatR8G8B8A8Unorm, new(1, 0), Usage.Default, 8, 0, 0);
            device->CreateTexture2D(&desc, null, (ID3D11Texture2D**)&resource);

            ScratchImage image = new();
            DirectXTex.CaptureTexture(device, context, resource, &image);

            resource->Release();

            TexMetadata metadata = image.GetMetadata();

            Assert.Equal(desc.Height, metadata.Height);
            Assert.Equal(desc.Width, metadata.Width);
            Assert.Equal(desc.ArraySize, metadata.ArraySize);
            Assert.Equal(desc.Format, metadata.Format);
            Assert.Equal((int)desc.MiscFlags, (int)metadata.MiscFlags);
            Assert.Equal(desc.MipLevels, metadata.MipLevels);

            image.Release();
        }

        public void Dispose()
        {
            graphicsDevice.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}