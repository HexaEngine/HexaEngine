namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;

    public class GraphicsPipeline
    {
        private IVertexShader? vs;
        private IHullShader? hs;
        private IDomainShader? ds;
        private IGeometryShader? gs;
        private IPixelShader? ps;
        private IInputLayout? layout;
        private IRasterizerState? rasterizerState;
        private IDepthStencilState? depthStencilState;
        private IBlendState? blendState;
        private readonly GraphicsPipelineDesc desc;
        private readonly ShaderMacro[] macros;
        private GraphicsPipelineState state = GraphicsPipelineState.Default;
        private volatile bool initialized;
        private readonly IGraphicsDevice device;
        private readonly InputElementDescription[]? inputElements;

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

        private void Compile(bool bypassCache = false)
        {
            if (desc.VertexShader != null)
                if (ShaderCache.GetShader(desc.VertexShader, macros, out var data) && !bypassCache)
                {
                    vs = device.CreateVertexShader(data);
                    vs.DebugName = GetType().Name + nameof(vs);
                    if (inputElements == null)
                        layout = device.CreateInputLayout(data);
                    else
                        layout = device.CreateInputLayout(inputElements, data.ToArray());
                    layout.DebugName = GetType().Name + nameof(layout);
                }
                else
                {
                    device.CompileFromFile(desc.VertexShader, macros, desc.VertexShaderEntrypoint, "vs_5_0", out var vBlob);

                    if (vBlob == null)
                    {
                        return;
                    }

                    ShaderCache.CacheShader(desc.VertexShader, macros, vBlob);
                    vs = device.CreateVertexShader(vBlob.AsBytes());
                    vs.DebugName = GetType().Name + nameof(vs);
                    if (inputElements == null)
                        layout = device.CreateInputLayout(vBlob);
                    else
                        layout = device.CreateInputLayout(inputElements, vBlob);
                    layout.DebugName = GetType().Name + nameof(layout);
                    vBlob.Dispose();
                }
            if (desc.HullShader != null)
                if (ShaderCache.GetShader(desc.HullShader, macros, out var data) && !bypassCache)
                {
                    hs = device.CreateHullShader(data);
                    hs.DebugName = GetType().Name + nameof(hs);
                }
                else
                {
                    device.CompileFromFile(desc.HullShader, macros, desc.HullShaderEntrypoint, "hs_5_0", out var pBlob);

                    if (pBlob == null)
                    {
                        return;
                    }

                    ShaderCache.CacheShader(desc.HullShader, macros, pBlob);
                    hs = device.CreateHullShader(pBlob);
                    hs.DebugName = GetType().Name + nameof(hs);
                    pBlob.Dispose();
                }
            if (desc.DomainShader != null)
                if (ShaderCache.GetShader(desc.DomainShader, macros, out var data) && !bypassCache)
                {
                    ds = device.CreateDomainShader(data);
                    ds.DebugName = GetType().Name + nameof(ds);
                }
                else
                {
                    device.CompileFromFile(desc.DomainShader, macros, desc.DomainShaderEntrypoint, "ds_5_0", out var pBlob);

                    if (pBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.DomainShader, macros, pBlob);
                    ds = device.CreateDomainShader(pBlob);
                    ds.DebugName = GetType().Name + nameof(ds);
                    pBlob.Dispose();
                }
            if (desc.GeometryShader != null)
                if (ShaderCache.GetShader(desc.GeometryShader, macros, out var data) && !bypassCache)
                {
                    gs = device.CreateGeometryShader(data);
                    gs.DebugName = GetType().Name + nameof(gs);
                }
                else
                {
                    device.CompileFromFile(desc.GeometryShader, macros, desc.GeometryShaderEntrypoint, "gs_5_0", out var pBlob);

                    if (pBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.GeometryShader, macros, pBlob);
                    gs = device.CreateGeometryShader(pBlob);
                    gs.DebugName = GetType().Name + nameof(gs);
                    pBlob.Dispose();
                }
            if (desc.PixelShader != null)
                if (ShaderCache.GetShader(desc.PixelShader, macros, out var data) && !bypassCache)
                {
                    ps = device.CreatePixelShader(data);
                    ps.DebugName = GetType().Name + nameof(ps);
                }
                else
                {
                    device.CompileFromFile(desc.PixelShader, macros, desc.PixelShaderEntrypoint, "ps_5_0", out var pBlob);

                    if (pBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.PixelShader, macros, pBlob);
                    ps = device.CreatePixelShader(pBlob);
                    ps.DebugName = GetType().Name + nameof(ps);
                    pBlob.Dispose();
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

        public void DrawInstanced(IGraphicsContext context, Viewport viewport, int vertexCount, int instanceCount, int vertexOffset, int instanceOffset)
        {
            if (!initialized) return;
            BeginDraw(context, viewport);
            context.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
            EndDraw(context);
        }

        public void DrawIndexedInstanced(IGraphicsContext context, Viewport viewport, int indexCount, int instanceCount, int indexOffset, int vertexOffset, int instanceOffset)
        {
            if (!initialized) return;
            BeginDraw(context, viewport);
            context.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
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