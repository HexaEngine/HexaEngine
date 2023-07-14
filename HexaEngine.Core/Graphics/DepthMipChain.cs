namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Mathematics;
    using ImGuiNET;
    using System;
    using System.Numerics;

    public unsafe class DepthMipChain
    {
        private readonly Quad quad;
        private readonly IComputePipeline downsample;
        private readonly ConstantBuffer<Vector4> cbDownsample;
        private readonly IGraphicsPipeline copy;
        public int Height;
        public int Width;
        private Texture2D texture;
        private ISamplerState samplerState;
        public IRenderTargetView RTV;
        public IShaderResourceView SRV;
        private IShaderResourceView[] srvs;
        private IUnorderedAccessView[] uavs;

        private Viewport[] viewports;
        public int Mips;

        private void** pUavs;

        public IShaderResourceView? Input;

        public DepthMipChain(IGraphicsDevice device, DepthStencilBufferDesc desc)
        {
            Width = desc.Width;
            Height = desc.Height;
            quad = new(device);

            downsample = device.CreateComputePipeline(new()
            {
                Path = "compute/hiz/shader.hlsl",
            });
            cbDownsample = new(device, CpuAccessFlags.Write);

            copy = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/copy/vs.hlsl",
                PixelShader = "effects/copy/ps.hlsl"
            });

            Mips = GetNumMipLevels(desc.Width, desc.Height);

            texture = new(device, Format.R32Float, desc.Width, desc.Height, 1, Mips, CpuAccessFlags.None, GpuAccessFlags.All);
            SRV = device.CreateShaderResourceView(texture);
            RTV = device.CreateRenderTargetView(texture, new(desc.Width, desc.Height));
            samplerState = device.CreateSamplerState(SamplerStateDescription.PointWrap);

            srvs = new IShaderResourceView[Mips];
            uavs = new IUnorderedAccessView[Mips];
            pUavs = AllocArray((uint)Mips);
            viewports = new Viewport[Mips];
            int mipWidth = desc.Width;
            int mipHeight = desc.Height;

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

        public DepthMipChain(IGraphicsDevice device, int width, int height)
        {
            Width = width;
            Height = height;
            quad = new(device);

            downsample = device.CreateComputePipeline(new()
            {
                Path = "compute/hiz/shader.hlsl",
            });
            cbDownsample = new(device, CpuAccessFlags.Write);

            copy = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/copy/vs.hlsl",
                PixelShader = "effects/copy/ps.hlsl"
            });

            Mips = GetNumMipLevels(width, height);

            texture = new(device, Format.R32Float, width, height, 1, Mips, CpuAccessFlags.None, GpuAccessFlags.All);
            SRV = device.CreateShaderResourceView(texture);
            RTV = device.CreateRenderTargetView(texture, new(width, height));
            samplerState = device.CreateSamplerState(SamplerStateDescription.PointWrap);

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

        public Viewport[] Viewports => viewports;

        public IUnorderedAccessView[] UAVs => uavs;

        public IShaderResourceView[] SRVs => srvs;

        public void Debug()
        {
            for (int i = 0; i < Mips; i++)
            {
                var view = viewports[i];
                ImGui.Image(srvs[i].NativePointer, new(view.Width, view.Height), Vector2.Zero, Vector2.One, new(new Vector3(0.2f), 1));
            }
        }

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            Width = width;
            Height = height;
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

            texture.Resize(device, Format.R32Float, width, height, 1, Mips, CpuAccessFlags.None, GpuAccessFlags.All);
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
            context.PSSetShaderResource(0, Input);
            context.SetViewport(viewports[0]);
            quad.DrawAuto(context, copy);
            context.ClearState();

            for (uint i = 1; i < Mips; i++)
            {
                Vector2 texel = new(1 / viewports[i].Width * viewports[i - 1].Width, 1 / viewports[i].Height * viewports[i - 1].Height);
                context.Write(cbDownsample, new Vector4(texel, 0, 0));
                context.CSSetConstantBuffer(0, cbDownsample);
                context.CSSetUnorderedAccessView(pUavs[i]);
                context.CSSetShaderResource(0, srvs[i - 1]);
                context.CSSetSampler(0, samplerState);
                downsample.Dispatch(context, (uint)viewports[i].Width / 16, (uint)viewports[i].Height / 16, 1);
            }

            context.ClearState();
        }

        public void Generate(IGraphicsContext context, IShaderResourceView input)
        {
            context.SetRenderTarget(RTV, null);
            context.PSSetShaderResource(0, input);
            context.SetViewport(viewports[0]);
            quad.DrawAuto(context, copy);
            context.ClearState();

            for (uint i = 1; i < Mips; i++)
            {
                Vector2 texel = new(viewports[i].Width, viewports[i].Height);
                context.Write(cbDownsample, new Vector4(texel, 0, 0));
                context.CSSetConstantBuffer(0, cbDownsample);
                context.CSSetUnorderedAccessView(pUavs[i]);
                context.CSSetShaderResource(0, srvs[i - 1]);
                context.CSSetSampler(0, samplerState);
                downsample.Dispatch(context, (uint)viewports[i].Width / 32 + 1, (uint)viewports[i].Height / 32 + 1, 1);
            }

            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();
            downsample.Dispose();
            cbDownsample.Dispose();
            copy.Dispose();
            samplerState.Dispose();

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
    }
}