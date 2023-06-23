namespace HexaEngine.Core.Rendering
{
    using HexaEngine.Core.Graphics;
    using System;

    public class ComputeSetUnorderedAccessViews : IRenderPass
    {
        private bool disposedValue;

        public void BeginDraw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void EndDraw(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}