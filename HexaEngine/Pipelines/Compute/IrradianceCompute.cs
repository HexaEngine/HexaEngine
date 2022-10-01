namespace HexaEngine.Pipelines.Compute
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class IrradianceCompute
    {
        private IComputeShader computeShader;

        private readonly List<IBuffer> inputs = new();
        private readonly List<IBuffer> constants = new();
        private readonly List<IShaderResourceView> resources = new();
        private readonly List<ISamplerState> samplers = new();

        public void Compute(IGraphicsContext context, ITexture2D env, ITexture2D irr)
        {
            context.CSSetShader(computeShader);
            context.SetSamplers(samplers.ToArray(), ShaderStage.Compute, 0);
            context.SetShaderResources(resources.ToArray(), ShaderStage.Compute, 0);

            context.Dispatch(irr.Description.Width / 32, irr.Description.Height / 32, 6);
        }
    }

    public class ComputePipeline
    {
        private IComputeShader computeShader;
    }
}