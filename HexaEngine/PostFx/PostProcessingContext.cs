namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;

    public class PostProcessingContext : IDisposable
    {
        private Texture2D[] buffers;
        private Format format;
        private int width;
        private int height;
        private int bufferCount;

        private readonly IGraphicsPipelineState copy;

        private int bufferIndex;
        private Texture2D previous;
        private bool disposedValue;
        private Texture2D input;
        private IRenderTargetView output;

        public PostProcessingContext(IGraphicsDevice device, Format format, int width, int height, int bufferCount)
        {
            buffers = new Texture2D[bufferCount];
            for (int i = 0; i < bufferCount; i++)
            {
                buffers[i] = new(device, format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW, filename: "PostFx.Buffer", lineNumber: i);
            }

            this.format = format;
            this.width = width;
            this.height = height;
            this.bufferCount = bufferCount;

            copy = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                PixelShader = "effects/copy/ps.hlsl",
                VertexShader = "quad.hlsl",
            }, GraphicsPipelineStateDesc.DefaultFullscreen);
        }

        public Format Format => format;

        public int Width => width;

        public int Height => height;

        public int BufferCount => bufferCount;

        public IRenderTargetView Output { get => output; set => output = value; }

        public ITexture2D OutputTex { get; set; }

        public Viewport OutputViewport { get; set; }

        public Texture2D Input { get => input; set => input = value; }

        public Texture2D Previous => previous;

        public Texture2D Current => buffers[bufferIndex];

        public bool CanDraw => input != null && output != null;

        public bool IsFirst => previous == input;

        public void Swap()
        {
            previous = Current;
            bufferIndex++;
            if (bufferIndex == bufferCount)
            {
                bufferIndex = 0;
            }
        }

        public void Reset()
        {
            bufferIndex = 0;
            previous = input;
        }

        public void CopyInputToOutput(IGraphicsContext context)
        {
            context.SetRenderTarget(Output, null);
            context.PSSetShaderResource(0, Input.SRV);
            context.SetViewport(OutputViewport);
            context.SetPipelineState(copy);
            context.DrawInstanced(4, 1, 0, 0);
            context.ClearState();
        }

        public void CopyPreviousToCurrent(IGraphicsContext context)
        {
            context.SetRenderTarget(Current, null);
            context.PSSetShaderResource(0, previous.SRV);
            context.SetViewport(OutputViewport);
            context.SetPipelineState(copy);
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
                buffers[i] = new(device, format, width, height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.RW);
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
        /// Finalizes an instance of the <see cref="PostProcessingContext"/> class.
        /// </summary>
        ~PostProcessingContext()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
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