namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;

    public class GraphicsPipeline
    {
        protected IVertexShader? vs;
        protected IHullShader? hs;
        protected IDomainShader? ds;
        protected IGeometryShader? gs;
        protected IPixelShader? ps;
        protected IInputLayout? layout;
        protected IRasterizerState? rasterizerState;
        protected IDepthStencilState? depthStencilState;
        protected IBlendState? blendState;
        protected readonly GraphicsPipelineDesc desc;
        protected readonly ShaderMacro[] macros;
        protected GraphicsPipelineState state = GraphicsPipelineState.Default;
        protected volatile bool initialized;
        protected readonly IGraphicsDevice device;
        protected readonly InputElementDescription[]? inputElements;

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            macros = Array.Empty<ShaderMacro>();
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc, ShaderMacro[] macros)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            this.macros = macros;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            macros = Array.Empty<ShaderMacro>();
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            this.macros = macros;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            this.state = state;
            macros = Array.Empty<ShaderMacro>();
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            this.state = state;
            this.macros = macros;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            this.state = state;
            macros = Array.Empty<ShaderMacro>();
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public GraphicsPipeline(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros)
        {
            Name = new FileInfo(desc.VertexShader ?? throw new ArgumentNullException(nameof(desc))).Directory?.Name ?? throw new ArgumentNullException(nameof(desc));
            this.device = device;
            this.desc = desc;
            this.inputElements = inputElements;
            this.state = state;
            this.macros = macros;
            Compile();
            PipelineManager.Register(this);
            initialized = true;
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc));
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc, ShaderMacro[] macros)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc, macros));
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc, inputElements));
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc, inputElements, macros));
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc, state));
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, ShaderMacro[] macros)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc, state, macros));
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc, state, inputElements));
        }

        public static Task<GraphicsPipeline> CreateAsync(IGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineState state, InputElementDescription[] inputElements, ShaderMacro[] macros)
        {
            return Task.Factory.StartNew(() => new GraphicsPipeline(device, desc, state, inputElements, macros));
        }

        public string Name { get; }

        public GraphicsPipelineDesc Description => desc;

        public GraphicsPipelineState State
        {
            get => state;
            set
            {
                state = value;
                rasterizerState = device.CreateRasterizerState(value.Rasterizer);
                depthStencilState = device.CreateDepthStencilState(value.DepthStencil);
                blendState = device.CreateBlendState(value.Blend);
            }
        }

        public void Recompile()
        {
            initialized = false;

            vs?.Dispose();
            vs = null;
            hs?.Dispose();
            hs = null;
            ds?.Dispose();
            ds = null;
            gs?.Dispose();
            gs = null;
            ps?.Dispose();
            ps = null;
            layout?.Dispose();
            layout = null;
            Compile(true);
            initialized = true;
        }

        private unsafe void Compile(bool bypassCache = false)
        {
            if (desc.VertexShader != null)
            {
                Shader* shader;
                ShaderCache.GetShaderOrCompileFile(device, desc.VertexShaderEntrypoint, desc.VertexShader, "vs_5_0", macros, &shader);
                vs = device.CreateVertexShader(shader);
                vs.DebugName = GetType().Name + nameof(vs);
                if (inputElements == null)
                    layout = device.CreateInputLayout(shader);
                else
                    layout = device.CreateInputLayout(inputElements, shader);
                layout.DebugName = GetType().Name + nameof(layout);
            }

            if (desc.HullShader != null)
            {
                Shader* shader;
                ShaderCache.GetShaderOrCompileFile(device, desc.HullShaderEntrypoint, desc.HullShader, "hs_5_0", macros, &shader);
                hs = device.CreateHullShader(shader);
                hs.DebugName = GetType().Name + nameof(hs);
            }

            if (desc.DomainShader != null)
            {
                Shader* shader;
                ShaderCache.GetShaderOrCompileFile(device, desc.DomainShaderEntrypoint, desc.DomainShader, "ds_5_0", macros, &shader);
                ds = device.CreateDomainShader(shader);
                ds.DebugName = GetType().Name + nameof(hs);
            }

            if (desc.GeometryShader != null)
            {
                Shader* shader;
                ShaderCache.GetShaderOrCompileFile(device, desc.GeometryShaderEntrypoint, desc.GeometryShader, "gs_5_0", macros, &shader);
                gs = device.CreateGeometryShader(shader);
                gs.DebugName = GetType().Name + nameof(gs);
            }

            if (desc.PixelShader != null)
            {
                Shader* shader;
                ShaderCache.GetShaderOrCompileFile(device, desc.PixelShaderEntrypoint, desc.PixelShader, "ps_5_0", macros, &shader);
                ps = device.CreatePixelShader(shader);
                ps.DebugName = GetType().Name + nameof(ps);
            }
        }

        public virtual void BeginDraw(IGraphicsContext context, Viewport viewport)
        {
            if (!initialized) return;

            context.VSSetShader(vs);
            context.HSSetShader(hs);
            context.DSSetShader(ds);
            context.GSSetShader(gs);
            context.PSSetShader(ps);

            context.SetViewport(viewport);
            context.SetRasterizerState(rasterizerState);
            context.SetBlendState(blendState);
            context.SetDepthStencilState(depthStencilState);
            context.SetInputLayout(layout);
            context.SetPrimitiveTopology(state.Topology);
        }

        protected virtual void EndDraw(IGraphicsContext context)
        {
        }

        public void DrawInstanced(IGraphicsContext context, Viewport viewport, uint vertexCount, uint instanceCount, uint vertexOffset, uint instanceOffset)
        {
            if (!initialized) return;
            BeginDraw(context, viewport);
            context.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
            EndDraw(context);
        }

        public void DrawIndexedInstanced(IGraphicsContext context, Viewport viewport, uint indexCount, uint instanceCount, uint indexOffset, int vertexOffset, uint instanceOffset)
        {
            if (!initialized) return;
            BeginDraw(context, viewport);
            context.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
            EndDraw(context);
        }

        public void DrawInstanced(IGraphicsContext context, Viewport viewport, IBuffer args, uint stride)
        {
            if (!initialized) return;
            BeginDraw(context, viewport);
            context.DrawInstancedIndirect(args, stride);
            EndDraw(context);
        }

        public void DrawIndexedInstancedIndirect(IGraphicsContext context, Viewport viewport, IBuffer args, uint stride)
        {
            if (!initialized) return;
            BeginDraw(context, viewport);
            context.DrawIndexedInstancedIndirect(args, stride);
            EndDraw(context);
        }

        public virtual void Dispose()
        {
            PipelineManager.Unregister(this);

            vs?.Dispose();
            hs?.Dispose();
            ds?.Dispose();
            gs?.Dispose();
            ps?.Dispose();
            layout?.Dispose();
            rasterizerState?.Dispose();
            depthStencilState?.Dispose();
            blendState?.Dispose();
        }
    }
}