namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;

    public class DrawIndexedPass : IRenderPass
    {
        private readonly IGraphicsPipeline graphicsPipeline;

        public uint IndexCount;

        public uint InstanceCount;

        public uint IndexOffset;

        public int VertexOffset;

        public uint InstanceOffset;
        private bool disposedValue;

        public DrawIndexedPass(IGraphicsPipeline graphicsPipeline, uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset)
        {
            this.graphicsPipeline = graphicsPipeline;
            IndexCount = indexCount;
            InstanceCount = instanceCount;
            IndexOffset = indexOffset;
            VertexOffset = vertexOffset;
            InstanceOffset = instanceOffset;
        }

        public void BeginDraw(IGraphicsContext context)
        {
            context.SetGraphicsPipeline(graphicsPipeline);
        }

        public void Draw(IGraphicsContext context)
        {
            context.DrawIndexedInstanced(IndexCount, InstanceCount, IndexOffset, VertexOffset, InstanceOffset);
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
        // ~DrawIndexedPass()
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