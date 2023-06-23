namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes.Managers;
    using System.Numerics;

    public class Skybox : IDisposable
    {
        private readonly IGraphicsDevice device;
        private readonly Cube cube;
        private IGraphicsPipeline pipeline;
        private readonly ISamplerState anisotropicClampSampler;

        private readonly ConstantBuffer<CBWorld> worldBuffer;

        private Texture2D environment;

        private readonly bool loaded;
        private bool disposedValue;

        public Skybox(IGraphicsDevice device)
        {
            this.device = device;
            cube = new(device);

            anisotropicClampSampler = ResourceManager2.Shared.GetOrAddSamplerState("AnisotropicClamp", SamplerDescription.AnisotropicClamp).Value;
            worldBuffer = new(device, CpuAccessFlags.Write);
        }

        public void Load(string environmentPath)
        {
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/skybox/vs.hlsl",
                PixelShader = "forward/skybox/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Rasterizer = RasterizerDescription.CullNone,
            });

            environment = new(device, new(environmentPath, TextureDimension.TextureCube, 0, Usage.Immutable));
        }

        public async Task LoadAsync(string environmentPath)
        {
            pipeline = await device.CreateGraphicsPipelineAsync(new()
            {
                VertexShader = "forward/skybox/vs.hlsl",
                PixelShader = "forward/skybox/ps.hlsl",
            },
            new GraphicsPipelineState()
            {
                Rasterizer = RasterizerDescription.CullNone,
            });

            environment = new(device, new(environmentPath, TextureDimension.TextureCube, 0, Usage.Immutable));
        }

        public void Unload()
        {
            environment.Dispose();
        }

        public void Update(IGraphicsContext context)
        {
            var camera = CameraManager.Current;
            if (camera == null)
            {
                return;
            }

            worldBuffer.Update(context, new(Matrix4x4.CreateScale(camera.Transform.Far / 2) * Matrix4x4.CreateTranslation(camera.Transform.Position)));
        }

        public void Draw(IGraphicsContext context, IBuffer camera)
        {
            if (!loaded)
                return;

            context.VSSetConstantBuffer(worldBuffer, 0);
            context.VSSetConstantBuffer(camera, 1);
            context.PSSetShaderResource(environment.SRV, 0);
            context.PSSetSampler(anisotropicClampSampler, 0);
            cube.DrawAuto(context, pipeline);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        ~Skybox()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}