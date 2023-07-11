namespace HexaEngine.Rendering.Graph
{
    public class RenderPass<ContentMediator>
    {
        private RenderPassMetadata metadata;

        public RenderPass(Name name, RenderPassPurpose purpose = RenderPassPurpose.Default)
        {
            metadata = new(name, purpose);
        }

        public RenderPassMetadata Metadata => metadata;

        public virtual void SetupRootSignatures(RootSignatureCreator rootSignatureCreator)
        {
        }

        public virtual void SetupPipelineStates(PipelineStateCreator stateCreator)
        {
        }

        public virtual void ScheduleResources(ResourceScheduler<ContentMediator> scheduler)
        {
        }

        public virtual void ScheduleSubPasses(SubPassScheduler<ContentMediator> scheduler)
        {
        }

        public virtual void ScheduleSamplers(SamplerCreator samplerCreator)
        {
        }

        public virtual void Render(RenderContext<ContentMediator> context)
        {
        }
    }
}