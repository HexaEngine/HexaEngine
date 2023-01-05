namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects.Primitives;
    using System;
    using System.Numerics;

    public unsafe class DepthMipChain : IEffect
    {
        private readonly Quad quad;
        private readonly ComputePipeline downsample;
        private readonly IBuffer cbDownsample;
        private readonly GraphicsPipeline copy;
        public int Height;
        public int Width;
        private ITexture2D texture;
        public IRenderTargetView RTV;
        public IShaderResourceView SRV;
        private IShaderResourceView[] srvs;
        private IUnorderedAccessView[] uavs;

        private Viewport[] viewports;
        public int Mips;

        private void** pUavs;

        public IShaderResourceView? Input;

        public DepthMipChain(IGraphicsDevice device, int width, int height)
        {
            Width = width;
            Height = height;
            quad = new(device);

            downsample = new(device, new()
            {
                Path = "compute/hiz/shader.hlsl",
            });
            cbDownsample = device.CreateBuffer(new Vector4(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);

            copy = new(device, new()
            {
                VertexShader = "effects/copy/vs.hlsl",
                PixelShader = "effects/copy/ps.hlsl"
            });

            Mips = GetNumMipLevels(width, height);

            texture = device.CreateTexture2D(Format.R32Float, width, height, 1, Mips, null, BindFlags.ShaderResource | BindFlags.RenderTarget | BindFlags.UnorderedAccess);
            SRV = device.CreateShaderResourceView(texture);
            RTV = device.CreateRenderTargetView(texture, new(width, height));

            srvs = new IShaderResourceView[Mips];
            uavs = new IUnorderedAccessView[Mips];
            pUavs = AllocArray((uint)Mips);
            viewports = new Viewport[Mips];
            int mipWidth = width;
            int mipHeight = height;

            for (int i = 0; i < Mips; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            this.Width = width;
            this.Height = height;
            for (int i = 0; i < Mips; i++)
            {
                srvs[i].Dispose();
                uavs[i].Dispose();
            }

            Free(pUavs);

            RTV.Dispose();
            SRV.Dispose();
            texture.Dispose();

            Mips = GetNumMipLevels(width, height);

            texture = device.CreateTexture2D(Format.R32Float, width, height, 1, Mips, null, BindFlags.ShaderResource | BindFlags.RenderTarget | BindFlags.UnorderedAccess);
            SRV = device.CreateShaderResourceView(texture);
            RTV = device.CreateRenderTargetView(texture, new(width, height));

            srvs = new IShaderResourceView[Mips];
            uavs = new IUnorderedAccessView[Mips];
            pUavs = AllocArray((uint)Mips);
            viewports = new Viewport[Mips];
            int mipWidth = width;
            int mipHeight = height;

            for (int i = 0; i < Mips; i++)
            {
                srvs[i] = device.CreateShaderResourceView(texture, new(ShaderResourceViewDimension.Texture2D, Format.R32Float, i, 1, 0, 1));
                uavs[i] = device.CreateUnorderedAccessView(texture, new(UnorderedAccessViewDimension.Texture2D, Format.R32Float, i, 0, 1, BufferUnorderedAccessViewFlags.None));
                pUavs[i] = (void*)uavs[i].NativePointer;
                viewports[i] = new(mipWidth, mipHeight);
                mipWidth /= 2;
                mipHeight /= 2;
            }
        }

        public void Draw(IGraphicsContext context)
        {
            context.SetRenderTarget(RTV, null);
            context.PSSetShaderResource(Input, 0);
            quad.DrawAuto(context, copy, viewports[0]);
            context.ClearState();

            for (uint i = 1; i < Mips; i++)
            {
                Vector2 texel = new(1 / viewports[i].Width * viewports[i - 1].Width, 1 / viewports[i].Height * viewports[i - 1].Height);
                context.Write(cbDownsample, new Vector4(texel, 0, 0));
                context.CSSetConstantBuffer(cbDownsample, 0);
                context.CSSetUnorderedAccessViews(&pUavs[i], (uint)Mips);
                context.CSSetShaderResource(srvs[i - 1], 0);
                downsample.Dispatch(context, (int)viewports[i].Width, (int)viewports[i].Height, 1);
            }

            context.ClearState();
        }

        public void Generate(IGraphicsContext context, IShaderResourceView input)
        {
            context.SetRenderTarget(RTV, null);
            context.PSSetShaderResource(input, 0);
            quad.DrawAuto(context, copy, viewports[0]);
            context.ClearState();

            for (uint i = 1; i < Mips; i++)
            {
                Vector2 texel = new(1 / viewports[i].Width * viewports[i - 1].Width, 1 / viewports[i].Height * viewports[i - 1].Height);
                context.Write(cbDownsample, new Vector4(texel, 0, 0));
                context.CSSetConstantBuffer(cbDownsample, 0);
                context.CSSetUnorderedAccessViews(&pUavs[i], 1);
                context.CSSetShaderResource(srvs[i - 1], 0);
                downsample.Dispatch(context, (int)viewports[i].Width / 32 + 1, (int)viewports[i].Height / 32 + 1, 1);
            }

            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();
            downsample.Dispose();
            cbDownsample.Dispose();
            copy.Dispose();

            for (int i = 0; i < Mips; i++)
            {
                srvs[i].Dispose();
                uavs[i].Dispose();
            }

            Free(pUavs);

            RTV.Dispose();
            SRV.Dispose();
            texture.Dispose();
        }

        private static int GetNumMipLevels(int width, int height)
        {
            int numLevels = 1;
            while (width > 1 && height > 1)
            {
                width = Math.Max(width / 2, 1);
                height = Math.Max(height / 2, 1);
                ++numLevels;
            }

            return numLevels;
        }

        public void BeginResize()
        {
            throw new NotImplementedException();
        }

        public void EndResize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}