namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public class DispatchPass : IRenderPass
    {
        private readonly IComputePipeline computeShader;

        // The number of groups to dispatch for the compute shader kernel.
        private UPoint3 numGroups;

        private bool disposedValue;

        public DispatchPass(IComputePipeline computeShader, UPoint3 numGroups)
        {
            this.computeShader = computeShader;
            this.numGroups = numGroups;
        }

        public UPoint3 NumGroups { get => numGroups; set => numGroups = value; }

        public void BeginDraw(IGraphicsContext context)
        {
            context.SetComputePipeline(computeShader);
        }

        public void Draw(IGraphicsContext context)
        {
            context.Dispatch(numGroups.X, numGroups.Y, numGroups.Z);
        }

        public void EndDraw(IGraphicsContext context)
        {
            context.SetComputePipeline(null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DispatchPass()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}