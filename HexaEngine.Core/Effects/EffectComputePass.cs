namespace HexaEngine.Core.Effects
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class EffectComputePass : EffectPass
    {
        public EffectComputePass()
        {
            UnorderedAccessViews.Add(new("UAV1", 0));
        }

        public ComputePipelineDesc PipelineDesc { get; set; } = new();

        public List<EffectUnorderedAccessView> UnorderedAccessViews { get; set; } = new();
    }
}