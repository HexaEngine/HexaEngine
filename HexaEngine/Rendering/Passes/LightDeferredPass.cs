#nullable disable

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class LightDeferredPass : DrawPass
    {
        private ConstantBuffer<ProbeBufferParams> probeParamsBuffer;
        private ConstantBuffer<DeferredLightParams> lightParamsBuffer;

        private ISamplerState linearClampSampler;
        private ISamplerState linearWrapSampler;
        private ISamplerState pointClampSampler;
        private ISamplerState shadowSampler;

        private unsafe void** cbs;
        private const uint nConstantBuffers = 3;
        private unsafe void** smps;
        private const uint nSamplers = 4;

        private IGraphicsPipeline deferredIndirect;
        private unsafe void** indirectSrvs;
        private const uint nIndirectSrvsBase = 11;
        private const uint nIndirectSrvs = 13;

        private IGraphicsPipeline deferred;
        private unsafe void** deferredSrvs;
        private const uint nDeferredSrvs = 10;

        private IGraphicsPipeline deferredClusterd;
        private unsafe void** deferredClusterdSrvs;
        private const uint nDeferredClusterdSrvs = 12;

        private readonly bool forceForward = true;
        private readonly bool clustered = true;

        public LightDeferredPass() : base("LightDeferred")
        {
            AddWriteDependency(new("LightBuffer"));
            AddReadDependency(new("GBuffer"));
            AddReadDependency(new("#AOBuffer"));
            AddReadDependency(new("ShadowAtlas"));
            AddReadDependency(new("BRDFLUT"));
        }

        public override unsafe void Init(GraphResourceBuilder creator, GraphPipelineBuilder pipelineCreator, IGraphicsDevice device)
        {
            probeParamsBuffer = creator.CreateConstantBuffer<ProbeBufferParams>("ProbeBufferParams", CpuAccessFlags.Write);
            lightParamsBuffer = creator.CreateConstantBuffer<DeferredLightParams>("DeferredLightParams", CpuAccessFlags.Write);

            linearClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.LinearClamp);
            linearWrapSampler = creator.CreateSamplerState("LinearWrap", SamplerStateDescription.LinearWrap);
            pointClampSampler = creator.CreateSamplerState("PointClamp", SamplerStateDescription.PointClamp);
            shadowSampler = creator.CreateSamplerState("LinearComparisonBorder", SamplerStateDescription.ComparisonLinearBorder);

            smps = AllocArrayAndZero(nSamplers);
            smps[0] = (void*)linearClampSampler.NativePointer;
            smps[1] = (void*)linearWrapSampler.NativePointer;
            smps[2] = (void*)pointClampSampler.NativePointer;
            smps[3] = (void*)shadowSampler.NativePointer;

            cbs = AllocArrayAndZero(nConstantBuffers);
            cbs[1] = (void*)creator.GetConstantBuffer<CBCamera>("CBCamera").NativePointer;
            cbs[2] = (void*)creator.GetConstantBuffer<CBWeather>("CBWeather").NativePointer;

            indirectSrvs = AllocArrayAndZero(nIndirectSrvs);
            deferredSrvs = AllocArrayAndZero(nDeferredSrvs);
            deferredClusterdSrvs = AllocArrayAndZero(nDeferredClusterdSrvs);

            deferredIndirect = pipelineCreator.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "deferred/brdf/indirect.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleStrip
            });

            deferred = pipelineCreator.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "deferred/brdf/light.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleStrip
            });

            deferredClusterd = pipelineCreator.CreateGraphicsPipeline(new()
            {
                VertexShader = "quad.hlsl",
                PixelShader = "deferred/brdf/light.hlsl",
            },
            new GraphicsPipelineState()
            {
                Blend = BlendDescription.Additive,
                BlendFactor = Vector4.One,
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullBack,
                Topology = PrimitiveTopology.TriangleStrip
            }, new ShaderMacro[] { new("CLUSTERED_DEFERRED", 1) });
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator)
        {
            if (forceForward)
                return;

            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var lights = current.LightManager;
            var globalProbes = lights.GlobalProbes;

            var gbuffer = creator.GetGBuffer("GBuffer");
            for (int i = 0; i < 4; i++)
            {
                if (i < gbuffer.Count)
                {
                    deferredSrvs[i] = indirectSrvs[i] = deferredClusterdSrvs[i] = (void*)gbuffer.SRVs[i]?.NativePointer;
                }
            }

            deferredSrvs[4] = indirectSrvs[4] = deferredClusterdSrvs[4] = (void*)creator.GetDepthStencilBuffer("DepthStencil").SRV.NativePointer;
            indirectSrvs[5] = deferredSrvs[5] = deferredClusterdSrvs[5] = (void*)creator.GetTexture2D("#AOBuffer").SRV.NativePointer;
            indirectSrvs[9] = (void*)creator.GetTexture2D("BRDFLUT").SRV.NativePointer;
            indirectSrvs[10] = (void*)globalProbes.SRV.NativePointer;

            deferredSrvs[6] = deferredClusterdSrvs[6] = (void*)lights.LightBuffer.SRV.NativePointer;
            deferredSrvs[7] = deferredClusterdSrvs[7] = (void*)lights.ShadowDataBuffer.SRV.NativePointer;

            deferredClusterdSrvs[8] = (void*)creator.GetStructuredUavBuffer<uint>("LightIndexList").SRV.NativePointer;
            deferredClusterdSrvs[9] = (void*)creator.GetStructuredUavBuffer<LightGrid>("LightGridBuffer").SRV.NativePointer;

            deferredClusterdSrvs[10] = deferredSrvs[8] = (void*)creator.GetShadowAtlas("ShadowAtlas").SRV.NativePointer;

            context.SetRenderTarget(creator.GetTexture2D("LightBuffer").RTV, null);

            // Indirect light pass
            var probeParams = probeParamsBuffer.Local;
            probeParams->GlobalProbes = globalProbes.Count;
            probeParamsBuffer.Update(context);
            cbs[0] = (void*)probeParamsBuffer.Buffer?.NativePointer;

            context.PSSetSamplers(0, nSamplers, smps);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nIndirectSrvs, indirectSrvs);

            context.SetGraphicsPipeline(deferredIndirect);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(deferredIndirect);

            nint* null_samplers = stackalloc nint[4];
            context.PSSetSamplers(0, 4, (void**)null_samplers);

            nint* null_cbs = stackalloc nint[3];
            context.PSSetConstantBuffers(0, 3, (void**)null_cbs);

            nint* null_srvs = stackalloc nint[(int)nIndirectSrvs];
            context.PSSetShaderResources(0, nIndirectSrvs, (void**)null_srvs);

            if (clustered)
            {
                DeferredClustered(context, creator);
            }
            else
            {
                Deferred(context, creator, lights);
            }
        }

        private unsafe void Deferred(IGraphicsContext context, GraphResourceBuilder creator, LightManager lights)
        {
            // Direct light pass
            var lightParams = lightParamsBuffer.Local;
            lightParams->LightCount = lights.LightBuffer.Count;
            lightParamsBuffer.Update(context);
            cbs[0] = (void*)lightParamsBuffer.Buffer?.NativePointer;

            context.PSSetSamplers(0, nSamplers, smps);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nDeferredSrvs, deferredSrvs);

            context.SetGraphicsPipeline(deferred);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

            nint* null_srvs = stackalloc nint[(int)nDeferredSrvs];
            context.PSSetShaderResources(0, nDeferredSrvs, (void**)null_srvs);
        }

        private unsafe void DeferredClustered(IGraphicsContext context, GraphResourceBuilder creator)
        {
            // Direct clusterd light pass
            context.PSSetSamplers(0, nSamplers, smps);
            context.PSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetShaderResources(0, nDeferredClusterdSrvs, deferredClusterdSrvs);

            context.SetGraphicsPipeline(deferredClusterd);
            context.DrawInstanced(4, 1, 0, 0);
            context.SetGraphicsPipeline(null);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(0, nSamplers, (void**)null_samplers);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);

            nint* null_srvs = stackalloc nint[(int)nDeferredClusterdSrvs];
            context.PSSetShaderResources(0, nDeferredClusterdSrvs, (void**)null_srvs);
        }
    }
}