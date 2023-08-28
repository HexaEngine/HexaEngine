#nullable disable

namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class Bloom : PostFxBase
    {
        private IGraphicsPipeline downsample;
        private IGraphicsPipeline upsample;
        private ConstantBuffer<ParamsDownsample> downsampleCB;
        private ConstantBuffer<ParamsUpsample> upsampleCB;
        private ISamplerState sampler;

        private IRenderTargetView[] mipChainRTVs;
        private IShaderResourceView[] mipChainSRVs;
        private Viewport[] viewports;

        private float radius = 0.003f;

        private int width;
        private int height;

        private IShaderResourceView Input;

        public override string Name => "Bloom";

        public override PostFxFlags Flags => PostFxFlags.NoOutput;

        public float Radius
        {
            get => radius;
            set
            {
                radius = value; dirty = true;
            }
        }

        #region Structs

        private struct ParamsDownsample
        {
            public Vector2 SrcResolution;
            public Vector2 Padd;

            public ParamsDownsample(Vector2 srcResolution)
            {
                SrcResolution = srcResolution;
                Padd = default;
            }
        }

        private struct ParamsUpsample
        {
            public float FilterRadius;
            public Vector3 Padd;

            public ParamsUpsample(float filterRadius)
            {
                FilterRadius = filterRadius;
                Padd = default;
            }
        }

        #endregion Structs

        public override async Task InitializeAsync(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .AddSource("Bloom")
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunAfter("MotionBlur")
                .RunAfter("DepthOfField")
                .RunAfter("GodRays")
                .RunAfter("VolumetricClouds")
                .RunAfter("SSR")
                .RunAfter("SSGI")
                .RunAfter("LensFlare")
                .RunBefore("AutoExposure");

            downsampleCB = new(device, CpuAccessFlags.Write);
            upsampleCB = new(device, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            downsample = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/downsample/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            upsample = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/upsample/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);

            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];
            viewports = new Viewport[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainRTVs[i] = ResourceManager2.Shared.AddTexture($"Bloom.{i}", new(Format.R16G16B16A16Float, currentWidth, currentHeight, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget), lineNumber: i).Value.RTV;
                mipChainSRVs[i] = ResourceManager2.Shared.GetTexture($"Bloom.{i}").Value.SRV;
                viewports[i] = new(currentWidth, currentHeight);
                currentWidth /= 2;
                currentHeight /= 2;
            }

            ResourceManager2.Shared.SetOrAddResource("Bloom", ResourceManager2.Shared.GetTexture("Bloom.0").Value);

            this.width = width;
            this.height = height;

            dirty = true;
        }

        public override void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .AddSource("Bloom")
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunAfter("MotionBlur")
                .RunAfter("DepthOfField")
                .RunAfter("GodRays")
                .RunAfter("VolumetricClouds")
                .RunAfter("SSR")
                .RunAfter("SSGI")
                .RunAfter("LensFlare")
                .RunBefore("AutoExposure");

            downsampleCB = new(device, CpuAccessFlags.Write);
            upsampleCB = new(device, CpuAccessFlags.Write);

            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            downsample = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/downsample/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);
            upsample = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/bloom/upsample/ps.hlsl",
            }, GraphicsPipelineState.DefaultFullscreen, macros);

            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];
            viewports = new Viewport[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainRTVs[i] = ResourceManager2.Shared.AddTexture($"Bloom.{i}", new(Format.R16G16B16A16Float, currentWidth, currentHeight, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget), lineNumber: i).Value.RTV;
                mipChainSRVs[i] = ResourceManager2.Shared.GetTexture($"Bloom.{i}").Value.SRV;
                viewports[i] = new(currentWidth, currentHeight);
                currentWidth /= 2;
                currentHeight /= 2;
            }

            ResourceManager2.Shared.SetOrAddResource("Bloom", ResourceManager2.Shared.GetTexture("Bloom.0").Value);

            this.width = width;
            this.height = height;

            dirty = true;
        }

        public override void Resize(int width, int height)
        {
            int currentWidth = width / 2;
            int currentHeight = height / 2;
            int levels = Math.Min(TextureHelper.ComputeMipLevels(currentWidth, currentHeight), 8);

            mipChainRTVs = new IRenderTargetView[levels];
            mipChainSRVs = new IShaderResourceView[levels];
            viewports = new Viewport[levels];

            for (int i = 0; i < levels; i++)
            {
                mipChainRTVs[i] = ResourceManager2.Shared.UpdateTexture($"Bloom.{i}", new Texture2DDescription(Format.R16G16B16A16Float, currentWidth, currentHeight, 1, 1, BindFlags.ShaderResource | BindFlags.RenderTarget)).Value.RTV;
                mipChainSRVs[i] = ResourceManager2.Shared.GetTexture($"Bloom.{i}").Value.SRV;
                viewports[i] = new(currentWidth, currentHeight);
                currentWidth /= 2;
                currentHeight /= 2;
            }

            ResourceManager2.Shared.SetOrAddResource("Bloom", ResourceManager2.Shared.GetTexture("Bloom.0").Value);

            this.width = width;
            this.height = height;
            dirty = true;
        }

        public override void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
        }

        public override void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                context.ClearRenderTargetView(mipChainRTVs[0], default);
                downsampleCB.Update(context, new ParamsDownsample(new(width, height)));
                upsampleCB.Update(context, new ParamsUpsample(radius));
                dirty = false;
            }
        }

        public override void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            context.PSSetConstantBuffer(0, downsampleCB);
            context.PSSetSampler(0, sampler);
            context.SetGraphicsPipeline(downsample);
            for (int i = 0; i < mipChainRTVs.Length; i++)
            {
                if (i > 0)
                {
                    context.PSSetShaderResource(0, mipChainSRVs[i - 1]);
                }
                else
                {
                    context.PSSetShaderResource(0, Input);
                }

                context.SetRenderTarget(mipChainRTVs[i], null);
                context.SetViewport(viewports[i]);
                context.DrawInstanced(4, 1, 0, 0);
                context.PSSetShaderResource(0, null);
                context.SetRenderTarget(null, null);
            }

            context.PSSetConstantBuffer(0, upsampleCB);
            context.SetGraphicsPipeline(upsample);
            for (int i = mipChainRTVs.Length - 1; i > 0; i--)
            {
                context.SetRenderTarget(mipChainRTVs[i - 1], null);
                context.PSSetShaderResource(0, mipChainSRVs[i]);
                context.SetViewport(viewports[i - 1]);
                context.DrawInstanced(4, 1, 0, 0);
                context.PSSetShaderResource(0, null);
                context.SetRenderTarget(null, null);
            }
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.SetGraphicsPipeline(null);
        }

        protected override void DisposeCore()
        {
            downsample.Dispose();
            upsample.Dispose();
            downsampleCB.Dispose();
            upsampleCB.Dispose();
            sampler.Dispose();
        }
    }
}