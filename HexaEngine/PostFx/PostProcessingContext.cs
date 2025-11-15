namespace HexaEngine.PostFx
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;

    public class PostProcessingContext : IDisposable
    {
        private Texture2D[] buffers = null!;
        private Format format;
        private int width;
        private int height;
        private int bufferCount;

        private readonly IGraphicsPipelineState copy = null!;

        private int bufferIndex;
        private Texture2D previous = null!;
        private bool disposedValue;
        private Texture2D input = null!;
        private IRenderTargetView output = null!;
        private bool isfirst;

        public PostProcessingContext(IGraphicsDevice device, Format format, int width, int height, int bufferCount)
        {
            buffers = new Texture2D[bufferCount];
            for (int i = 0; i < bufferCount; i++)
            {
                buffers[i] = new(format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW, filename: "PostFx.Buffer", lineNumber: i);
            }

            this.format = format;
            this.width = width;
            this.height = height;
            this.bufferCount = bufferCount;

            copy = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                PixelShader = "HexaEngine.Core:shaders/effects/copy/ps.hlsl",
                VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        public Format Format => format;

        public int Width => width;

        public int Height => height;

        public int BufferCount => bufferCount;

        public IRenderTargetView Output { get => output; set => output = value; }

        public ITexture2D OutputTex { get; set; } = null!;

        public Viewport OutputViewport { get; set; }

        public Texture2D Input { get => input; set => input = value; }

        public Texture2D Previous => previous;

        public Texture2D Current => buffers[bufferIndex];

        public bool CanDraw => input != null && output != null;

        public bool IsFirst => isfirst;

        public void Swap()
        {
            previous = Current;
            bufferIndex++;
            if (bufferIndex == bufferCount)
            {
                bufferIndex = 0;
            }
        }

        public void Signal()
        {
            isfirst = false;
        }

        public void Reset()
        {
            isfirst = true;
            bufferIndex = 0;
            previous = input;
        }

        public void CopyInputToOutput(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            copy.Bindings.SetSRV("sourceTex", Input);
            context.SetViewport(OutputViewport);
            context.SetGraphicsPipelineState(copy);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        public void CopyPreviousToCurrent(IGraphicsContext context)
        {
            context.SetRenderTarget(Current, null);
            copy.Bindings.SetSRV("sourceTex", previous);
            context.SetViewport(OutputViewport);
            context.SetGraphicsPipelineState(copy);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        public void Resize(IGraphicsDevice device, int width, int height)
        {
            Resize(device, format, width, height, bufferCount);
        }

        public void Resize(IGraphicsDevice device, int width, int height, int bufferCount)
        {
            Resize(device, format, width, height, bufferCount);
        }

        public void Resize(IGraphicsDevice device, Format format, int width, int height, int bufferCount)
        {
            for (int i = 0; i < this.bufferCount; i++)
            {
                buffers[i].Dispose();
            }

            buffers = new Texture2D[bufferCount];
            for (int i = 0; i < bufferCount; i++)
            {
                buffers[i] = new(format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
            }

            this.format = format;
            this.width = width;
            this.height = height;
            this.bufferCount = bufferCount;
        }

        public void Clear(IGraphicsContext context)
        {
            for (int i = 0; i < bufferCount; i++)
            {
                context.ClearRenderTargetView(buffers[i], default);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                copy.Dispose();
                for (int i = 0; i < bufferCount; i++)
                {
                    buffers[i].Dispose();
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}