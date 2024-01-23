﻿namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using System.Numerics;

    public class Vignette : PostFxBase
    {
        private IGraphicsPipeline pipeline;
        private ConstantBuffer<VignetteParams> paramsBuffer;

        private ISamplerState samplerState;

        private float intensity = -1;
        private float ratio = 1;
        private float radius = 1;
        private float slope = 8;
        private Vector2 center = new(0.5f);
        private Vector3 color;

        private struct VignetteParams
        {
            public float VignetteIntensity;
            public float VignetteRatio;
            public float VignetteRadius;
            public float VignetteSlope;

            public Vector2 VignetteCenter;
            public Vector2 padd2;

            public Vector3 VignetteColor;
            public float padd3;

            public VignetteParams(float vignetteIntensity, float vignetteRatio, float vignetteRadius, float vignetteSlope, Vector2 vignetteCenter, Vector3 vignetteColor)
            {
                VignetteIntensity = vignetteIntensity;
                VignetteRatio = vignetteRatio;
                VignetteRadius = vignetteRadius;
                VignetteSlope = vignetteSlope;
                VignetteCenter = vignetteCenter;
                padd2 = default;
                VignetteColor = vignetteColor;
                padd3 = default;
            }
        }

        public override string Name { get; } = "Vignette";

        public override PostFxFlags Flags { get; } = PostFxFlags.Inline;

        public float Intensity
        {
            get => intensity;
            set => NotifyPropertyChangedAndSet(ref intensity, value);
        }

        public float Ratio
        {
            get => ratio;
            set => NotifyPropertyChangedAndSet(ref ratio, value);
        }

        public float Radius
        {
            get => radius;
            set => NotifyPropertyChangedAndSet(ref radius, value);
        }

        public float Slope
        {
            get => slope;
            set => NotifyPropertyChangedAndSet(ref slope, value);
        }

        public Vector2 Center
        {
            get => center;
            set => NotifyPropertyChangedAndSet(ref center, value);
        }

        public Vector3 Color
        {
            get => color;
            set => NotifyPropertyChangedAndSet(ref color, value);
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunAfter<HBAO>()
                .RunAfter<SSGI>()
                .RunAfter<SSR>()
                .RunAfter<MotionBlur>()
                .RunAfter<AutoExposure>()
                .RunAfter<TAA>()
                .RunAfter<DepthOfField>()
                .RunAfter<ChromaticAberration>()
                .RunAfter<Bloom>();
        }

        public override void Initialize(IGraphicsDevice device, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            paramsBuffer = new(device, CpuAccessFlags.Write);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/vignette/ps.hlsl",
                State = new()
                {
                    DepthStencil = DepthStencilDescription.None,
                    Rasterizer = RasterizerDescription.CullBack,
                    Blend = new(Blend.SourceAlpha, Blend.InverseSourceAlpha, Blend.One, Blend.DestinationAlpha, BlendOperation.Add, BlendOperation.Add),
                    Topology = PrimitiveTopology.TriangleStrip,
                    BlendFactor = default,
                    SampleMask = int.MaxValue
                },
                Macros = macros
            });

            samplerState = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public override unsafe void Update(IGraphicsContext context)
        {
            if (dirty)
            {
                paramsBuffer.Update(context, new(intensity, ratio, radius, slope, center, color));
                dirty = false;
            }
        }

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.PSSetShaderResource(0, Input);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetSampler(0, samplerState);
            context.SetGraphicsPipeline(pipeline);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.PSSetSampler(0, null);
            context.PSSetConstantBuffer(0, null);
            context.PSSetShaderResource(0, null);
            context.SetViewport(default);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
            pipeline.Dispose();
            paramsBuffer.Dispose();
            samplerState.Dispose();
        }
    }
}