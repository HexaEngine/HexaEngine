namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Types;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;
    using System.Threading.Tasks;

    public class LensFlare : IPostFx
    {
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;
        private ConstantBuffer<Vector4> lightBuffer;

        private Texture2D lens0;
        private Texture2D lens1;
        private Texture2D lens2;
        private Texture2D lens3;
        private Texture2D lens4;
        private Texture2D lens5;
        private Texture2D lens6;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;
        private bool enabled = true;
        private int priority = 99;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "LensFlare";

        public PostFxFlags Flags { get; } = PostFxFlags.Inline;

        public bool Enabled
        {
            get => enabled; set
            {
                enabled = value;
                OnEnabledChanged?.Invoke(value);
            }
        }

        public int Priority
        {
            get => priority; set
            {
                priority = value;
                OnPriorityChanged?.Invoke(value);
            }
        }

        public async Task Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            builder
                .RunBefore("Compose")
                .RunAfter("TAA")
                .RunAfter("HBAO")
                .RunAfter("MotionBlur")
                .RunAfter("DepthOfField")
                .RunAfter("GodRays")
                .RunAfter("VolumetricClouds")
                .RunAfter("SSR")
                .RunAfter("SSGI")
                .RunBefore("Bloom")
                .RunBefore("AutoExposure");

            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/lensflare/vs.hlsl",
                GeometryShader = "effects/lensflare/gs.hlsl",
                PixelShader = "effects/lensflare/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Additive,
                Topology = PrimitiveTopology.PointList,
                BlendFactor = default,
                SampleMask = int.MaxValue
            }, macros);
            sampler = device.CreateSamplerState(SamplerStateDescription.PointClamp);

            lightBuffer = new(device, CpuAccessFlags.Write);

            lens0 = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lens/tex1.png"));
            lens1 = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lens/tex2.png"));
            lens2 = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lens/tex3.png"));
            lens3 = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lens/tex4.png"));
            lens4 = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lens/tex5.png"));
            lens5 = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lens/tex6.png"));
            lens6 = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/lens/tex7.png"));

            Viewport = new(width, height);
        }

        public void Resize(int width, int height)
        {
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            Output = view;
            Viewport = viewport;
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            Input = view;
        }

        public void Update(IGraphicsContext context)
        {
        }

        public unsafe void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (Output == null)
            {
                return;
            }

            var camera = CameraManager.Current;
            var lights = LightManager.Current;
            if (camera == null || lights == null)
            {
                return;
            }

            var depth = creator.GetDepthStencilBuffer("#DepthStencil");

            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (light is DirectionalLight directional)
                {
                    var camera_position = camera.Transform.GlobalPosition;

                    var translation = Matrix4x4.CreateTranslation(camera_position);

                    var far = camera.Transform.Far;
                    var light_position = Vector3.Transform(light.Transform.Backward * (far / 2f), translation);

                    var light_posH = Vector4.Transform(light_position, camera.Transform.ViewProjection);
                    var ss_sun_pos = new Vector4(0.5f * light_posH.X / light_posH.W + 0.5f, -0.5f * light_posH.Y / light_posH.W + 0.5f, light_posH.Z / light_posH.W, 1.0f);
                    lightBuffer.Update(context, ss_sun_pos);

                    nint* srvs = stackalloc nint[8];
                    srvs[0] = lens0.SRV.NativePointer;
                    srvs[1] = lens1.SRV.NativePointer;
                    srvs[2] = lens2.SRV.NativePointer;
                    srvs[3] = lens3.SRV.NativePointer;
                    srvs[4] = lens4.SRV.NativePointer;
                    srvs[5] = lens5.SRV.NativePointer;
                    srvs[6] = lens6.SRV.NativePointer;
                    srvs[7] = depth.SRV.NativePointer;

                    context.SetRenderTarget(Output, default);
                    context.SetViewport(Viewport);
                    context.GSSetConstantBuffer(0, lightBuffer);
                    context.GSSetShaderResources(0, 8, (void**)srvs);
                    context.PSSetShaderResources(0, 8, (void**)srvs);
                    context.GSSetSampler(0, sampler);
                    context.PSSetSampler(0, sampler);
                    context.SetGraphicsPipeline(pipeline);
                    context.DrawInstanced(7, 1, 0, 0);
                    context.ClearState();
                }
            }
        }

        public void Dispose()
        {
            pipeline.Dispose();
            sampler.Dispose();
            lightBuffer.Dispose();
            lens0.Dispose();
            lens1.Dispose();
            lens2.Dispose();
            lens3.Dispose();
            lens4.Dispose();
            lens5.Dispose();
            lens6.Dispose();
        }
    }
}