namespace HexaEngine.UI.Text
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;

    public class GlyphRun : IDisposable
    {
        private readonly FontAtlas font;
        private readonly VertexBuffer<TextVertex> vertexBuffer;
        private readonly IndexBuffer<uint> indexBuffer;
        private string? text;
        private bool disposedValue;

        public GlyphRun(IGraphicsDevice device, FontAtlas font)
        {
            this.font = font;
            vertexBuffer = new(device, CpuAccessFlags.Write);
            indexBuffer = new(device, CpuAccessFlags.Write);
        }

        public VertexBuffer<TextVertex> VertexBuffer => vertexBuffer;

        public IndexBuffer<uint> IndexBuffer => indexBuffer;

        public FontAtlas Font => font;

        public string? Text
        {
            get => text;
            set
            {
#if DEBUG
                Logger.Assert(!disposedValue, "GlyphRun is disposed");
#endif
                vertexBuffer.ResetCounter();
                indexBuffer.ResetCounter();
                font.CreateRun(text, vertexBuffer, indexBuffer);
                text = value;
            }
        }

        public void Draw(IGraphicsContext context)
        {
#if DEBUG
            Logger.Assert(!disposedValue, "GlyphRun is disposed");
#endif
            vertexBuffer.Bind(context);
            indexBuffer.Bind(context);
            context.DrawIndexedInstanced(indexBuffer.Count, 1, 0, 0, 0);
            indexBuffer.Unbind(context);
            vertexBuffer.Unbind(context);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                vertexBuffer.Dispose();
                indexBuffer.Dispose();
                disposedValue = true;
            }
        }

        ~GlyphRun()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}