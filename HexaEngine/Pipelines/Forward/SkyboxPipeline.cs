namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;

    public unsafe class Skybox : IEffect
    {
        private readonly UVSphere sphere;
        private readonly IGraphicsPipeline pipeline;
        private readonly ISamplerState sampler;
        private readonly void** cbs;

        public IRenderTargetView? Output;
        public IDepthStencilView? DSV;
        public IShaderResourceView? Env;
        public IBuffer? Camera;
        public IBuffer? World;

        public Skybox(IGraphicsDevice device)
        {
            sphere = new(device);
            pipeline = device.CreateGraphicsPipeline(new()
            {
                VertexShader = "forward/skybox/vs.hlsl",
                PixelShader = "forward/skybox/ps.hlsl"
            });
            pipeline.State = new GraphicsPipelineState()
            {
                Rasterizer = RasterizerDescription.CullNone,
                Topology = PrimitiveTopology.TriangleList,
                Blend = BlendDescription.Opaque,
                DepthStencil = DepthStencilDescription.Default,
            };
            sampler = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            cbs = AllocArray(2);
        }

        public void Resize()
        {
#nullable disable
            cbs[0] = (void*)World?.NativePointer;
            cbs[1] = (void*)Camera?.NativePointer;
#nullable enable
        }

        public void Draw(IGraphicsContext context)
        {
        }

        public void Dispose()
        {
            sphere.Dispose();
            pipeline.Dispose();
            sampler.Dispose();
            Free(cbs);
            GC.SuppressFinalize(this);
        }

        public void BeginResize()
        {
            throw new NotImplementedException();
        }

        public void EndResize(int width, int height)
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.

        public async Task Initialize(IGraphicsDevice device, int width, int height)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            throw new NotImplementedException();
        }
    }
}