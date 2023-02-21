namespace HexaEngine.Core.Effects
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public struct EffectDescription
    {
        public EffectDescription()
        {
        }

        public GraphicsPipelineDesc GraphicsPipelineDesc { get; set; } = new();

        public GraphicsPipelineState GraphicsPipelineState { get; set; } = GraphicsPipelineState.Default;

        public List<EffectConstantBuffer> EffectConstants { get; set; } = new();

        public List<EffectResourceDescription> EffectResources { get; set; } = new();

        public List<EffectSamplerDescription> EffectSamplers { get; set; } = new();

        public List<EffectTargetDescription> EffectTargets { get; set; } = new();
    }
}