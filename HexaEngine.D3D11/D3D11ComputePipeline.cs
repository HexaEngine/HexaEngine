namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11ComputePipeline : DisposableBase, IComputePipeline, IPipeline
    {
        private readonly string dbgName;
        private readonly D3D11GraphicsDevice device;
        private ComputePipelineDescEx desc;
        private ShaderMacro[]? macros;

        internal ComPtr<ID3D11ComputeShader> cs;

        internal Shader* computeShaderBlob;

        private bool valid;
        private volatile bool initialized;

        public D3D11ComputePipeline(D3D11GraphicsDevice device, ComputePipelineDescEx desc, string dbgName)
        {
            PipelineManager.Register(this);
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            macros = desc.Macros;
            Compile();
            initialized = true;
        }

        public D3D11ComputePipeline(D3D11GraphicsDevice device, ComputePipelineDescEx desc, ShaderMacro[] macros, string dbgName)
        {
            PipelineManager.Register(this);
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.macros = macros;
            Compile();
            initialized = true;
        }

        public string DebugName => dbgName;

        public ComputePipelineDescEx Desc => desc;

        public bool IsInitialized => initialized;

        public bool IsValid => valid;

        public ShaderMacro[]? Macros { get => macros; set => macros = value; }

        public event Action<IPipeline>? OnCompile;

        public void Recompile()
        {
            initialized = false;

            if (cs.Handle != null)
            {
                cs.Release();
                cs = default;
            }

            if (computeShaderBlob != null)
            {
                Free(computeShaderBlob);
                computeShaderBlob = null;
            }

            Compile(true);

            initialized = true;
        }

        private unsafe void Compile(bool bypassCache = false)
        {
            ShaderMacro[] macros = GetShaderMacros();

            if (desc.Source != null)
            {
                Shader* shader;
                D3D11GraphicsDevice.Compiler.GetShaderOrCompileFile(desc.Entry, desc.Source, "cs_5_0", macros, &shader, bypassCache);
                if (shader == null)
                {
                    valid = false;
                    return;
                }
                ComPtr<ID3D11ComputeShader> computeShader;
                device.Device.CreateComputeShader(shader->Bytecode, shader->Length, (ID3D11ClassLinkage*)null, &computeShader.Handle);
                cs = computeShader;
                Utils.SetDebugName(cs, dbgName);

                computeShaderBlob = shader;
            }

            valid = true;

            OnCompile?.Invoke(this);
        }

        protected virtual ShaderMacro[] GetShaderMacros()
        {
            if (macros == null)
            {
                return [];
            }
            return macros;
        }

        protected override void DisposeCore()
        {
            PipelineManager.Unregister(this);
            if (cs.Handle != null)
            {
                cs.Release();
                cs = default;
            }

            if (computeShaderBlob != null)
            {
                Free(computeShaderBlob);
                computeShaderBlob = null;
            }
        }
    }
}