namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graph;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class HizDepthPass : ComputePass
    {
        private ResourceRef<DepthStencil> depthStencil;
        private IComputePipeline downsample;
        private ResourceRef<ConstantBuffer<Vector4>> cbDownsample;
        private IGraphicsPipeline copy;
        private ResourceRef<ISamplerState> samplerState;
        private ResourceRef<DepthMipChain> chain;

        public HizDepthPass() : base("HiZDepth")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("HiZBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            downsample = pipelineCreator.CreateComputePipeline(new()
            {
                Path = "compute/hiz/shader.hlsl",
            });

            copy = pipelineCreator.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl"
            }, GraphicsPipelineState.DefaultFullscreen);

            cbDownsample = creator.CreateConstantBuffer<Vector4>("HiZDownsampleCB", CpuAccessFlags.Write);
            samplerState = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            chain = creator.CreateDepthMipChain("HiZBuffer", new((int)creator.Viewport.Width, (int)creator.Viewport.Height, 1, Math.Min(TextureHelper.ComputeMipLevels((int)creator.Viewport.Width, (int)creator.Viewport.Height), 8), Format.R32Float));
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var input = depthStencil.Value.SRV;
            var chain = this.chain.Value;
            var viewports = chain.Viewports;
            var uavs = chain.UAVs;
            var srvs = chain.SRVs;

            profiler?.Begin($"HizDepthPass.CopyEffect");

            context.SetRenderTarget(chain.RTV, null);
            context.PSSetShaderResource(0, input);
            context.SetViewport(viewports[0]);
            context.SetGraphicsPipeline(copy);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);
            context.SetRenderTarget(null, null);

            context.SetRenderTarget(null, null);
            context.PSSetShaderResource(0, null);

            context.SetComputePipeline(downsample);
            context.CSSetConstantBuffer(0, cbDownsample.Value);
            context.CSSetSampler(0, samplerState.Value);

            profiler?.End($"HizDepthPass.CopyEffect");

            for (uint i = 1; i < chain.MipLevels; i++)
            {
                profiler?.Begin($"HizDepthPass.{i}x");
                profiler?.Begin($"HizDepthPass.{i}x.Update");
                Vector2 texel = new(1 / viewports[i].Width, 1 / viewports[i].Height);
                context.Write(cbDownsample.Value, new Vector4(texel, 0, 0));
                profiler?.End($"HizDepthPass.{i}x.Update");
                context.CSSetUnorderedAccessView((void*)uavs[i].NativePointer);
                context.CSSetShaderResource(0, srvs[i - 1]);
                context.Dispatch((uint)viewports[i].Width / 32 + 1, (uint)viewports[i].Height / 32 + 1, 1);
                profiler?.End($"HizDepthPass.{i}x");
            }

            context.CSSetUnorderedAccessView(null);
            context.CSSetShaderResource(0, null);
            context.CSSetSampler(0, null);
            context.CSSetConstantBuffer(0, null);
            context.SetComputePipeline(null);
        }
    }
}