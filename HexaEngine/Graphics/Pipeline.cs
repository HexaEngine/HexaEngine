namespace HexaEngine.Graphics
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class Pipeline
    {
        private IVertexShader vs;
        private IHullShader hs;
        private IDomainShader ds;
        private IGeometryShader gs;
        private IPixelShader ps;
        private IInputLayout layout;
        private IRasterizerState rasterizerState;
        private IDepthStencilState depthStencilState;
        private IBlendState blendState;
        private readonly PipelineDesc desc;
        private PipelineState state = PipelineState.Default;
        private readonly IGraphicsDevice device;
        private readonly List<BoundConstant> constants = new();
        private readonly List<BoundResource> resources = new();
        private readonly List<BoundSampler> samplers = new();

        public Pipeline(IGraphicsDevice device, PipelineDesc desc)
        {
            this.device = device;
            this.desc = desc;
            Compile();
            Reload += OnReload;
        }

        public List<BoundConstant> Constants => constants;

        public List<BoundResource> Resources => resources;

        public List<BoundSampler> Samplers => samplers;

        public PipelineDesc Description => desc;

        public PipelineState State
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

        #region Hotreload

        public static event EventHandler Reload;

        public static void ReloadShaders()
        {
            ImGuiConsole.Log(ConsoleMessageType.Info, "recompiling shaders ...");
            Reload?.Invoke(null, null);
            ImGuiConsole.Log(ConsoleMessageType.Info, "recompiling shaders ... done!");
        }

        protected virtual void OnReload(object sender, EventArgs args)
        {
            vs?.Dispose();
            hs?.Dispose();
            ds?.Dispose();
            gs?.Dispose();
            ps?.Dispose();
            layout?.Dispose();
            Compile();
        }

        #endregion Hotreload

        private void Compile()
        {
            if (desc.VertexShader != null)
                if (ShaderCache.GetShader(desc.VertexShader, out var data))
                {
                    vs = device.CreateVertexShader(data);
                    vs.DebugName = GetType().Name + nameof(vs);
                    layout = device.CreateInputLayout(data);
                    layout.DebugName = GetType().Name + nameof(layout);
                }
                else
                {
                    device.CompileFromFile(desc.VertexShader, desc.VertexShaderEntrypoint, "vs_5_0", out var vBlob);
                    if (vBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.VertexShader, vBlob);
                    vs = device.CreateVertexShader(vBlob.AsBytes());
                    vs.DebugName = GetType().Name + nameof(vs);
                    layout = device.CreateInputLayout(vBlob);
                    layout.DebugName = GetType().Name + nameof(layout);
                    vBlob.Dispose();
                }
            if (desc.HullShader != null)
                if (ShaderCache.GetShader(desc.HullShader, out var data))
                {
                    hs = device.CreateHullShader(data);
                    hs.DebugName = GetType().Name + nameof(hs);
                }
                else
                {
                    device.CompileFromFile(desc.HullShader, desc.HullShaderEntrypoint, "hs_5_0", out var pBlob);
                    if (pBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.HullShader, pBlob);
                    hs = device.CreateHullShader(pBlob);
                    hs.DebugName = GetType().Name + nameof(hs);
                    pBlob.Dispose();
                }
            if (desc.DomainShader != null)
                if (ShaderCache.GetShader(desc.DomainShader, out var data))
                {
                    ds = device.CreateDomainShader(data);
                    ds.DebugName = GetType().Name + nameof(ds);
                }
                else
                {
                    device.CompileFromFile(desc.DomainShader, desc.DomainShaderEntrypoint, "ds_5_0", out var pBlob);
                    if (pBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.DomainShader, pBlob);
                    ds = device.CreateDomainShader(pBlob);
                    ds.DebugName = GetType().Name + nameof(ds);
                    pBlob.Dispose();
                }
            if (desc.GeometryShader != null)
                if (ShaderCache.GetShader(desc.GeometryShader, out var data))
                {
                    gs = device.CreateGeometryShader(data);
                    gs.DebugName = GetType().Name + nameof(gs);
                }
                else
                {
                    device.CompileFromFile(desc.GeometryShader, desc.GeometryShaderEntrypoint, "gs_5_0", out var pBlob);
                    if (pBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.GeometryShader, pBlob);
                    gs = device.CreateGeometryShader(pBlob);
                    gs.DebugName = GetType().Name + nameof(gs);
                    pBlob.Dispose();
                }
            if (desc.PixelShader != null)
                if (ShaderCache.GetShader(desc.PixelShader, out var data))
                {
                    ps = device.CreatePixelShader(data);
                    ps.DebugName = GetType().Name + nameof(ps);
                }
                else
                {
                    device.CompileFromFile(desc.PixelShader, desc.PixelShaderEntrypoint, "ps_5_0", out var pBlob);
                    if (pBlob == null)
                    {
                        return;
                    }
                    ShaderCache.CacheShader(desc.PixelShader, pBlob);
                    ps = device.CreatePixelShader(pBlob);
                    ps.DebugName = GetType().Name + nameof(ps);
                    pBlob.Dispose();
                }
        }

        protected virtual void BeginDraw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform)
        {
            for (int i = 0; i < constants.Count; i++)
            {
                constants[i].Bind(context);
            }
            for (int i = 0; i < resources.Count; i++)
            {
                resources[i].Bind(context);
            }
            for (int i = 0; i < samplers.Count; i++)
            {
                samplers[i].Bind(context);
            }

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
            context.ClearState();
        }

        public void Draw(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform, int vertexCount, int offset)
        {
            BeginDraw(context, viewport, view, transform);
            context.Draw(vertexCount, offset);
            EndDraw(context);
        }

        public void DrawIndexed(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform, int indexCount, int indexOffset, int vertexOffset)
        {
            BeginDraw(context, viewport, view, transform);
            context.DrawIndexed(indexCount, indexOffset, vertexOffset);
            EndDraw(context);
        }

        public void DrawInstanced(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform, int vertexCount, int instanceCount, int vertexOffset, int instanceOffset)
        {
            BeginDraw(context, viewport, view, transform);
            context.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
            EndDraw(context);
        }

        public void DrawIndexedInstanced(IGraphicsContext context, Viewport viewport, IView view, Matrix4x4 transform, int indexCount, int instanceCount, int indexOffset, int vertexOffset, int instanceOffset)
        {
            BeginDraw(context, viewport, view, transform);
            context.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
            EndDraw(context);
        }

        protected unsafe IBuffer CreateConstantBuffer<T>(params ShaderBinding[] bindings) where T : unmanaged
        {
            var constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(T), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constants.Add(new(constantBuffer, bindings));
            return constantBuffer;
        }

        protected unsafe IBuffer CreateConstantBuffer<T>(int count, params ShaderBinding[] bindings) where T : unmanaged
        {
            var constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(T) * count, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constants.Add(new(constantBuffer, bindings));
            return constantBuffer;
        }

        protected unsafe IBuffer CreateConstantBuffer<T>(ShaderStage stage, int index) where T : unmanaged
        {
            var constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(T), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constants.Add(new(constantBuffer, new ShaderBinding(stage, index)));
            return constantBuffer;
        }

        protected unsafe IBuffer CreateConstantBuffer<T>(int count, ShaderStage stage, int index) where T : unmanaged
        {
            var constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(T) * count, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constants.Add(new(constantBuffer, new ShaderBinding(stage, index)));
            return constantBuffer;
        }

        public virtual void Dispose()
        {
            Reload -= OnReload;

            foreach (var constant in constants)
                constant.Constant.Dispose();
            foreach (var resource in resources)
                resource.Resource.Dispose();
            foreach (var sampler in samplers)
                sampler.Sampler.Dispose();

            if (vs is not null)
                vs.Dispose();
            if (hs is not null)
                hs.Dispose();
            if (ds is not null)
                ds.Dispose();
            if (gs is not null)
                gs.Dispose();
            if (ps is not null)
                ps.Dispose();
            if (layout is not null)
                layout.Dispose();
            if (rasterizerState is not null)
                rasterizerState.Dispose();
            if (depthStencilState is not null)
                depthStencilState.Dispose();
            if (blendState is not null)
                blendState.Dispose();
        }
    }
}