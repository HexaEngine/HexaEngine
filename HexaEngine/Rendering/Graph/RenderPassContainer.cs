namespace HexaEngine.Rendering.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RenderPassContainer<ContentMediator>
    {   /*
        private Dictionary<Name, PassHelpers> mRenderPasses = new();
        private Dictionary<Name, PassHelpers> mRenderSubPasses = new();

        private struct PassHelpers
        {
            public PassHelpers(
                RenderPass<ContentMediator> renderPass,
                RenderDevice renderDevice,
                PipelineResourceStorage resourceStorage,
                RenderPassUtilityProvider utilityProvider,
                PipelineStateManager pipelineStateManager,
                Memory::PoolDescriptorAllocator descriptorAllocator,
                GPUDataInspector gpuDataInspector,
                RenderGraph graph,
                ulong graphNodeIndex
            )
            {
                Pass = renderPass;
            }

            private RenderPass<ContentMediator> Pass;
            private CommandRecorder PassCommandRecorder;
            private ResourceProvider PassResourceProvider;
            private RootConstantsUpdater PassRootConstantsUpdater;
            private RenderPassUtilityProvider UtilityProvider;
            private ulong GraphNodeIndex;

            private bool AreRootSignaturesScheduled = false;
            private bool ArePipelineStatesScheduled = false;
            private bool AreSamplersScheduled = false;

            private RenderContext<ContentMediator> GetContext()
            {
            }
        };*/
    }
}