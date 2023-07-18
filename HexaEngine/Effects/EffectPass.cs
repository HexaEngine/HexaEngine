namespace HexaEngine.Effects
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class EffectPass
    {
        public EffectPass()
        {
            ConstantBuffers.Add(new("TestCB", ShaderStage.Vertex, 0));
            ConstantBuffers.Add(new("TestCB", ShaderStage.Pixel, 1));
            ShaderResources.Add(new("TestSRV", ShaderStage.Pixel, 0));
            SamplerStates.Add(new("TestSampler", ShaderStage.Pixel, 0));
        }

        public List<EffectBinding> ConstantBuffers { get; set; } = new();

        public List<EffectBinding> ShaderResources { get; set; } = new();

        public List<EffectBinding> SamplerStates { get; set; } = new();
    }
}