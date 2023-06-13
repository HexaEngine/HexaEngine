namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;

    public class DrawPass : IRenderPass
    {
        private readonly IGraphicsPipeline graphicsPipeline;

        public uint VertexCount;

        public uint InstanceCount;

        public uint VertexOffset;

        public uint InstanceOffset;
        private bool disposedValue;

        public DrawPass(IGraphicsPipeline graphicsPipeline, uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset)
        {
            this.graphicsPipeline = graphicsPipeline;
            VertexCount = vertexCount;
            InstanceCount = instanceCount;
            VertexOffset = vertexOffset;
            InstanceOffset = instanceOffset;
        }

        public void BeginDraw(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(graphicsPipeline);
        }

        public void Draw(IGraphicsContext context)
        {
            context.DrawInstanced(VertexCount, InstanceCount, VertexOffset, InstanceOffset);
        }

        public void EndDraw(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(null);
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
        // ~DrawPass()
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