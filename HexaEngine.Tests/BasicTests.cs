namespace HexaEngine.Tests
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.D3D11;
    using System;
    using System.Numerics;

    public class DeviceTests
    {
        private IGraphicsDevice device;

        [SetUp]
        public void Setup()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Fail("Only windows is supported");
                return;
            }
            device = new D3D11GraphicsDevice(null);
        }

        [Test]
        public void BlendStateTest()
        {
            IBlendState nonPremultiplied = device.CreateBlendState(BlendDescription.NonPremultiplied);
            IBlendState opaque = device.CreateBlendState(BlendDescription.Opaque);
            IBlendState additive = device.CreateBlendState(BlendDescription.Additive);
            IBlendState alphaBlend = device.CreateBlendState(BlendDescription.AlphaBlend);
            nonPremultiplied.Dispose();
            opaque.Dispose();
            additive.Dispose();
            alphaBlend.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public unsafe void BufferTest()
        {
            {
                IBuffer vb = device.CreateBuffer(1, BindFlags.VertexBuffer);
                IBuffer ib = device.CreateBuffer(1, BindFlags.IndexBuffer);
                IBuffer cb = device.CreateBuffer(Matrix4x4.Identity, BindFlags.ConstantBuffer);
                vb.Dispose();
                ib.Dispose();
                cb.Dispose();
            }
            {
                IBuffer vb = device.CreateBuffer(new int[] { 1, 1 }, BindFlags.VertexBuffer);
                IBuffer ib = device.CreateBuffer(new int[] { 1, 1 }, BindFlags.IndexBuffer);
                IBuffer cb = device.CreateBuffer(new Vector4[] { new Vector4(1, 1, 1, 1), new Vector4(2, 2, 2, 2) }, BindFlags.ConstantBuffer);
                vb.Dispose();
                ib.Dispose();
                cb.Dispose();
            }
            {
                IBuffer vb = device.CreateBuffer(new(4, BindFlags.VertexBuffer));
                IBuffer ib = device.CreateBuffer(new(4, BindFlags.IndexBuffer));
                IBuffer cb = device.CreateBuffer(new(16, BindFlags.ConstantBuffer));
                vb.Dispose();
                ib.Dispose();
                cb.Dispose();
            }
            {
                IBuffer vb = device.CreateBuffer(1, new(4, BindFlags.VertexBuffer));
                IBuffer ib = device.CreateBuffer(1, new(4, BindFlags.IndexBuffer));
                IBuffer cb = device.CreateBuffer(new Vector4(), new(16, BindFlags.ConstantBuffer));
                vb.Dispose();
                ib.Dispose();
                cb.Dispose();
            }
            {
                IBuffer vb = device.CreateBuffer(new int[] { 1, 1 }, new(4 * 2, BindFlags.VertexBuffer));
                IBuffer ib = device.CreateBuffer(new int[] { 1, 1 }, new(4 * 2, BindFlags.IndexBuffer));
                IBuffer cb = device.CreateBuffer(new Vector4[] { new Vector4(1, 1, 1, 1), new Vector4(2, 2, 2, 2) }, new(16 * 2, BindFlags.ConstantBuffer));
                vb.Dispose();
                ib.Dispose();
                cb.Dispose();
            }
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public unsafe void BufferWriteTest()
        {
            IBuffer vb = device.CreateBuffer(1, BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            IBuffer ib = device.CreateBuffer(1, BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            IBuffer cb = device.CreateBuffer(Matrix4x4.Identity, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            device.Context.Write(vb, 2);
            device.Context.Write(ib, 2);
            device.Context.Write(cb, Matrix4x4.CreateTranslation(1, 1, 1));
            vb.Dispose();
            ib.Dispose();
            cb.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void DepthStencilStateTest()
        {
            IDepthStencilState @default = device.CreateDepthStencilState(DepthStencilDescription.Default);
            IDepthStencilState none = device.CreateDepthStencilState(DepthStencilDescription.None);
            IDepthStencilState read = device.CreateDepthStencilState(DepthStencilDescription.DepthRead);
            IDepthStencilState reverse = device.CreateDepthStencilState(DepthStencilDescription.DepthReverseZ);
            IDepthStencilState readReverse = device.CreateDepthStencilState(DepthStencilDescription.DepthReadReverseZ);
            @default.Dispose();
            none.Dispose();
            read.Dispose();
            reverse.Dispose();
            readReverse.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void RasterizerStateTest()
        {
            IRasterizerState wireframe = device.CreateRasterizerState(RasterizerDescription.Wireframe);
            IRasterizerState cullfront = device.CreateRasterizerState(RasterizerDescription.CullFront);
            IRasterizerState cullback = device.CreateRasterizerState(RasterizerDescription.CullBack);
            IRasterizerState cullnone = device.CreateRasterizerState(RasterizerDescription.CullNone);
            wireframe.Dispose();
            cullfront.Dispose();
            cullback.Dispose();
            cullnone.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void TextureTest()
        {
            {
                ITexture1D texture1D = device.CreateTexture1D(Format.R32Float, IResource.MaximumTexture1DSize, 1, 1, null, BindFlags.ShaderResource, ResourceMiscFlag.None);
                texture1D.Dispose();
                ITexture2D texture2D = device.CreateTexture2D(Format.R32Float, IResource.MaximumTexture2DSize, IResource.MaximumTexture2DSize, 1, 1, null, BindFlags.ShaderResource, ResourceMiscFlag.None);
                texture2D.Dispose();
                ITexture3D texture3D = device.CreateTexture3D(Format.R8UInt, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, 1, null, BindFlags.ShaderResource, ResourceMiscFlag.None);
                texture3D.Dispose();
            }
            {
                ITexture1D texture1D = device.CreateTexture1D(Format.R32Float, IResource.MaximumTexture1DSize, 1, 1, null, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None);
                texture1D.Dispose();
                ITexture2D texture2D = device.CreateTexture2D(Format.R32Float, IResource.MaximumTexture2DSize, IResource.MaximumTexture2DSize, 1, 1, null, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, 1, 0, ResourceMiscFlag.None);
                texture2D.Dispose();
                ITexture3D texture3D = device.CreateTexture3D(Format.R8UInt, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, 1, null, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None);
                texture3D.Dispose();
            }
            {
                ITexture1D texture1D = device.CreateTexture1D(new(Format.R32Float, IResource.MaximumTexture1DSize, 1, 1, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None));
                texture1D.Dispose();
                ITexture2D texture2D = device.CreateTexture2D(new(Format.R32Float, IResource.MaximumTexture2DSize, IResource.MaximumTexture2DSize, 1, 1, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, 1, 0, ResourceMiscFlag.None));
                texture2D.Dispose();
                ITexture3D texture3D = device.CreateTexture3D(new(Format.R8UInt, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, 1, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None));
                texture3D.Dispose();
            }
            {
                ITexture1D texture1D = device.CreateTexture1D(new(Format.R32Float, IResource.MaximumTexture1DSize, 1, 1, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None), null);
                texture1D.Dispose();
                ITexture2D texture2D = device.CreateTexture2D(new(Format.R32Float, IResource.MaximumTexture2DSize, IResource.MaximumTexture2DSize, 1, 1, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, 1, 0, ResourceMiscFlag.None), null);
                texture2D.Dispose();
                ITexture3D texture3D = device.CreateTexture3D(new(Format.R8UInt, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, IResource.MaximumTexture3DSize / 2, 1, BindFlags.ShaderResource, Usage.Default, CpuAccessFlags.None, ResourceMiscFlag.None), null);
                texture3D.Dispose();
            }
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void TextureFormatTest()
        {
            Format[] formats = Enum.GetValues<Format>();
            foreach (Format format in formats)
            {
                if (format == Format.Unknown || format.ToString().Contains("BC"))
                    continue;
                if (!(format.ToString().Contains("BC")))
                {
                    ITexture1D texture1D = device.CreateTexture1D(format, 1, 1, 1, null, BindFlags.None, ResourceMiscFlag.None);
                    texture1D.Dispose();
                }
                ITexture2D texture2D = device.CreateTexture2D(format, 1, 1, 1, 1, null, BindFlags.None, ResourceMiscFlag.None);
                texture2D.Dispose();
                if (!(format == Format.Depth16UNorm || format == Format.Depth24UNormStencil8 || format == Format.Depth32Float || format == Format.Depth32FloatStencil8))
                {
                    ITexture3D texture3D = device.CreateTexture3D(format, 1, 1, 1, 1, null, BindFlags.None, ResourceMiscFlag.None);
                    texture3D.Dispose();
                }
            }

            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void DepthStencilViewTest()
        {
            ITexture2D texture = device.CreateTexture2D(Format.Depth24UNormStencil8, 128, 128, 1, 1, null, BindFlags.DepthStencil, ResourceMiscFlag.None);
            IDepthStencilView dsv = device.CreateDepthStencilView(texture);
            dsv.Dispose();
            IDepthStencilView dsv1 = device.CreateDepthStencilView(texture, new DepthStencilViewDescription(texture, DepthStencilViewDimension.Texture2D));
            dsv1.Dispose();
            texture.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void RenderTargetViewTest()
        {
            ITexture2D texture = device.CreateTexture2D(Format.RGBA32Float, 128, 128, 1, 1, null, BindFlags.RenderTarget, ResourceMiscFlag.None);
            IRenderTargetView rtv = device.CreateRenderTargetView(texture, new(128, 128));
            rtv.Dispose();
            IRenderTargetView rtv1 = device.CreateRenderTargetView(texture, new RenderTargetViewDescription(texture, RenderTargetViewDimension.Texture2D), new(128, 128));
            rtv1.Dispose();
            texture.Dispose();

            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void ShaderResourceViewTest()
        {
            ITexture2D texture = device.CreateTexture2D(Format.RGBA32Float, 128, 128, 1, 1, null, BindFlags.ShaderResource, ResourceMiscFlag.None);
            IShaderResourceView srv = device.CreateShaderResourceView(texture);
            srv.Dispose();
            IShaderResourceView srv1 = device.CreateShaderResourceView(texture, new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2D));
            srv1.Dispose();
            texture.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void SamplerStateTest()
        {
            ISamplerState LinearWrap = device.CreateSamplerState(SamplerDescription.LinearWrap);
            ISamplerState LinearClamp = device.CreateSamplerState(SamplerDescription.LinearClamp);
            ISamplerState PointWrap = device.CreateSamplerState(SamplerDescription.PointWrap);
            ISamplerState PointClamp = device.CreateSamplerState(SamplerDescription.PointClamp);
            ISamplerState AnisotropicWrap = device.CreateSamplerState(SamplerDescription.AnisotropicWrap);
            ISamplerState AnisotropicClamp = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            LinearWrap.Dispose();
            LinearClamp.Dispose();
            PointWrap.Dispose();
            PointClamp.Dispose();
            AnisotropicWrap.Dispose();
            AnisotropicClamp.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void TextureLoadTest()
        {
            ITexture1D tex1d = device.LoadTexture1D("assets/data/1d.dds");
            ITexture2D tex2d = device.LoadTexture2D("assets/data/sp_d.png");
            ITexture3D tex3d = device.LoadTexture3D("assets/data/3d.dds");
            ITexture2D cube = device.LoadTextureCube("assets/data/env_o.dds");
            tex1d.Dispose();
            tex2d.Dispose();
            tex3d.Dispose();
            cube.Dispose();
            LeakTracer.ReportLiveInstances();
        }

        [Test]
        public void TextureFallbackTest()
        {
            ITexture1D tex1d = device.LoadTexture1D("");
            ITexture2D tex2d = device.LoadTexture2D("");
            ITexture3D tex3d = device.LoadTexture3D("");
            ITexture2D cube = device.LoadTextureCube("");
            tex1d.Dispose();
            tex2d.Dispose();
            tex3d.Dispose();
            cube.Dispose();
            LeakTracer.ReportLiveInstances();
        }
    }
}