namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Effects.Noise;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;
    using System.Threading.Tasks;

    public class GodRays : PostFxBase
    {
        private IGraphicsDevice device;
        private Core.Graphics.Primitives.Plane quad1;
        private IGraphicsPipeline sun;
        private ISamplerState sunSampler;
        private ConstantBuffer<SunParams> paramsSunBuffer;
        private ConstantBuffer<CBWorld> paramsWorldBuffer;

        private IGraphicsPipeline godrays;
        private ISamplerState sampler;
        private ConstantBuffer<GodRaysParams> paramsBuffer;

        private Texture2D sunsprite;
        private Texture2D sunBuffer;
        private Texture2D noiseTex;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;

        private float godraysDensity = 0.975f;
        private float godraysWeight = 0.25f;
        private float godraysDecay = 0.825f;
        private float godraysExposure = 2.0f;

        private bool sunPresent;

        public override string Name => "GodRays";

        public override PostFxFlags Flags { get; } = PostFxFlags.Inline | PostFxFlags.PrePass;

        public float Density
        {
            get => godraysDensity;
            set => NotifyPropertyChangedAndSet(ref godraysDensity, value);
        }

        public float Weight
        {
            get => godraysWeight;
            set => NotifyPropertyChangedAndSet(ref godraysWeight, value);
        }

        public float Decay
        {
            get => godraysDecay;
            set => NotifyPropertyChangedAndSet(ref godraysDecay, value);
        }

        public float Exposure
        {
            get => godraysExposure;
            set => NotifyPropertyChangedAndSet(ref godraysExposure, value);
        }

        public struct GodRaysParams
        {
            public Vector4 ScreenSpacePosition;
            public float GodraysDensity;
            public float GodraysWeight;
            public float GodraysDecay;
            public float GodraysExposure;
            public Vector4 Color;
        }

        public struct SunParams
        {
            public Vector3 Diffuse;
            public float AlbedoFactor;
        }

        public override async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunAfter("MotionBlur")
                .RunAfter("DepthOfField")
                .RunBefore("VolumetricClouds")
                .RunBefore("SSR")
                .RunBefore("SSGI")
                .RunBefore("LensFlare")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            this.device = device;
            quad1 = new(device, 5);

            sun = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "forward/sun/vs.hlsl",
                PixelShader = "forward/sun/ps.hlsl"
            }, new GraphicsPipelineState()
            {
                Blend = BlendDescription.AlphaBlend,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.DepthRead,
                Rasterizer = RasterizerDescription.CullBack,
                SampleMask = int.MaxValue,
                StencilRef = 0,
                Topology = PrimitiveTopology.TriangleList
            }, macros);
            sunSampler = device.CreateSamplerState(SamplerStateDescription.LinearWrap);

            paramsSunBuffer = new(device, CpuAccessFlags.Write);
            paramsWorldBuffer = new(device, CpuAccessFlags.Write);

            godrays = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/godrays/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Additive,
                Topology = PrimitiveTopology.TriangleStrip,
                BlendFactor = default,
                SampleMask = int.MaxValue
            }, macros);
            sampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);

            paramsBuffer = new(device, CpuAccessFlags.Write);

            sunsprite = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/sun/sunsprite.png"));
            sunBuffer = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);

            noiseTex = new(device, Format.R32Float, 1024, 1024, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            Application.MainWindow.Dispatcher.InvokeBlocking(() =>
            {
                Noise noise = new(device, NoiseType.Blue2D);
                noise.Draw2D(device.Context, noiseTex.RTV, new(1024), Vector2.One);
                noise.Dispose();
            });

            Viewport = new(width, height);
        }

        public override void Resize(int width, int height)
        {
            sunBuffer.Resize(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None);
        }

        public override void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public override void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        private const float MaxLightDist = 1.3f;

        public override void Update(IGraphicsContext context)
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
                    sunPresent = true;
                    GodRaysParams raysParams = default;

                    raysParams.GodraysDecay = godraysDecay;
                    raysParams.GodraysWeight = godraysWeight;
                    raysParams.GodraysDensity = godraysDensity;
                    raysParams.GodraysExposure = godraysExposure;
                    raysParams.Color = light.Color;

                    var camera_position = camera.Transform.GlobalPosition;

                    var translation = Matrix4x4.CreateTranslation(camera_position);

                    var far = camera.Transform.Far;
                    var light_position = Vector3.Transform(light.Transform.Backward * (far / 2f), translation);

                    var transform = Matrix4x4.CreateTranslation(light.Transform.Backward * (far / 15));

                    var light_posH = Vector4.Transform(light_position, camera.Transform.ViewProjection);
                    var ss_sun_pos = new Vector4(0.5f * light_posH.X / light_posH.W + 0.5f, -0.5f * light_posH.Y / light_posH.W + 0.5f, light_posH.Z / light_posH.W, 1.0f);

                    raysParams.ScreenSpacePosition = ss_sun_pos;

                    paramsBuffer.Update(context, raysParams);

                    CBWorld world = default;

                    world.World = Matrix4x4.Transpose(transform);
                    world.WorldInv = Matrix4x4.Transpose(light.Transform.GlobalInverse);

                    paramsWorldBuffer.Update(context, world);

                    SunParams sunParams = default;

                    sunParams.Diffuse = Vector3.One;
                    sunParams.AlbedoFactor = 1f;

                    paramsSunBuffer.Update(context, sunParams);

                    break;
                }
            }
        }

        public override unsafe void PrePassDraw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (!sunPresent)
                return;

            var depth = creator.GetDepthStencilBuffer("#DepthStencil");
            var camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            context.ClearRenderTargetView(sunBuffer.RTV, default);
            context.SetRenderTarget(sunBuffer.RTV, depth.DSV);
            context.SetViewport(Viewport);
            context.VSSetConstantBuffer(0, paramsWorldBuffer);
            context.VSSetConstantBuffer(1, camera);
            context.PSSetConstantBuffer(0, paramsSunBuffer);
            context.PSSetShaderResource(0, sunsprite.SRV);
            context.PSSetSampler(0, sunSampler);
            quad1.DrawAuto(context, sun);
        }

        public override unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null || !sunPresent)
            {
                return;
            }

            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffer(0, paramsBuffer);
            context.PSSetShaderResource(0, sunBuffer.SRV);
            context.PSSetSampler(0, sampler);
            context.SetGraphicsPipeline(godrays);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        protected override void DisposeCore()
        {
            quad1.Dispose();
            sun.Dispose();
            sunSampler.Dispose();
            paramsSunBuffer.Dispose();
            paramsWorldBuffer.Dispose();

            godrays.Dispose();
            sampler.Dispose();
            paramsBuffer.Dispose();

            sunsprite.Dispose();
            sunBuffer.Dispose();
            noiseTex.Dispose();
        }
    }
}