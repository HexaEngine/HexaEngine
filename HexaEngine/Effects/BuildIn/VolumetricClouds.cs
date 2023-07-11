namespace HexaEngine.Effects.BuildIn
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Resources;
    using HexaEngine.Effects.Blur;
    using HexaEngine.Mathematics;
    using HexaEngine.PostFx;
    using HexaEngine.Weather;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    public class VolumetricClouds : IPostFx
    {
        private IGraphicsDevice device;
        private Quad quad;
        private IGraphicsPipeline pipeline;
        private ISamplerState linearWrapSampler;
        private ISamplerState pointWrapSampler;

        private Texture2D weatherTex;
        private Texture3D cloudTex;
        private Texture3D worleyTex;

        private Texture2D intermediateTex;
        private GaussianBlur gaussianBlur;

        public ResourceRef<IBuffer> Camera;
        public ResourceRef<IBuffer> Weather;
        public ResourceRef<IShaderResourceView> Depth;

        public IRenderTargetView Output;
        public IShaderResourceView Input;
        public Viewport Viewport;

        private bool enabled = true;
        private int priority = 340;

        public string Name { get; } = "Volumetric Clouds";

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

        public event Action<bool>? OnEnabledChanged;

        public event Action<int>? OnPriorityChanged;

        public async Task Initialize(IGraphicsDevice device, int width, int height, ShaderMacro[] macros)
        {
            this.device = device;
            quad = new(device);
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "effects/clouds/vs.hlsl",
                PixelShader = "effects/clouds/ps.hlsl",
            }, new GraphicsPipelineState()
            {
                Blend = BlendDescription.AlphaBlend,
                BlendFactor = Vector4.One,
            });

            linearWrapSampler = device.CreateSamplerState(SamplerDescription.LinearWrap);
            pointWrapSampler = device.CreateSamplerState(SamplerDescription.PointWrap);

            weatherTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/weather.dds"));
            cloudTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/cloud.dds"));
            worleyTex = new(device, new TextureFileDescription(Paths.CurrentAssetsPath + "textures/clouds/worley.dds"));

            Camera = ResourceManager2.Shared.GetBuffer("CBCamera");
            Weather = ResourceManager2.Shared.GetBuffer("CBWeather");
            Depth = ResourceManager2.Shared.GetResource<IShaderResourceView>("GBuffer.Depth");

            intermediateTex = new(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            gaussianBlur = new(device, Format.R16G16B16A16Float, width, height);
        }

        public void Update(IGraphicsContext context)
        {
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output == null || WeatherManager.Current == null || !WeatherManager.Current.HasSun)
            {
                return;
            }

            context.ClearRenderTargetView(intermediateTex.RTV, default);
            context.SetRenderTarget(intermediateTex.RTV, default);
            context.SetViewport(Viewport);
            nint* srvs = stackalloc nint[] { weatherTex.SRV.NativePointer, cloudTex.SRV.NativePointer, worleyTex.SRV.NativePointer, Depth.Value.NativePointer };
            context.PSSetShaderResources(0, 4, (void**)srvs);
            nint* smps = stackalloc nint[] { linearWrapSampler.NativePointer, pointWrapSampler.NativePointer };
            context.PSSetSamplers(0, 2, (void**)smps);
            nint* cbcs = stackalloc nint[] { Camera.Value.NativePointer, Weather.Value.NativePointer };
            context.PSSetConstantBuffers(1, 2, (void**)cbcs);

            quad.DrawAuto(context, pipeline);

            context.ClearState();

            gaussianBlur.Blur(context, intermediateTex.SRV, Output, (int)Viewport.Width, (int)Viewport.Height);
        }

        public void Resize(int width, int height)
        {
            intermediateTex.Resize(device, Format.R16G16B16A16Float, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            gaussianBlur.Resize(Format.R16G16B16A16Float, width, height);
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

        public void Dispose()
        {
            quad.Dispose();
            pipeline.Dispose();
            linearWrapSampler.Dispose();
            pointWrapSampler.Dispose();
            weatherTex.Dispose();
            cloudTex.Dispose();
            worleyTex.Dispose();

            intermediateTex.Dispose();
            gaussianBlur.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}