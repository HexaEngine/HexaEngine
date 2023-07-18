namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class EffectGraphicsPass : EffectPass
    {
        public EffectGraphicsPass()
        {
            RenderTargets.Add(new("RTV1", 0));
        }

        public GraphicsPipelineDesc PipelineDesc { get; set; } = new();

        public EffectGraphicsPipelineState PipelineState { get; set; } = new();

        public List<EffectRenderTarget> RenderTargets { get; set; } = new();
    }
}