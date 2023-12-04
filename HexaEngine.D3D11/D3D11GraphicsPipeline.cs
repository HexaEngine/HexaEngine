namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;
    using System;
    using Viewport = Mathematics.Viewport;

    public unsafe class UniformCollection
    {
        private readonly List<ConstantBufferVariable> constantBufferVariables = new();
        private readonly Dictionary<string, ConstantBufferVariable> nameToConstantBufferVariables = new();

        public enum ConstantBufferVariableType
        {
            Unknown,

            Bool,
            Bool2,
            Bool3,
            Bool4,

            Bool1x1,
            Bool1x2,
            Bool1x3,
            Bool1x4,
            Bool2x1,
            Bool2x2,
            Bool2x3,
            Bool2x4,
            Bool3x1,
            Bool3x2,
            Bool3x3,
            Bool3x4,
            Bool4x1,
            Bool4x2,
            Bool4x3,
            Bool4x4,

            Int,
            Int2,
            Int3,
            Int4,

            Int1x1,
            Int1x2,
            Int1x3,
            Int1x4,
            Int2x1,
            Int2x2,
            Int2x3,
            Int2x4,
            Int3x1,
            Int3x2,
            Int3x3,
            Int3x4,
            Int4x1,
            Int4x2,
            Int4x3,
            Int4x4,

            Float,
            Float2,
            Float3,
            Float4,

            Float1x1,
            Float1x2,
            Float1x3,
            Float1x4,
            Float2x1,
            Float2x2,
            Float2x3,
            Float2x4,
            Float3x1,
            Float3x2,
            Float3x3,
            Float3x4,
            Float4x1,
            Float4x2,
            Float4x3,
            Float4x4,

            UInt,
            UInt2,
            UInt3,
            UInt4,

            UInt1x1,
            UInt1x2,
            UInt1x3,
            UInt1x4,
            UInt2x1,
            UInt2x2,
            UInt2x3,
            UInt2x4,
            UInt3x1,
            UInt3x2,
            UInt3x3,
            UInt3x4,
            UInt4x1,
            UInt4x2,
            UInt4x3,
            UInt4x4,

            Double,
            Double2,
            Double3,
            Double4,

            Double1x1,
            Double1x2,
            Double1x3,
            Double1x4,
            Double2x1,
            Double2x2,
            Double2x3,
            Double2x4,
            Double3x1,
            Double3x2,
            Double3x3,
            Double3x4,
            Double4x1,
            Double4x2,
            Double4x3,
            Double4x4,

            Struct,
        }

        public enum MatrixMajor
        {
            Column,
            Row,
        }

        public struct ConstantBufferVariable
        {
            public uint BufferIndex;
            public uint VariableIndex;
            public string Name;
            public ConstantBufferVariableType Type;
            public uint Size;
            public uint Offset;
            public MatrixMajor Major;
        }

        public UniformCollection()
        {
        }

        private static ConstantBufferVariableType Transform(D3DShaderVariableClass variableClass, ConstantBufferVariableType baseVariableType, ConstantBufferVariableType matrixVariableType, uint rows, uint columns)
        {
            ConstantBufferVariableType result;
            if (variableClass is D3DShaderVariableClass.D3DSvcScalar or D3DShaderVariableClass.D3DSvcVector)
            {
                result = (ConstantBufferVariableType)((uint)baseVariableType + (columns - 1));
            }
            else if (variableClass == D3DShaderVariableClass.D3DSvcMatrixRows)
            {
                result = (ConstantBufferVariableType)((uint)matrixVariableType + (rows - 1) * 4);
                result = (ConstantBufferVariableType)((uint)result + (columns - 1));
            }
            else if (variableClass == D3DShaderVariableClass.D3DSvcMatrixColumns)
            {
                result = (ConstantBufferVariableType)((uint)matrixVariableType + (rows - 1) * 4);
                result = (ConstantBufferVariableType)((uint)result + (columns - 1));
            }
            else
            {
                throw new NotSupportedException($"Variable class {variableClass} is not supported!");
            }
            return result;
        }

        public void Append(Shader* pShader, ShaderStage stage)
        {
            ShaderCompiler.Reflect(pShader, out ComPtr<ID3D11ShaderReflection> reflection);

            ShaderDesc desc;
            reflection.GetDesc(&desc);
            for (uint i = 0; i < desc.ConstantBuffers; i++)
            {
                var cb = reflection.GetConstantBufferByIndex(i);
                ShaderBufferDesc bufferDesc;
                cb->GetDesc(&bufferDesc);
                for (uint j = 0; j < bufferDesc.Variables; j++)
                {
                    var v = cb->GetVariableByIndex(j);
                    ShaderVariableDesc varDesc;
                    v->GetDesc(&varDesc);

                    ConstantBufferVariable variable;
                    variable.BufferIndex = i;
                    variable.VariableIndex = j;
                    variable.Name = Utils.ToStr(varDesc.Name);
                    variable.Size = varDesc.Size;
                    variable.Offset = varDesc.StartOffset;
                    var type = v->GetType();
                    ShaderTypeDesc typeDesc;
                    type->GetDesc(&typeDesc);

                    uint rows = typeDesc.Rows;
                    uint columns = typeDesc.Columns;
                    ConstantBufferVariableType variableType = ConstantBufferVariableType.Unknown;

                    switch (typeDesc.Type)
                    {
                        case D3DShaderVariableType.D3DSvtBool:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Bool, ConstantBufferVariableType.Bool1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtInt:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Int, ConstantBufferVariableType.Int1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtFloat:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Float, ConstantBufferVariableType.Float1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtString:
                            break;

                        case D3DShaderVariableType.D3DSvtUint:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.UInt, ConstantBufferVariableType.UInt1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtUint8:
                            break;

                        case D3DShaderVariableType.D3DSvtDouble:
                            variableType = Transform(typeDesc.Class, ConstantBufferVariableType.Double, ConstantBufferVariableType.Double1x1, rows, columns);
                            break;

                        case D3DShaderVariableType.D3DSvtMin8float:
                            break;

                        case D3DShaderVariableType.D3DSvtMin10float:
                            break;

                        case D3DShaderVariableType.D3DSvtMin16float:
                            break;

                        case D3DShaderVariableType.D3DSvtMin12int:
                            break;

                        case D3DShaderVariableType.D3DSvtMin16int:
                            break;

                        case D3DShaderVariableType.D3DSvtMin16Uint:
                            break;

                        case D3DShaderVariableType.D3DSvtInt16:
                            break;

                        case D3DShaderVariableType.D3DSvtUint16:
                            break;

                        case D3DShaderVariableType.D3DSvtFloat16:
                            break;

                        case D3DShaderVariableType.D3DSvtInt64:
                            break;

                        case D3DShaderVariableType.D3DSvtUint64:
                            break;
                    }

                    variable.Type = variableType;
                }
            }

            reflection.Release();
        }
    }

    public unsafe class D3D11GraphicsPipeline : DisposableBase, IGraphicsPipeline
    {
        private readonly string dbgName;
        private bool disposedValue;
        protected readonly D3D11GraphicsDevice device;
        protected InputElementDescription[]? inputElements;
        protected readonly GraphicsPipelineDesc desc;
        protected ShaderMacro[]? macros;
        protected ComPtr<ID3D11VertexShader> vs;
        protected ComPtr<ID3D11HullShader> hs;
        protected ComPtr<ID3D11DomainShader> ds;
        protected ComPtr<ID3D11GeometryShader> gs;
        protected ComPtr<ID3D11PixelShader> ps;
        protected ComPtr<ID3D11InputLayout> layout;
        protected ComPtr<ID3D11RasterizerState2> rasterizerState;
        protected ComPtr<ID3D11DepthStencilState> depthStencilState;
        protected ComPtr<ID3D11BlendState1> blendState;
        protected UniformCollection uniformCollection = new();
        protected GraphicsPipelineState state = GraphicsPipelineState.Default;
        protected bool valid;
        protected volatile bool initialized;

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            State = desc.State;
            macros = desc.Macros;
            inputElements = desc.InputElements;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, ShaderMacro[] macros, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            this.macros = macros;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            this.macros = macros;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            State = state;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            State = state;
            this.macros = macros;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            State = state;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public D3D11GraphicsPipeline(D3D11GraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros, string dbgName = "")
        {
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            State = state;
            this.macros = macros;
            this.dbgName = dbgName;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public string DebugName => dbgName;

        public GraphicsPipelineDesc Description => desc;

        public bool IsInitialized => initialized;

        public bool IsValid => valid;

        public ShaderMacro[]? Macros { get => macros; set => macros = value; }

        public GraphicsPipelineState State
        {
            get => state;
            set
            {
                state = value;
                if (rasterizerState.Handle != null)
                {
                    rasterizerState.Release();
                }

                rasterizerState = null;
                if (depthStencilState.Handle != null)
                {
                    depthStencilState.Release();
                }

                depthStencilState.Handle = null;
                if (blendState.Handle != null)
                {
                    blendState.Release();
                }

                blendState = null;

                ComPtr<ID3D11RasterizerState2> rs;

                var rsDesc = Helper.Convert(value.Rasterizer);
                device.Device.CreateRasterizerState2(&rsDesc, &rs.Handle);
                rasterizerState = rs;
                Utils.SetDebugName(rasterizerState, $"{dbgName}.{nameof(rasterizerState)}");

                ComPtr<ID3D11DepthStencilState> ds;
                var dsDesc = Helper.Convert(value.DepthStencil);
                device.Device.CreateDepthStencilState(&dsDesc, &ds.Handle);
                depthStencilState = ds;
                Utils.SetDebugName(depthStencilState, $"{dbgName}.{nameof(depthStencilState)}");

                ComPtr<ID3D11BlendState1> bs;
                var bsDesc = Helper.Convert(value.Blend);
                device.Device.CreateBlendState1(&bsDesc, &bs.Handle);
                blendState = bs;
                Utils.SetDebugName(blendState, $"{dbgName}.{nameof(blendState)}");
            }
        }

        public void Recompile()
        {
            initialized = false;

            if (vs.Handle != null)
            {
                vs.Release();
            }

            vs = null;
            if (hs.Handle != null)
            {
                hs.Release();
            }

            hs = null;
            if (ds.Handle != null)
            {
                ds.Release();
            }

            ds = null;
            if (gs.Handle != null)
            {
                gs.Release();
            }

            gs = null;
            if (ps.Handle != null)
            {
                ps.Release();
            }

            ps = null;
            layout.Release();
            layout = null;
            Compile(true);
            initialized = true;
        }

        protected virtual ShaderMacro[] GetShaderMacros()
        {
            if (macros == null)
            {
                return Array.Empty<ShaderMacro>();
            }
            return macros;
        }

        private static bool CanSkipLayout(InputElementDescription[]? inputElements)
        {
            ArgumentNullException.ThrowIfNull(inputElements, nameof(inputElements));

            for (int i = 0; i < inputElements.Length; i++)
            {
                var inputElement = inputElements[i];
                if (inputElement.SemanticName is not "SV_VertexID" and not "SV_InstanceID")
                {
                    return false;
                }
            }

            return true;
        }

        private unsafe void Compile(bool bypassCache = false)
        {
            var macros = GetShaderMacros();
            if (desc.VertexShader != null)
            {
                Shader* shader;
                D3D11GraphicsDevice.Compiler.GetShaderOrCompileFileWithInputSignature(desc.VertexShaderEntrypoint, desc.VertexShader, "vs_5_0", macros, &shader, out var elements, out var signature, bypassCache);
                if (shader == null || signature == null || inputElements == null && elements == null)
                {
                    valid = false;
                    return;
                }

                ComPtr<ID3D11VertexShader> vertexShader;
                device.Device.CreateVertexShader(shader->Bytecode, shader->Length, (ID3D11ClassLinkage*)null, &vertexShader.Handle);
                vs = vertexShader;
                uniformCollection.Append(shader, ShaderStage.Vertex);
                Utils.SetDebugName(vs, $"{dbgName}.{nameof(vs)}");

                inputElements ??= elements;

                if (!CanSkipLayout(inputElements))
                {
                    ComPtr<ID3D11InputLayout> il;
                    InputElementDesc* descs = AllocT<InputElementDesc>(inputElements.Length);
                    Helper.Convert(inputElements, descs);
                    device.Device.CreateInputLayout(descs, (uint)inputElements.Length, (void*)signature.BufferPointer, signature.PointerSize, &il.Handle);
                    Helper.Free(descs, inputElements.Length);
                    Free(descs);
                    layout = il;

                    Utils.SetDebugName(layout, $"{dbgName}.{nameof(layout)}");
                }
                else
                {
                    layout = default;
                }

                Free(shader);
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
                uniformCollection.Append(shader, ShaderStage.Hull);
                Utils.SetDebugName(hs, $"{dbgName}.{nameof(hs)}");

                Free(shader);
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
                uniformCollection.Append(shader, ShaderStage.Domain);
                Utils.SetDebugName(ds, $"{dbgName}.{nameof(hs)}");

                Free(shader);
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
                uniformCollection.Append(shader, ShaderStage.Geometry);
                Utils.SetDebugName(gs, $"{dbgName}.{nameof(gs)}");

                Free(shader);
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
                uniformCollection.Append(shader, ShaderStage.Pixel);
                Utils.SetDebugName(ps, $"{dbgName}.{nameof(ps)}");

                Free(shader);
            }

            valid = true;
        }

        public void BeginDraw(IGraphicsContext context)
        {
            if (context is not D3D11GraphicsContext contextd3d11)
            {
                return;
            }

            if (!initialized)
            {
                return;
            }

            if (!valid)
            {
                return;
            }

            ComPtr<ID3D11DeviceContext3> ctx = contextd3d11.DeviceContext;
            ctx.VSSetShader(vs, null, 0);
            ctx.HSSetShader(hs, null, 0);
            ctx.DSSetShader(ds, null, 0);
            ctx.GSSetShader(gs, null, 0);
            ctx.PSSetShader(ps, null, 0);

            ctx.RSSetState(rasterizerState);

            var factor = State.BlendFactor;
            float* fac = (float*)&factor;

            ctx.OMSetBlendState(blendState, fac, state.SampleMask);
            ctx.OMSetDepthStencilState(depthStencilState, state.StencilRef);
            ctx.IASetInputLayout(layout);
            ctx.IASetPrimitiveTopology(Helper.Convert(state.Topology));
        }

        public void SetGraphicsPipeline(ComPtr<ID3D11DeviceContext3> context, Viewport viewport)
        {
            if (!initialized)
            {
                return;
            }

            if (!valid)
            {
                return;
            }

            context.VSSetShader(vs, null, 0);
            context.HSSetShader(hs, null, 0);
            context.DSSetShader(ds, null, 0);
            context.GSSetShader(gs, null, 0);
            context.PSSetShader(ps, null, 0);

            var dViewport = Helper.Convert(viewport);
            context.RSSetViewports(1, &dViewport);
            context.RSSetState(rasterizerState);

            var factor = State.BlendFactor;
            float* fac = (float*)&factor;

            context.OMSetBlendState(blendState, fac, uint.MaxValue);
            context.OMSetDepthStencilState(depthStencilState, state.StencilRef);
            context.IASetInputLayout(layout);
            context.IASetPrimitiveTopology(Helper.Convert(state.Topology));
        }

        public void SetGraphicsPipeline(ComPtr<ID3D11DeviceContext3> context)
        {
            if (!initialized)
            {
                return;
            }

            if (!valid)
            {
                return;
            }

            context.VSSetShader(vs, null, 0);
            context.HSSetShader(hs, null, 0);
            context.DSSetShader(ds, null, 0);
            context.GSSetShader(gs, null, 0);
            context.PSSetShader(ps, null, 0);

            context.RSSetState(rasterizerState);

            var factor = State.BlendFactor;
            float* fac = (float*)&factor;

            context.OMSetBlendState(blendState, fac, uint.MaxValue);
            context.OMSetDepthStencilState(depthStencilState, state.StencilRef);
            context.IASetInputLayout(layout);
            context.IASetPrimitiveTopology(Helper.Convert(state.Topology));
        }

        public void EndDraw(IGraphicsContext context)
        {
            if (context is not D3D11GraphicsContext contextd3d11)
            {
                return;
            }

            EndDraw(contextd3d11.DeviceContext);
        }

        public static void EndDraw(ComPtr<ID3D11DeviceContext3> context)
        {
            context.VSSetShader((ID3D11VertexShader*)null, null, 0);
            context.HSSetShader((ID3D11HullShader*)null, null, 0);
            context.DSSetShader((ID3D11DomainShader*)null, null, 0);
            context.GSSetShader((ID3D11GeometryShader*)null, null, 0);
            context.PSSetShader((ID3D11PixelShader*)null, null, 0);

            context.RSSetState((ID3D11RasterizerState*)null);

            context.OMSetBlendState((ID3D11BlendState*)null, (float*)null, uint.MaxValue);
            context.OMSetDepthStencilState((ID3D11DepthStencilState*)null, 0);
            context.IASetInputLayout((ID3D11InputLayout*)null);
            context.IASetPrimitiveTopology(0);
        }

        protected override void DisposeCore()
        {
            PipelineManager.Unregister(this);

            if (vs.Handle != null)
            {
                vs.Release();
            }

            if (hs.Handle != null)
            {
                hs.Release();
            }

            if (ds.Handle != null)
            {
                ds.Release();
            }

            if (gs.Handle != null)
            {
                gs.Release();
            }

            if (ps.Handle != null)
            {
                ps.Release();
            }

            if (layout.Handle != null)
            {
                layout.Release();
            }

            if (rasterizerState.Handle != null)
            {
                rasterizerState.Release();
            }

            if (rasterizerState.Handle != null)
            {
                depthStencilState.Release();
            }

            if (rasterizerState.Handle != null)
            {
                blendState.Release();
            }
        }
    }
}