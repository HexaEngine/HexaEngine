namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using System.Numerics;

    public class HizDepthPass : ComputePass
    {
        private ResourceRef<DepthStencil> depthStencil;
        private ResourceRef<IComputePipeline> downsample;
        private ResourceRef<ConstantBuffer<Vector4>> cbDownsample;
        private ResourceRef<IGraphicsPipelineState> copy;
        private ResourceRef<ISamplerState> samplerState;
        private ResourceRef<DepthMipChain> chain;
        private string[] names;

        public HizDepthPass() : base("HiZDepth")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("HiZBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            downsample = creator.CreateComputePipeline(new()
            {
                Path = "compute/hiz/shader.hlsl",
            });

            copy = creator.CreateGraphicsPipelineState(new(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen));

            cbDownsample = creator.CreateConstantBuffer<Vector4>("HiZDownsampleCB", CpuAccessFlags.Write);
            samplerState = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            int mipLevels = Math.Min(TextureHelper.ComputeMipLevels((int)creator.Viewport.Width, (int)creator.Viewport.Height), 16);
            chain = creator.CreateDepthMipChain("HiZBuffer", new((int)creator.Viewport.Width, (int)creator.Viewport.Height, 1, mipLevels, Format.R32Float));

            names = new string[mipLevels];
            for (int i = 0; i < mipLevels; i++)
            {
                names[i] = $"HizDepthPass.{i + 1}x";
            }
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var input = depthStencil.Value.SRV;
            var chain = this.chain.Value;
            var viewports = chain.Viewports;
            var uavs = chain.UAVs;
            var srvs = chain.SRVs;

            profiler?.Begin("HizDepthPass.CopyEffect");

            context.SetRenderTarget(chain.RTV, null);
            context.PSSetShaderResource(0, input);
            context.SetViewport(viewports[0]);
            context.SetPipelineState(copy.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetPipelineState(null);
            context.SetRenderTarget(null, null);

            context.SetRenderTarget(null, null);
            context.PSSetShaderResource(0, null);

            context.SetComputePipeline(downsample.Value);
            context.CSSetConstantBuffer(0, cbDownsample.Value);
            context.CSSetSampler(0, samplerState.Value);

            profiler?.End("HizDepthPass.CopyEffect");

            for (uint i = 1; i < chain.MipLevels; i++)
            {
                string name = names[(i - 1)];
                profiler?.Begin(name);
                Vector2 texel = new(1 / viewports[i].Width, 1 / viewports[i].Height);
                context.Write(cbDownsample.Value, new Vector4(texel, 0, 0));
                context.CSSetUnorderedAccessView((void*)uavs[i].NativePointer);
                context.CSSetShaderResource(0, srvs[i - 1]);
                context.Dispatch((uint)viewports[i].Width / 32 + 1, (uint)viewports[i].Height / 32 + 1, 1);
                profiler?.End(name);
            }

            context.CSSetUnorderedAccessView(null);
            context.CSSetShaderResource(0, null);
            context.CSSetSampler(0, null);
            context.CSSetConstantBuffer(0, null);
            context.SetComputePipeline(null);
        }
    }
}