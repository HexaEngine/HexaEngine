namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
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

        private ResourceRef<IComputePipelineState> clusterBuilding;
        private ResourceRef<IComputePipelineState> clusterCulling;

        private ResourceRef<ConstantBuffer<CullLightParams>> lightParamsBuffer;

        public LightCullPass() : base("LightCull")
        {
            AddReadDependency(new("#DepthStencil"));
            AddWriteDependency(new("LightIndexList"));
            AddWriteDependency(new("LightGridBuffer"));
        }

        public override void Init(GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            float screenWidth = creator.Viewport.Width;
            float screenHeight = creator.Viewport.Height;

            clusterBuilding = creator.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "compute/clustered/building.hlsl"
            });
            clusterCulling = creator.CreateComputePipelineState(new ComputePipelineDesc()
            {
                Path = "compute/clustered/culling.hlsl"
            });

            lightParamsBuffer = creator.CreateConstantBuffer<CullLightParams>("CBCullLightParams", CpuAccessFlags.Write);

            ClusterBuffer = creator.CreateStructuredUavBuffer<Cluster>("ClusterBuffer", CLUSTER_COUNT, CpuAccessFlags.None);
            LightIndexCounter = creator.CreateStructuredUavBuffer<uint>("LightIndexCounter", 1, CpuAccessFlags.None);
            LightIndexList = creator.CreateStructuredUavBuffer<uint>("LightIndexList", CLUSTER_COUNT * CLUSTER_MAX_LIGHTS, CpuAccessFlags.None);
            LightGridBuffer = creator.CreateStructuredUavBuffer<LightGrid>("LightGridBuffer", CLUSTER_COUNT, CpuAccessFlags.None);

            LightManager.ActiveLightsChanged += ActiveLightsChanged;
            if (LightManager.Current != null)
            {
                ActiveLightsChanged(LightManager.Current, new(LightManager.Current));
            }
        }

        private void ActiveLightsChanged(object? sender, ActiveLightsChangedEventArgs e)
        {
            var bindings = clusterCulling.Value!.Bindings;
            bindings.SetSRV("lights", e.LightManager.LightBuffer.SRV!);
        }

        public override void Prepare(GraphResourceBuilder creator)
        {
            {
                var bindings = clusterBuilding.Value!.Bindings;
                bindings.SetUAV("clusters", ClusterBuffer.Value!.UAV!);
            }

            {
                var lightBuffer = LightManager.Current?.LightBuffer;
                var bindings = clusterCulling.Value!.Bindings;
                bindings.SetCBV("CBCullingParams", lightParamsBuffer.Value!);
                bindings.SetSRV("clusters", ClusterBuffer.Value.SRV);
                bindings.SetSRV("lights", lightBuffer?.SRV!);
                bindings.SetUAV("lightIndexCounter", LightIndexCounter.Value!.UAV!);
                bindings.SetUAV("lightIndexList", LightIndexList.Value!.UAV!);
                bindings.SetUAV("lightGrid", LightGridBuffer.Value!.UAV!);
            }
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

            if (recreateClusters)
            {
                profiler?.Begin("RecreateClusters");

                context.SetComputePipelineState(clusterBuilding.Value);
                context.Dispatch(CLUSTERS_X, CLUSTERS_Y, CLUSTERS_Z);
                context.SetComputePipelineState(null);

                recreateClusters = false;
                profiler?.End("RecreateClusters");
            }

            lightParamsBuffer.Value!.Update(context, new(lightBuffer.Count));

            profiler?.End("LightCull.Update");

            profiler?.Begin("LightCull.Dispatch");

            context.SetComputePipelineState(clusterCulling.Value);
            context.Dispatch(CLUSTERS_X / CLUSTERS_X_THREADS, CLUSTERS_Y / CLUSTERS_Y_THREADS, CLUSTERS_Z / CLUSTERS_Z_THREADS);
            context.SetComputePipelineState(null);

            profiler?.End("LightCull.Dispatch");
        }
    }
}