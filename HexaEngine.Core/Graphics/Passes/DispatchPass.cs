namespace HexaEngine.Core.Graphics.Passes
{
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DispatchPass : IRenderPass
    {
        private IComputePipeline computeShader;

        // The number of groups to dispatch for the compute shader kernel.
        private UPoint3 numGroups;

        public DispatchPass(IComputePipeline computeShader, UPoint3 numGroups)
        {
            this.computeShader = computeShader;
            this.numGroups = numGroups;
        }

        public UPoint3 NumGroups { get => numGroups; set => numGroups = value; }

        public virtual void PreRender(IGraphicsContext context)
        {
            context.SetComputePipeline(computeShader);
        }

        public virtual void Render(IGraphicsContext context)
        {
            context.Dispatch(numGroups.X, numGroups.Y, numGroups.Z);
        }

        public virtual void PostRender(IGraphicsContext context)
        {
        }
    }
}