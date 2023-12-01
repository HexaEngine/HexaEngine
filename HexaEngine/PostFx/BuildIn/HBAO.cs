﻿#nullable disable

using HexaEngine;

namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Effects.Blur;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class HBAO : PostFxBase
    {
        private IGraphicsDevice device;
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<HBAOParams> paramsBuffer;
        private Texture2D noiseTex;
        private Texture2D intermediateTex;
        private GaussianBlur blur;

        private ResourceRef<Texture2D> ao;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private ResourceRef<GBuffer> gbuffer;

        private Viewport viewport;

        private ISamplerState samplerLinear;

        private float samplingRadius = 0.5f;
        private uint numSamplingDirections = 8;
        private float samplingStep = 0.004f;
        private uint numSamplingSteps = 4;
        private float power = 1;

        private const int NoiseSize = 4;

        #region Structs

        private struct HBAOParams
        {
            public float SamplingRadius;
            public float SamplingRadiusToScreen;
            public uint NumSamplingDirections;
            public float SamplingStep;

            public uint NumSamplingSteps;
            public float Power;
            public Vector2 NoiseScale;

            public HBAOParams()
            {
                SamplingRadius = 0.5f;
                SamplingRadiusToScreen = 0;
                NumSamplingDirections = 8;
                SamplingStep = 0.004f;
                NumSamplingSteps = 4;
                Power = 1;
            }
        }

        #endregion Structs

        #region Properties

        public override string Name => "HBAO";

        public override PostFxFlags Flags { get; } = PostFxFlags.NoOutput | PostFxFlags.NoInput;

        public float SamplingRadius
        {
            get => samplingRadius;
            set => NotifyPropertyChangedAndSet(ref samplingRadius, value);
        }

        public uint NumSamplingDirections
        {
            get => numSamplingDirections;
            set => NotifyPropertyChangedAndSet(ref numSamplingDirections, value);
        }

        public float SamplingStep
        {
            get => samplingStep;
            set => NotifyPropertyChangedAndSet(ref samplingStep, value);
        }

        public uint NumSamplingSteps
        {
            get => numSamplingSteps;
            set => NotifyPropertyChangedAndSet(ref numSamplingSteps, value);
        }

        public float Power
        {
            get => power;
            set => NotifyPropertyChangedAndSet(ref power, value);
        }

        #endregion Properties

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
        }

        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            ao = creator.GetTexture2D("#AOBuffer");
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            gbuffer = creator.GetGBuffer("GBuffer");

            this.device = device;

            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/hbao/ps.hlsl",
                State = GraphicsPipelineState.DefaultFullscreen,
            });
            paramsBuffer = new(device, CpuAccessFlags.Write);
            intermediateTex = new(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            blur = new(device, Format.R32Float, width, height);

            unsafe
            {
                Texture2DDescription description = new(Format.R32G32B32A32Float, NoiseSize, NoiseSize, 1, 1, BindFlags.ShaderResource, Usage.Immutable);

                Vector4* pixelData = stackalloc Vector4[NoiseSize * NoiseSize];

                SubresourceData initialData = default;
                initialData.DataPointer = (nint)pixelData;
                initialData.RowPitch = NoiseSize * sizeof(Vector4);

                for (int i = 0; i < NoiseSize * NoiseSize; i++)
                {
                    float rand = Random.Shared.NextSingle() * float.Pi * 2.0f;
                    pixelData[i] = new Vector4(MathF.Sin(rand), MathF.Cos(rand), Random.Shared.NextSingle(), 1.0f);
                }

                noiseTex = new(device, description, initialData);
            }

            samplerLinear = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
            viewport = new(width, height);
        }

        public override void Resize(int width, int height)
        {
            intermediateTex.Resize(device, Format.R32Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            viewport = new(width, height);
        }

        public override void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                var camera = CameraManager.Current;
                HBAOParams hbaoParams = default;
                hbaoParams.SamplingRadius = samplingRadius;
                hbaoParams.SamplingRadiusToScreen = samplingRadius * 0.5f * viewport.Height / (MathF.Tan(camera.Fov.ToRad() * 0.5f) * 2.0f); ;
                hbaoParams.SamplingStep = samplingStep;
                hbaoParams.NumSamplingSteps = numSamplingSteps;
                hbaoParams.NumSamplingDirections = numSamplingDirections;
                hbaoParams.Power = power;
                hbaoParams.NoiseScale = viewport.Size / NoiseSize;

                paramsBuffer.Update(context, hbaoParams);
                dirty = false;
            }
        }

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            context.ClearRenderTargetView(ao.Value.RTV, Vector4.One);

            nint* srvs = stackalloc nint[] { depth.Value.SRV.NativePointer, gbuffer.Value.SRVs[1].NativePointer, noiseTex.SRV.NativePointer };
            nint* cbs = stackalloc nint[] { paramsBuffer.NativePointer, camera.Value.NativePointer };
            context.SetRenderTarget(intermediateTex.RTV, null);
            context.SetViewport(viewport);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetSampler(0, samplerLinear);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            ZeroMemory(srvs, sizeof(nint) * 3);
            ZeroMemory(cbs, sizeof(nint) * 2);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffers(0, 2, (void**)cbs);
            context.PSSetShaderResources(0, 3, (void**)srvs);
            context.SetRenderTarget(null, null);

            blur.Blur(context, intermediateTex.SRV, ao.Value.RTV, (int)viewport.Width, (int)viewport.Height);
        }

        protected override unsafe void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            noiseTex.Dispose();
            intermediateTex.Dispose();
            blur.Dispose();
            samplerLinear.Dispose();
        }
    }
}