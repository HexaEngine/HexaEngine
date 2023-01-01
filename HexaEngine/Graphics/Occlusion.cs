namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Occlusion
    {
        private ComputePipeline occlusion;

        public DepthMipChain? DepthMipChain;

        public Occlusion(IGraphicsDevice device)
        {
            occlusion = new(device, new()
            {
                Path = "compute/occlusion/shader.hlsl",
            });
        }

        public void Test()
        {
        }
    }
}