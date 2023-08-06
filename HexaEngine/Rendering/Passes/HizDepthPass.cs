namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class HizDepthPass : ComputePass
    {
        private IComputePipeline downsample;
        private ConstantBuffer<Vector4> cbDownsample;
        private IGraphicsPipeline copy;
        private ISamplerState samplerState;

        public HizDepthPass() : base("HiZDepth")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("HiZBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device, ICPUProfiler? profiler)
        {
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
            creator.CreateDepthMipChain("HiZBuffer", new((int)creator.Viewport.Width, (int)creator.Viewport.Height, 1, Format.R32Float));
            var mips = DepthMipChain.GetNumMipLevels((int)creator.Viewport.Width, (int)creator.Viewport.Height);
            for (int i = 1; i < mips; i++)
            {
                profiler?.CreateStage($"HizDepthPass.{i}x");
            }
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var input = creator.GetDepthStencilBuffer("#DepthStencil").SRV;
            var chain = creator.GetDepthMipChain("HiZBuffer");
            var viewports = chain.Viewports;
            var uavs = chain.UAVs;
            var srvs = chain.SRVs;

            profiler?.Begin($"HizDepthPass.Copy");

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
            context.CSSetConstantBuffer(0, cbDownsample);
            context.CSSetSampler(0, samplerState);

            profiler?.End($"HizDepthPass.Copy");

            for (uint i = 1; i < chain.Mips; i++)
            {
                profiler?.Begin($"HizDepthPass.{i}x");
                profiler?.Begin($"HizDepthPass.{i}x.Update");
                Vector2 texel = new(viewports[i].Width, viewports[i].Height);
                context.Write(cbDownsample, new Vector4(texel, 0, 0));
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