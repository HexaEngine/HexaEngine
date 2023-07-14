#nullable disable

using HexaEngine;

namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;

    public class LightCullPass : ComputePass
    {
        private bool recreateClusters = true;

        public const uint CLUSTERS_X = 16;
        public const uint CLUSTERS_Y = 16;
        public const uint CLUSTERS_Z = 16;

        public const uint CLUSTERS_X_THREADS = 16;
        public const uint CLUSTERS_Y_THREADS = 16;
        public const uint CLUSTERS_Z_THREADS = 4;

        public const uint CLUSTER_COUNT = CLUSTERS_X * CLUSTERS_Y * CLUSTERS_Z;

        public const uint CLUSTER_MAX_LIGHTS = 128;

        private StructuredUavBuffer<Cluster> ClusterBuffer;
        private StructuredUavBuffer<uint> LightIndexCounter;
        private StructuredUavBuffer<uint> LightIndexList;
        private StructuredUavBuffer<LightGrid> LightGridBuffer;

        private IComputePipeline clusterBuilding;
        private IComputePipeline clusterCulling;

        private ConstantBuffer<CullLightParams> lightParamsBuffer;
        private ConstantBuffer<ClusterSizes> clusterSizesBuffer;

        public LightCullPass() : base("LightCull")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("LightIndexList"));
            AddWriteDependency(new("LightGridBuffer"));
        }

        public override void Init(ResourceCreator creator, PipelineCreator pipelineCreator, IGraphicsDevice device)
        {
            float screenWidth = creator.GetViewport().Width;
            float screenHeight = creator.GetViewport().Height;

            clusterBuilding = pipelineCreator.CreateComputePipeline(new()
            {
                Path = "compute/clustered/building.hlsl"
            });
            clusterCulling = pipelineCreator.CreateComputePipeline(new()
            {
                Path = "compute/clustered/culling.hlsl"
            });

            lightParamsBuffer = new(device, CpuAccessFlags.Write);
            clusterSizesBuffer = new(device, new ClusterSizes(screenWidth / CLUSTERS_X, screenHeight / CLUSTERS_Y), CpuAccessFlags.Write);

            ClusterBuffer = creator.CreateStructuredUavBuffer<Cluster>("ClusterBuffer", CLUSTER_COUNT, CpuAccessFlags.None);
            LightIndexCounter = creator.CreateStructuredUavBuffer<uint>("LightIndexCounter", 1, CpuAccessFlags.None);
            LightIndexList = creator.CreateStructuredUavBuffer<uint>("LightIndexList", CLUSTER_COUNT * CLUSTER_MAX_LIGHTS, CpuAccessFlags.None);
            LightGridBuffer = creator.CreateStructuredUavBuffer<LightGrid>("LightGridBuffer", CLUSTER_COUNT, CpuAccessFlags.None);
        }

        public override unsafe void Execute(IGraphicsContext context, ResourceCreator creator)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var lightBuffer = current.LightManager.LightBuffer;

            context.CSSetConstantBuffer(1, creator.GetConstantBuffer<CBCamera>("CBCamera"));
            if (recreateClusters)
            {
                context.CSSetConstantBuffer(0, clusterSizesBuffer);
                context.CSSetUnorderedAccessView(0, (void*)ClusterBuffer.UAV.NativePointer);

                context.SetComputePipeline(clusterBuilding);
                context.Dispatch(CLUSTERS_X, CLUSTERS_Y, CLUSTERS_Z);
                context.SetComputePipeline(null);

                context.CSSetUnorderedAccessView(0, null);
                recreateClusters = false;
            }

            lightParamsBuffer.Update(context, new(lightBuffer.Count));
            context.CSSetConstantBuffer(0, lightParamsBuffer);

            nint* srvs = stackalloc nint[] { ClusterBuffer.SRV.NativePointer, lightBuffer.SRV.NativePointer };
            context.CSSetShaderResources(0, 2, (void**)srvs);
            nint* uavs = stackalloc nint[] { LightIndexCounter.UAV.NativePointer, LightIndexList.UAV.NativePointer, LightGridBuffer.UAV.NativePointer };
            context.CSSetUnorderedAccessViews(0, 3, (void**)uavs, null);

            context.SetComputePipeline(clusterCulling);
            context.Dispatch(CLUSTERS_X / CLUSTERS_X_THREADS, CLUSTERS_Y / CLUSTERS_Y_THREADS, CLUSTERS_Z / CLUSTERS_Z_THREADS);
            context.SetComputePipeline(null);

            ZeroMemory(srvs, sizeof(nint) * 2);
            context.CSSetShaderResources(0, 2, (void**)srvs);
            ZeroMemory(uavs, sizeof(nint) * 3);
            context.CSSetUnorderedAccessViews(0, 3, (void**)uavs, null);
            context.CSSetConstantBuffer(1, null);
            context.CSSetConstantBuffer(0, null);
        }
    }
}