namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Direct3D11;

    public unsafe class ComputePipeline : IComputePipeline
    {
        private bool valid;
        private bool initialized;
        private ID3D11ComputeShader* cs;
        private readonly D3D11GraphicsDevice device;
        private ComputePipelineDesc desc;
        private bool disposedValue;

        public ComputePipeline(D3D11GraphicsDevice device, ComputePipelineDesc desc)
        {
            Name = new FileInfo(desc.Path ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            PipelineManager.Register(this);
            this.device = device;
            this.desc = desc;
            Compile();
            initialized = true;
        }

        public string Name { get; }

        public virtual void BeginDispatch(IGraphicsContext context)
        {
            if (!valid) return;
            if (!initialized) return;
            ((D3D11GraphicsContext)context).DeviceContext->CSSetShader(cs, null, 0);
        }

        public virtual void BeginDispatch(ID3D11DeviceContext1* context)
        {
            if (!valid) return;
            if (!initialized) return;
            context->CSSetShader(cs, null, 0);
        }

        public void Dispatch(IGraphicsContext context, int x, int y, int z)
        {
            if (!valid) return;
            if (!initialized) return;
            BeginDispatch(context);
            context.Dispatch(x, y, z);
            EndDispatch(context);
        }

        public void Dispatch(ID3D11DeviceContext1* context, uint x, uint y, uint z)
        {
            if (!valid) return;
            if (!initialized) return;
            BeginDispatch(context);
            context->Dispatch(x, y, z);
            EndDispatch(context);
        }

        public virtual void EndDispatch(IGraphicsContext context)
        {
            if (!valid) return;
            if (!initialized) return;
            context.ClearState();
        }

        public virtual void EndDispatch(ID3D11DeviceContext1* context)
        {
            if (!valid) return;
            if (!initialized) return;
            context->ClearState();
        }

        public void Recompile()
        {
            initialized = false;

            if (cs != null)
                cs->Release();
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
                ShaderCompiler.GetShaderOrCompileFile(desc.Entry, desc.Path, "cs_5_0", macros, &shader, bypassCache);
                if (shader == null)
                {
                    valid = false;
                    return;
                }
                ID3D11ComputeShader* computeShader;
                device.Device->CreateComputeShader(shader->Bytecode, shader->Length, null, &computeShader);
                cs = computeShader;
                //TODO: cs.DebugName = GetType().Name + nameof(cs);
                Free(shader);
                valid = true;
            }
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
                if (cs != null)
                    cs->Release();
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