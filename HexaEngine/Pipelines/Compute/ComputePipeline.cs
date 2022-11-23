namespace HexaEngine.Pipelines.Compute
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;

    public class ComputePipeline : IDisposable
    {
        private IComputeShader? cs;
        private readonly IGraphicsDevice device;
        private ComputePipelineDesc desc;
        private bool disposedValue;

        public ComputePipeline(IGraphicsDevice device, ComputePipelineDesc desc)
        {
            this.device = device;
            this.desc = desc;
            Compile();
            Reload += OnReload;
        }

        public virtual void BeginDispatch(IGraphicsContext context)
        {
            context.CSSetShader(cs);
        }

        public void Dispatch(IGraphicsContext context, int x, int y, int z)
        {
            BeginDispatch(context);
            context.Dispatch(x, y, z);
            EndDispatch(context);
        }

        public virtual void EndDispatch(IGraphicsContext context)
        {
            context.ClearState();
        }

        #region Hotreload

        public static event EventHandler? Reload;

        public static void ReloadShaders()
        {
            ImGuiConsole.Log(LogSeverity.Info, "recompiling compute shaders ...");
            Reload?.Invoke(null, EventArgs.Empty);
            ImGuiConsole.Log(LogSeverity.Info, "recompiling compute shaders ... done!");
        }

        protected virtual void OnReload(object? sender, EventArgs args)
        {
            cs?.Dispose();
            Compile();
        }

        #endregion Hotreload

        private void Compile()
        {
            ShaderMacro[] macros = GetShaderMacros();
            if (desc.Path != null)
                if (ShaderCache.GetShader(desc.Path, macros, out var data))
                {
                    cs = device.CreateComputeShader(data);
                    cs.DebugName = GetType().Name + nameof(cs);
                }
                else
                {
                    device.CompileFromFile(desc.Path, macros, desc.Entry, "cs_5_0", out var vBlob);

                    if (vBlob == null)
                    {
                        return;
                    }

                    ShaderCache.CacheShader(desc.Path, macros, vBlob);
                    cs = device.CreateComputeShader(vBlob.AsBytes());
                    cs.DebugName = GetType().Name + nameof(cs);
                    vBlob.Dispose();
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
                Reload -= OnReload;
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