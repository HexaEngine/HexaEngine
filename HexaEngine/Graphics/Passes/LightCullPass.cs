namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Profiling;
    using HexaEngine.Scenes;

    public class LightCullPass : ComputePass
    {
        private bool recreateClusters = true;

        public const uint CLUSTERS_X = 16;
        public const uint CLUSTERS_Y = 9;
        public const uint CLUSTERS_Z = 24;

        public const uint CLUSTERS_X_THREADS = 16;
        public const uint CLUSTERS_Y_THREADS = 9;
        public const uint CLUSTERS_Z_THREADS = 4;

        public const uint CLUSTER_COUNT = CLUSTERS_X * CLUSTERS_Y * CLUSTERS_Z;

        public const uint CLUSTER_MAX_LIGHTS = 128;

        private ResourceRef<StructuredUavBuffer<Cluster>> ClusterBuffer;
        private ResourceRef<StructuredUavBuffer<uint>> LightIndexCounter;
        private ResourceRef<StructuredUavBuffer<uint>> LightIndexList;
        private ResourceRef<StructuredUavBuffer<LightGrid>> LightGridBuffer;

        private ResourceRef<IComputePipeline> clusterBuilding;
        private ResourceRef<IComputePipeline> clusterCulling;

        private ResourceRef<ConstantBuffer<CullLightParams>> lightParamsBuffer;
        private ResourceRef<ConstantBuffer<ClusterSizes>> clusterSizesBuffer;
        private ResourceRef<ConstantBuffer<CBCamera>> camera;

        public LightCullPass() : base("LightCull")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("LightIndexList"));
            AddWriteDependency(new("LightGridBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            camera = creator.GetConstantBuffer<CBCamera>("CBCamera");

            float screenWidth = creator.Viewport.Width;
            float screenHeight = creator.Viewport.Height;

            clusterBuilding = creator.CreateComputePipeline(new()
            {
                Path = "compute/clustered/building.hlsl"
            });
            clusterCulling = creator.CreateComputePipeline(new()
            {
                Path = "compute/clustered/culling.hlsl"
            });

            lightParamsBuffer = creator.CreateConstantBuffer<CullLightParams>("CBCullLightParams", CpuAccessFlags.Write);

            clusterSizesBuffer = creator.CreateConstantBuffer("CBClusterSizes", new ClusterSizes(CLUSTERS_X, CLUSTERS_Y, CLUSTERS_Z, (uint)MathF.Ceiling(screenWidth / CLUSTERS_X)), CpuAccessFlags.Write);

            ClusterBuffer = creator.CreateStructuredUavBuffer<Cluster>("ClusterBuffer", CLUSTER_COUNT, CpuAccessFlags.None);
            LightIndexCounter = creator.CreateStructuredUavBuffer<uint>("LightIndexCounter", 1, CpuAccessFlags.None);
            LightIndexList = creator.CreateStructuredUavBuffer<uint>("LightIndexList", CLUSTER_COUNT * CLUSTER_MAX_LIGHTS, CpuAccessFlags.None);
            LightGridBuffer = creator.CreateStructuredUavBuffer<LightGrid>("LightGridBuffer", CLUSTER_COUNT, CpuAccessFlags.None);
        }

        public override void OnResize(GraphResourceBuilder creator)
        {
            recreateClusters = true;
            base.OnResize(creator);
        }

        public override unsafe void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var lights = current.LightManager;
            var lightBuffer = current.LightManager.LightBuffer;

            profiler?.Begin("LightCull.Update");

            lights.GlobalProbes.Update(context);
            lights.LightBuffer.Update(context);
            lights.ShadowDataBuffer.Update(context);

            context.CSSetConstantBuffer(1, camera.Value);
            if (recreateClusters)
            {
                profiler?.Begin("RecreateClusters");
                context.CSSetConstantBuffer(0, clusterSizesBuffer.Value);
                context.CSSetUnorderedAccessView(0, (void*)ClusterBuffer.Value.UAV.NativePointer);

                context.SetComputePipeline(clusterBuilding.Value);
                context.Dispatch(CLUSTERS_X, CLUSTERS_Y, CLUSTERS_Z);
                context.SetComputePipeline(null);

                context.CSSetUnorderedAccessView(0, null);
                recreateClusters = false;
                profiler?.End("RecreateClusters");
            }

            lightParamsBuffer.Value.Update(context, new(lightBuffer.Count));

            profiler?.End("LightCull.Update");

            profiler?.Begin("LightCull.Dispatch");

            context.CSSetConstantBuffer(0, lightParamsBuffer.Value);

            nint* srvs = stackalloc nint[] { ClusterBuffer.Value.SRV.NativePointer, lightBuffer.SRV.NativePointer };
            context.CSSetShaderResources(0, 2, (void**)srvs);
            nint* uavs = stackalloc nint[] { LightIndexCounter.Value.UAV.NativePointer, LightIndexList.Value.UAV.NativePointer, LightGridBuffer.Value.UAV.NativePointer };
            context.CSSetUnorderedAccessViews(0, 3, (void**)uavs, null);

            context.SetComputePipeline(clusterCulling.Value);
            context.Dispatch(CLUSTERS_X / CLUSTERS_X_THREADS, CLUSTERS_Y / CLUSTERS_Y_THREADS, CLUSTERS_Z / CLUSTERS_Z_THREADS);
            context.SetComputePipeline(null);

            ZeroMemory(srvs, sizeof(nint) * 2);
            context.CSSetShaderResources(0, 2, (void**)srvs);
            ZeroMemory(uavs, sizeof(nint) * 3);
            context.CSSetUnorderedAccessViews(0, 3, (void**)uavs, null);
            context.CSSetConstantBuffer(1, null);
            context.CSSetConstantBuffer(0, null);

            profiler?.End("LightCull.Dispatch");
        }
    }
}