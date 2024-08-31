namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Meshes;
    using HexaEngine.Scenes.Managers;
    using Silk.NET.OpenAL;
    using System;
    using System.Numerics;

    [EditorDisplayName("Fog")]
    public class Fog : PostFxBase
    {
#nullable disable

        private IComputePipelineState volume;
        private IGraphicsPipelineState fog;

        private ConstantBuffer<VolumeParams> volumeParamsBuffer;
        private ConstantBuffer<FogParams> fogParamsBuffer;

        private Texture3D densityTex;

#nullable restore
        private float minValue;
        private float maxValue;
        private bool animate;
        private float animationStrength;
        private float fogIntensity;
        private float fogStart;
        private float fogEnd;
        private float fogDensity;
        private Vector3 fogColor;
        private float fogHeightEnd;
        private float noiseScale = 0.003f;

        private struct VolumeParams(Matrix4x4 transform, Vector3 dimensions, float minValue, float maxValue, int animate, float animationStrength, float noiseScale)
        {
            public Matrix4x4 Transform = transform;
            public Vector3 Dimensions = dimensions;
            public float MinValue = minValue;
            public float MaxValue = maxValue;
            public int Animate = animate;
            public float AnimationStrength = animationStrength;
            public float NoiseScale = noiseScale;
        }

        private struct FogParams(Vector4 fogColor, float fogIntensity, float fogStart, float fogEnd, float fogHeightEnd)
        {
            public Vector4 FogColor = fogColor;
            public float FogIntensity = fogIntensity;
            public float FogStart = fogStart;
            public float FogEnd = fogEnd;
            public float FogHeightEnd = fogHeightEnd;
        }

        public float MinValue
        {
            get => minValue;
            set => NotifyPropertyChangedAndSet(ref minValue, value);
        }

        public float MaxValue
        {
            get => maxValue;
            set => NotifyPropertyChangedAndSet(ref maxValue, value);
        }

        public bool Animate
        {
            get => animate;
            set => NotifyPropertyChangedAndSet(ref animate, value);
        }

        public float AnimationStrength
        {
            get => animationStrength;
            set => NotifyPropertyChangedAndSet(ref animationStrength, value);
        }

        public float FogIntensity
        {
            get => fogIntensity;
            set => NotifyPropertyChangedAndSet(ref fogIntensity, value);
        }

        public float FogStart
        {
            get => fogStart;
            set => NotifyPropertyChangedAndSet(ref fogStart, value);
        }

        public float FogEnd
        {
            get => fogEnd;
            set => NotifyPropertyChangedAndSet(ref fogEnd, value);
        }

        public Vector3 FogColor
        {
            get => fogColor;
            set => NotifyPropertyChangedAndSet(ref fogColor, value);
        }

        public float FogHeightEnd
        {
            get => fogHeightEnd;
            set => NotifyPropertyChangedAndSet(ref fogHeightEnd, value);
        }

        public float NoiseScale
        {
            get => noiseScale;
            set => NotifyPropertyChangedAndSet(ref noiseScale, value);
        }

        public override string Name { get; } = "Fog";

        public override PostFxFlags Flags { get; }

        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunAfter<HBAO>()
                .RunAfter<SSGI>()
                .RunAfter<SSR>()
                .RunBefore<MotionBlur>()
                .RunBefore<AutoExposure>()
                .RunBefore<TAA>()
                .RunBefore<DepthOfField>()
                .RunBefore<ChromaticAberration>()
                .RunBefore<Bloom>()
                .RunBefore<VolumetricLighting>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            volume = device.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "compute/volume/shader.hlsl"
            });

            fog = device.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    PixelShader = "effects/fog/ps.hlsl",
                    VertexShader = "quad.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultFullscreen
            });

            volumeParamsBuffer = new(CpuAccessFlags.Write);
            fogParamsBuffer = new(CpuAccessFlags.Write);

            densityTex = new(Format.R32Float, 160, 90, 128, 1, CpuAccessFlags.None, GpuAccessFlags.Read | GpuAccessFlags.UA);
        }

        public override void Update(IGraphicsContext context)
        {
            var camera = CameraManager.Current;
            if (camera == null)
            {
                return;
            }

            Matrix4x4.Decompose(camera.Transform, out _, out var rotation, out var translation);

            Matrix4x4 transform = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);

            VolumeParams volumeParams = new(Matrix4x4.Transpose(transform), new(densityTex.Width, densityTex.Height, densityTex.Depth), minValue, maxValue, animate ? 1 : 0, animationStrength, noiseScale);
            volumeParamsBuffer.Update(context, volumeParams);

            if (dirty)
            {
                FogParams fogParams = new(new(fogColor, 1), fogIntensity, fogStart, fogEnd, fogHeightEnd);
                fogParamsBuffer.Update(context, fogParams);
                dirty = false;
            }
        }

        public override void UpdateBindings()
        {
            volume.Bindings.SetCBV("VolumeParams", volumeParamsBuffer);
            volume.Bindings.SetUAV("volumeTex", densityTex);

            fog.Bindings.SetCBV("FogParams", fogParamsBuffer);
            fog.Bindings.SetSRV("hdrTexture", Input);
            fog.Bindings.SetSRV("volumeTex", densityTex);
        }

        public override unsafe void Draw(IGraphicsContext context)
        {
            context.SetComputePipelineState(volume);

            uint threadGroupsX = (uint)Math.Ceiling(densityTex.Width / 16f);
            uint threadGroupsY = (uint)Math.Ceiling(densityTex.Height / 9f);
            uint threadGroupsZ = (uint)Math.Ceiling(densityTex.Depth / 4f);
            context.Dispatch(threadGroupsX, threadGroupsY, threadGroupsZ);

            context.SetComputePipelineState(null);

            context.SetRenderTarget(Output, null);
            context.SetViewport(Viewport);
            context.SetGraphicsPipelineState(fog);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);
        }

        protected override void DisposeCore()
        {
        }
    }
}