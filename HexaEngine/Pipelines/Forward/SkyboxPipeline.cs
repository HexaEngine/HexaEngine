﻿namespace HexaEngine.Pipelines.Forward
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;

    public unsafe class Skybox : IEffect
    {
        private readonly UVSphere sphere;
        private readonly GraphicsPipeline pipeline;
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
            pipeline = new(device, new()
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
            if (Output == null) return;

            context.SetRenderTarget(Output, DSV);
            context.PSSetSampler(sampler, 0);
            context.VSSetConstantBuffers(cbs, 2, 0);
            context.PSSetShaderResource(Env, 0);
            sphere.DrawAuto(context, pipeline, Output.Viewport);
            context.ClearState();
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

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}