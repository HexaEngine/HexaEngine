#nullable disable

using HexaEngine;

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Rendering.Graph;
    using System.Numerics;

    public class HizDepthPass : ComputePass
    {
        private Quad quad;
        private IComputePipeline downsample;
        private ConstantBuffer<Vector4> cbDownsample;
        private IGraphicsPipeline copy;
        private ISamplerState samplerState;

        public HizDepthPass() : base("HiZDepth")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("HiZBuffer"));
        }

        public override void Init(ResourceCreator creator, PipelineCreator pipelineCreator, IGraphicsDevice device)
        {
            quad = new(device);

            downsample = pipelineCreator.CreateComputePipeline(new()
            {
                Path = "compute/hiz/shader.hlsl",
            });

            copy = pipelineCreator.CreateGraphicsPipeline(new()
            {
                VertexShader = "effects/copy/vs.hlsl",
                PixelShader = "effects/copy/ps.hlsl"
            });

            cbDownsample = creator.CreateConstantBuffer<Vector4>("HiZDownsampleCB", CpuAccessFlags.Write);
            samplerState = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            creator.CreateDepthMipChain("HiZBuffer", new((int)creator.GetViewport().Width, (int)creator.GetViewport().Height, 1, Format.R32Float, BindFlags.None, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default));
        }

        public override unsafe void Execute(IGraphicsContext context, ResourceCreator creator)
        {
            var input = creator.GetDepthStencilBuffer("#DepthStencil").SRV;
            var chain = creator.GetDepthMipChain("HiZBuffer");
            var viewports = chain.Viewports;
            var uavs = chain.UAVs;
            var srvs = chain.SRVs;

            context.SetRenderTarget(chain.RTV, null);
            context.PSSetShaderResource(0, input);
            context.SetViewport(viewports[0]);
            quad.DrawAuto(context, copy);
            context.SetRenderTarget(null, null);

            context.SetComputePipeline(downsample);
            context.CSSetConstantBuffer(0, cbDownsample);
            context.CSSetSampler(0, samplerState);

            for (uint i = 1; i < chain.Mips; i++)
            {
                Vector2 texel = new(viewports[i].Width, viewports[i].Height);
                context.Write(cbDownsample, new Vector4(texel, 0, 0));
                context.CSSetUnorderedAccessView((void*)uavs[i].NativePointer);
                context.CSSetShaderResource(0, srvs[i - 1]);
                context.Dispatch((uint)viewports[i].Width / 32 + 1, (uint)viewports[i].Height / 32 + 1, 1);
            }

            context.CSSetUnorderedAccessView(null);
            context.CSSetShaderResource(0, null);
            context.CSSetSampler(0, null);
            context.CSSetConstantBuffer(0, null);
            context.SetComputePipeline(null);
        }
    }
}