namespace HexaEngine.Effects
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Lights.Types;
    using HexaEngine.Core.PostFx;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public class GodRays : IPostFx
    {
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ISamplerState sampler;
        private ConstantBuffer<GodRaysParams> paramsBuffer;

        private Texture2D sunsprite;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;
        private bool enabled = true;
        private int priority = 98;

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public string Name => "GodRays";

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

        public struct GodRaysParams
        {
            public Vector4 ScreenSpacePosition;
            public float GodraysDensity;
            public float GodraysWeight;
            public float GodraysDecay;
            public float GodraysExposure;
        }

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            quad = new Quad(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/godrays/vs.hlsl",
                PixelShader = "effects/godrays/ps.hlsl"
            },
            new GraphicsPipelineState()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Blend = BlendDescription.Additive,
                Topology = PrimitiveTopology.TriangleList,
                BlendFactor = default,
                SampleMask = int.MaxValue
            }, macros);
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);

            paramsBuffer = new(device, CpuAccessFlags.Write);

            sunsprite = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/sun/sunsprite.png"));

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
            var camera = CameraManager.Current;
            var lights = LightManager.Current;
            if (camera == null || lights == null)
            {
                return;
            }

            for (int i = 0; i < lights.ActiveCount; i++)
            {
                var light = lights.Active[i];
                if (light is DirectionalLight)
                {
                    GodRaysParams raysParams = default;

                    raysParams.GodraysDecay = 0.825f;
                    raysParams.GodraysWeight = 0.25f;
                    raysParams.GodraysDensity = 0.975f;
                    raysParams.GodraysExposure = 2.0f;

                    var camera_position = camera.Transform.GlobalPosition;

                    var light_position = Vector4.Transform(new Vector4(light.Transform.GlobalPosition, 1), Matrix4x4.CreateTranslation(camera_position.X, 0.0f, camera_position.Z));

                    var light_posH = Vector4.Transform(new Vector4(light.Transform.GlobalPosition, 1), camera.Transform.ViewProjection);
                    var ss_sun_pos = new Vector4(0.5f * light_posH.X / light_posH.W + 0.5f, -0.5f * light_posH.Y / light_posH.W + 0.5f, light_posH.Z / light_posH.W, 1.0f);

                    raysParams.ScreenSpacePosition = ss_sun_pos;

                    paramsBuffer.Set(context, raysParams);
                }
            }
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null)
            {
                return;
            }

            context.SetRenderTarget(Output, default);
            context.SetViewport(Viewport);
            context.PSSetConstantBuffer(paramsBuffer, 0);
            context.PSSetShaderResource(sunsprite.SRV, 0);
            context.PSSetSampler(sampler, 0);
            quad.DrawAuto(context, pipeline);
            context.ClearState();
        }

        public void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
            paramsBuffer.Dispose();
        }
    }
}