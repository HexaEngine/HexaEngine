namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using System;

    public unsafe class D3D11GraphicsPipeline : DisposableBase, IGraphicsPipeline, IPipeline
    {
        private readonly string dbgName;
        private readonly D3D11GraphicsDevice device;
        private readonly GraphicsPipelineDescEx desc;
        private ShaderMacro[]? macros;
        internal ComPtr<ID3D11VertexShader> vs;
        internal ComPtr<ID3D11HullShader> hs;
        internal ComPtr<ID3D11DomainShader> ds;
        internal ComPtr<ID3D11GeometryShader> gs;
        internal ComPtr<ID3D11PixelShader> ps;

        internal Shader* vertexShaderBlob;
        internal Shader* hullShaderBlob;
        internal Shader* domainShaderBlob;
        internal Shader* geometryShaderBlob;
        internal Shader* pixelShaderBlob;

        internal Blob? signature;
        internal InputElementDescription[]? inputElements;
        private bool valid;
        private volatile bool initialized;

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDescEx desc, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            macros = desc.Macros;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public string DebugName => dbgName;

        public GraphicsPipelineDescEx Description => desc;

        public bool IsInitialized => initialized;

        public bool IsValid => valid;

        public ShaderMacro[]? Macros { get => macros; set => macros = value; }

        public event Action<IPipeline>? OnCompile;

        public event Action<IGraphicsPipeline, InputElementDescription[]?, Blob>? OnCreateLayout;

        public void Recompile()
        {
            initialized = false;

            if (vs.Handle != null)
            {
                vs.Release();
                vs = default;
            }

            if (hs.Handle != null)
            {
                hs.Release();
                hs = default;
            }

            if (ds.Handle != null)
            {
                ds.Release();
                ds = default;
            }

            if (gs.Handle != null)
            {
                gs.Release();
                gs = default;
            }

            if (ps.Handle != null)
            {
                ps.Release();
                ps = default;
            }

            if (signature != null)
            {
                signature.Dispose();
                signature = null;
            }

            if (vertexShaderBlob != null)
            {
                Free(vertexShaderBlob);
                vertexShaderBlob = null;
            }

            if (hullShaderBlob != null)
            {
                Free(hullShaderBlob);
                hullShaderBlob = null;
            }

            if (domainShaderBlob != null)
            {
                Free(domainShaderBlob);
                domainShaderBlob = null;
            }

            if (geometryShaderBlob != null)
            {
                Free(geometryShaderBlob);
                geometryShaderBlob = null;
            }

            if (pixelShaderBlob != null)
            {
                Free(pixelShaderBlob);
                pixelShaderBlob = null;
            }

            Compile(true);
            initialized = true;
        }

        protected virtual ShaderMacro[] GetShaderMacros()
        {
            if (macros == null)
            {
                return [];
            }
            return macros;
        }

        private unsafe void Compile(bool bypassCache = false)
        {
            var macros = GetShaderMacros();
            if (desc.VertexShader != null)
            {
                Shader* shader;
                D3D11GraphicsDevice.Compiler.GetShaderOrCompileFileWithInputSignature(desc.VertexShaderEntrypoint, desc.VertexShader, "vs_5_0", macros, &shader, out inputElements, out signature, bypassCache);
                if (shader == null || signature == null)
                {
                    valid = false;
                    return;
                }

                OnCreateLayout?.Invoke(this, inputElements, signature);

                ComPtr<ID3D11VertexShader> vertexShader;
                device.Device.CreateVertexShader(shader->Bytecode, shader->Length, (ID3D11ClassLinkage*)null, &vertexShader.Handle);
                vs = vertexShader;
                Utils.SetDebugName(vs.Handle, $"{dbgName}.{nameof(vs)}");

                vertexShaderBlob = shader;
            }

            if (desc.HullShader != null)
            {
                Shader* shader;
                D3D11GraphicsDevice.Compiler.GetShaderOrCompileFile(desc.HullShaderEntrypoint, desc.HullShader, "hs_5_0", macros, &shader, bypassCache);
                if (shader == null)
                {
                    valid = false;
                    return;
                }

                ComPtr<ID3D11HullShader> hullShader;
                device.Device.CreateHullShader(shader->Bytecode, shader->Length, (ID3D11ClassLinkage*)null, &hullShader.Handle);
                hs = hullShader;
                Utils.SetDebugName(hs.Handle, $"{dbgName}.{nameof(hs)}");

                hullShaderBlob = shader;
            }

            if (desc.DomainShader != null)
            {
                Shader* shader;
                D3D11GraphicsDevice.Compiler.GetShaderOrCompileFile(desc.DomainShaderEntrypoint, desc.DomainShader, "ds_5_0", macros, &shader, bypassCache);
                if (shader == null)
                {
                    valid = false;
                    return;
                }

                ComPtr<ID3D11DomainShader> domainShader;
                device.Device.CreateDomainShader(shader->Bytecode, shader->Length, (ID3D11ClassLinkage*)null, &domainShader.Handle);
                ds = domainShader;
                Utils.SetDebugName(ds.Handle, $"{dbgName}.{nameof(hs)}");

                domainShaderBlob = shader;
            }

            if (desc.GeometryShader != null)
            {
                Shader* shader;
                D3D11GraphicsDevice.Compiler.GetShaderOrCompileFile(desc.GeometryShaderEntrypoint, desc.GeometryShader, "gs_5_0", macros, &shader, bypassCache);
                if (shader == null)
                {
                    valid = false;
                    return;
                }

                ComPtr<ID3D11GeometryShader> geometryShader;
                device.Device.CreateGeometryShader(shader->Bytecode, shader->Length, (ID3D11ClassLinkage*)null, &geometryShader.Handle);
                gs = geometryShader;
                Utils.SetDebugName(gs.Handle, $"{dbgName}.{nameof(gs)}");

                geometryShaderBlob = shader;
            }

            if (desc.PixelShader != null)
            {
                Shader* shader;
                D3D11GraphicsDevice.Compiler.GetShaderOrCompileFile(desc.PixelShaderEntrypoint, desc.PixelShader, "ps_5_0", macros, &shader, bypassCache);
                if (shader == null)
                {
                    valid = false;
                    return;
                }

                ComPtr<ID3D11PixelShader> pixelShader;
                device.Device.CreatePixelShader(shader->Bytecode, shader->Length, (ID3D11ClassLinkage*)null, &pixelShader.Handle);
                ps = pixelShader;
                Utils.SetDebugName(ps.Handle, $"{dbgName}.{nameof(ps)}");

                pixelShaderBlob = shader;
            }

            valid = true;

            OnCompile?.Invoke(this);
        }

        protected override void DisposeCore()
        {
            PipelineManager.Unregister(this);

            if (vs.Handle != null)
            {
                vs.Release();
                vs = default;
            }

            if (hs.Handle != null)
            {
                hs.Release();
                hs = default;
            }

            if (ds.Handle != null)
            {
                ds.Release();
                ds = default;
            }

            if (gs.Handle != null)
            {
                gs.Release();
                gs = default;
            }

            if (ps.Handle != null)
            {
                ps.Release();
                ps = default;
            }

            if (signature != null)
            {
                signature.Dispose();
                signature = null;
            }

            if (vertexShaderBlob != null)
            {
                Free(vertexShaderBlob);
                vertexShaderBlob = null;
            }

            if (hullShaderBlob != null)
            {
                Free(hullShaderBlob);
                hullShaderBlob = null;
            }

            if (domainShaderBlob != null)
            {
                Free(domainShaderBlob);
                domainShaderBlob = null;
            }

            if (geometryShaderBlob != null)
            {
                Free(geometryShaderBlob);
                geometryShaderBlob = null;
            }

            if (pixelShaderBlob != null)
            {
                Free(pixelShaderBlob);
                pixelShaderBlob = null;
            }
        }
    }
}