﻿namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Profiling;
    using System.Collections.Generic;
    using System.Numerics;

    public class HizDepthPass : ComputePass
    {
        private ResourceRef<DepthStencil> depthStencil;
        private ResourceRef<IComputePipelineState> downsample;
        private ResourceRef<ConstantBuffer<Vector4>> cbDownsample;
        private ResourceRef<IGraphicsPipelineState> copy;
        private ResourceRef<ISamplerState> samplerState;
        private ResourceRef<DepthMipChain> chain;
        private string[] names;
        private IResourceBindingList[] lists;

        public HizDepthPass() : base("HiZDepth")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("HiZBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            depthStencil = creator.GetDepthStencilBuffer("#DepthStencil");
            downsample = creator.CreateComputePipelineState(new()
            {
                Path = "compute/hiz/shader.hlsl",
            });

            copy = creator.CreateGraphicsPipelineState(new(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "effects/copy/ps.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreenF));

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

        public override void Prepare(GraphResourceBuilder creator)
        {
            copy.Value!.Bindings.SetSRV("sourceTex", depthStencil.Value!.SRV!);

            var chain = this.chain.Value!;
            var uavs = chain.UAVs;
            var srvs = chain.SRVs;

            var bindings = downsample.Value!.Bindings;
            bindings.SetCBV("params", cbDownsample.Value!);
            bindings.SetSampler("samplerPoint", samplerState.Value!);

            // Set these so that automatic cleanup is performed of the UAV and SRV after the loop.
            // This is far more optimal than clearing it inside of the loop explicitly.
            // It avoids excessive state changes in backends like D3D11.
            bindings.SetUAV("output", uavs[1]);
            bindings.SetSRV("input", srvs[0]);

            lists = new IResourceBindingList[chain.MipLevels];

            for (int i = 1; i < lists.Length; i++)
            {
                var list = creator.Device.CreateResourceBindingList(downsample.Value.Pipeline!);
                list.SetUAV("output", uavs[i]);
                list.SetSRV("input", srvs[i - 1]);
                lists[i] = list;
            }
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var chain = this.chain.Value!;
            var viewports = chain.Viewports;

            profiler?.Begin("HizDepthPass.CopyEffect");

            context.SetRenderTarget(chain.RTV, null);
            context.SetViewport(viewports[0]);
            context.SetGraphicsPipelineState(copy.Value);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipelineState(null);
            context.SetRenderTarget(null, null);

            context.SetComputePipelineState(downsample.Value);

            profiler?.End("HizDepthPass.CopyEffect");

            for (uint i = 1; i < chain.MipLevels; i++)
            {
                string name = names[i - 1];
                profiler?.Begin(name);
                Vector2 texel = new(1 / viewports[i].Width, 1 / viewports[i].Height);
                context.Write(cbDownsample.Value!, new Vector4(texel, 0, 0));
                context.SetResourceBindingList(lists[i]);
                context.Dispatch((uint)viewports[i].Width / 32 + 1, (uint)viewports[i].Height / 32 + 1, 1);
                profiler?.End(name);
            }

            context.SetComputePipelineState(null);
        }
    }
}