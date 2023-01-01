namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;

    public class ComputePipeline : IDisposable
    {
        private bool initialized;
        private IComputeShader? cs;
        private readonly IGraphicsDevice device;
        private ComputePipelineDesc desc;
        private bool disposedValue;

        public ComputePipeline(IGraphicsDevice device, ComputePipelineDesc desc)
        {
            Name = new FileInfo(desc.Path ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            PipelineManager.Register(this);
            this.device = device;
            this.desc = desc;
            Compile();
            initialized = true;
        }

        public static Task<ComputePipeline> CreateAsync(IGraphicsDevice device, ComputePipelineDesc desc)
        {
            return Task.Factory.StartNew(() => new ComputePipeline(device, desc));
        }

        public string Name { get; }

        public virtual void BeginDispatch(IGraphicsContext context)
        {
            if (!initialized) return;
            context.CSSetShader(cs);
        }

        public void Dispatch(IGraphicsContext context, int x, int y, int z)
        {
            if (!initialized) return;
            BeginDispatch(context);
            context.Dispatch(x, y, z);
            EndDispatch(context);
        }

        public virtual void EndDispatch(IGraphicsContext context)
        {
            if (!initialized) return;
            context.ClearState();
        }

        public void Recompile()
        {
            initialized = false;

            cs?.Dispose();
            cs = null;

            Compile(true);

            initialized = true;
        }

        private unsafe void Compile(bool bypassCache = false)
        {
            ShaderMacro[] macros = GetShaderMacros();

            if (desc.Path != null)
            {
                Shader* shader;
                ShaderCache.GetShaderOrCompileFile(device, desc.Entry, desc.Path, "cs_5_0", macros, &shader, bypassCache);
                cs = device.CreateComputeShader(shader);
                cs.DebugName = GetType().Name + nameof(cs);
            }
        }

        public static IBuffer CreateRawBuffer<T>(IGraphicsDevice device, T[] values) where T : struct
        {
            return device.CreateBuffer(values, BindFlags.UnorderedAccess | BindFlags.ShaderResource | BindFlags.IndexBuffer | BindFlags.VertexBuffer, miscFlags: ResourceMiscFlag.BufferAllowRawViews);
        }

        public static IBuffer CreateRawBuffer(IGraphicsDevice device, int size)
        {
            BufferDescription description = new()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource | BindFlags.IndexBuffer | BindFlags.VertexBuffer,
                ByteWidth = size,
                MiscFlags = ResourceMiscFlag.BufferAllowRawViews
            };
            return device.CreateBuffer(description);
        }

        protected virtual ShaderMacro[] GetShaderMacros()
        {
            return Array.Empty<ShaderMacro>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                PipelineManager.Unregister(this);
                cs?.Dispose();
                disposedValue = true;
            }
        }

        ~ComputePipeline()
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