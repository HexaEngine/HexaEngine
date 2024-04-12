namespace HexaEngine.PostFx.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.UI;
    using HexaEngine.Graphics.Effects.Noise;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class VolumetricScattering : PostFxBase
    {
        private PostFxGraphResourceBuilder creator;

        private IGraphicsPipelineState godrays;
        private ISamplerState linearClampSampler;
        private Quad quad;
        private IGraphicsPipelineState sun;
        private ConstantBuffer<GodRaysParams> paramsBuffer;
        private ConstantBuffer<CBWorld> paramsWorldBuffer;

        private ResourceRef<Texture2D> sunMask;
        private Texture2D noiseTex;
        private Texture2D sunSpriteTex;
        private float sunSize = 15;
        private string sunSpriteTexPath = "sun/sunsprite.png";

        private float density = 0.975f;
        private float weight = 0.25f;
        private float decay = 0.974f;
        private float exposure = 1.0f;

        private bool sunPresent;
        private ResourceRef<DepthStencil> depth;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;
        private VolumetricScatteringQualityPreset quality;

        public override string Name => "GodRays";

        public override PostFxFlags Flags { get; } = PostFxFlags.Inline | PostFxFlags.PrePass | PostFxFlags.Dynamic;

        /// <inheritdoc/>
        public override PostFxColorSpace ColorSpace { get; } = PostFxColorSpace.HDR;

        public float SunSize
        {
            get => sunSize;
            set => NotifyPropertyChangedAndSetAndReload(ref sunSize, value);
        }

        public string SunSpriteTexPath
        {
            get => sunSpriteTexPath;
            set => NotifyPropertyChangedAndSetAndReload(ref sunSpriteTexPath, value);
        }

        public enum VolumetricScatteringQualityPreset
        {
            Custom = -1,
            Low = 0,
            Medium = 1,
            High = 2,
            Extreme = 3
        }

        public struct GodRaysParams
        {
            public Vector4 SunPos;
            public Vector4 Color;
            public float Density;
            public float Weight;
            public float Decay;
            public float Exposure;

            public GodRaysParams(Vector4 sunPos, Vector4 color, float density, float weight, float decay, float exposure)
            {
                SunPos = sunPos;
                Color = color;
                Density = density;
                Weight = weight;
                Decay = decay;
                Exposure = exposure;
            }
        }

        public VolumetricScatteringQualityPreset Quality
        {
            get => quality;
            set => NotifyPropertyChangedAndSetAndReload(ref quality, value);
        }

        public float Density
        {
            get => density;
            set => NotifyPropertyChangedAndSet(ref density, value);
        }

        public float Weight
        {
            get => weight;
            set => NotifyPropertyChangedAndSet(ref weight, value);
        }

        public float Decay
        {
            get => decay;
            set => NotifyPropertyChangedAndSet(ref decay, value);
        }

        public float Exposure
        {
            get => exposure;
            set => NotifyPropertyChangedAndSet(ref exposure, value);
        }

        /// <inheritdoc/>
        public override void SetupDependencies(PostFxDependencyBuilder builder)
        {
            builder
                .RunBefore<ColorGrading>()
                .RunBefore<Vignette>()
                .RunAfter<VolumetricClouds>()
                .RunBefore<TAA>();
        }

        public override void Initialize(IGraphicsDevice device, PostFxGraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            this.creator = creator;
            depth = creator.GetDepthStencilBuffer("#DepthStencil");
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");
            sunMask = creator.CreateBuffer("VOLUMETRIC_SCATTERING_SUN_MASK");

            // TODO: Integrate in new asset system.
            if (!string.IsNullOrEmpty(sunSpriteTexPath))
            {
                try
                {
                    sunSpriteTex = Texture2D.LoadFromAssets(sunSpriteTexPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load lens dirt tex", ex.Message);
                    LoggerFactory.General.Log(ex);
                }
            }

            List<ShaderMacro> shaderMacros = new(macros)
            {
                new("VOLUMETRIC_SCATTERING_QUALITY", ((int)quality).ToString())
            };

            godrays = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/godrays/ps.hlsl",
                Macros = [.. shaderMacros],
            }, new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Additive,
                Topology = PrimitiveTopology.TriangleStrip,
                BlendFactor = default,
                SampleMask = int.MaxValue
            });

            paramsBuffer = new(CpuAccessFlags.Write);

            quad = new(sunSize);

            sun = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "effects/sun/vs.hlsl",
                PixelShader = "effects/sun/ps.hlsl",
                Macros = macros
            }, new()
            {
                Blend = BlendDescription.Opaque,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.DepthRead,
                Rasterizer = RasterizerDescription.CullBack,
                SampleMask = int.MaxValue,
                StencilRef = 0,
                Topology = PrimitiveTopology.TriangleList
            });

            paramsWorldBuffer = new(CpuAccessFlags.Write);

            linearClampSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            noiseTex = new(Format.R32Float, 256, 256, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            Application.MainWindow.Dispatcher.InvokeBlocking(() =>
            {
                Noise noise = new(device, NoiseType.Blue2D);
                noise.Draw2D(device.Context, noiseTex.RTV, new(256), Vector2.One);
                noise.Dispose();
            });

            Viewport = new(width, height);
        }

        public override void Resize(int width, int height)
        {
            creator.UpdateTexture2D("SunMask", new(Format.R16G16B16A16Float, width, height, 1, 1, GpuAccessFlags.RW));
        }

        /// <inheritdoc/>
        public override unsafe void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (!sunPresent)
            {
                return;
            }
        }

        /// <inheritdoc/>
        public override unsafe void Draw(IGraphicsContext context)
        {
            var camera = CameraManager.Current;
            var lights = LightManager.Current;
            if (camera == null || lights == null)
            {
                return;
            }

            sunPresent = false;
            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (light is DirectionalLight)
                {
                    var lightPosition = Vector3.Transform(light.Transform.Backward * camera.Transform.ClipRange + camera.Transform.GlobalPosition, camera.Transform.View);

                    // skip render, the light is behind the camera.
                    if (lightPosition.Z < 0.0f || exposure <= 0)
                    {
                        return;
                    }

                    var transform = Matrix4x4.CreateTranslation(light.Transform.Backward * camera.Transform.ClipRange);
                    paramsWorldBuffer.Update(context, new CBWorld(transform));

                    GodRaysParams raysParams = default;
                    raysParams.SunPos = camera.ProjectPosition(lightPosition);
                    raysParams.Color = light.Color;
                    raysParams.Density = density;
                    raysParams.Weight = weight;
                    raysParams.Decay = decay;
                    raysParams.Exposure = exposure;
                    paramsBuffer.Update(context, raysParams);

                    sunPresent = true;

                    break;
                }
            }

            if (!sunPresent || Output == null || !sunMask.HasValue)
            {
                return;
            }

            context.ClearRenderTargetView(sunMask.Value, default);

            context.SetRenderTarget(sunMask.Value, depth.Value);
            context.SetViewport(Viewport);
            context.VSSetConstantBuffer(0, paramsWorldBuffer);
            context.VSSetConstantBuffer(1, this.camera.Value);
            context.PSSetShaderResource(0, sunSpriteTex);
            context.PSSetSampler(0, linearClampSampler);
            quad.DrawAuto(context, sun);
            context.ClearState();

            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetShaderResource(0, sunMask.Value);
            context.PSSetShaderResource(1, noiseTex);
            context.PSSetSampler(0, linearClampSampler);
            context.SetPipelineState(godrays);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        /// <inheritdoc/>
        protected override void DisposeCore()
        {
            godrays.Dispose();
            linearClampSampler.Dispose();
            paramsBuffer.Dispose();
            noiseTex.Dispose();

            quad.Dispose();
            sun.Dispose();
            linearClampSampler.Dispose();
            paramsWorldBuffer.Dispose();
            sunSpriteTex.Dispose();
            creator.DisposeResource("SunMask");
        }
    }
}