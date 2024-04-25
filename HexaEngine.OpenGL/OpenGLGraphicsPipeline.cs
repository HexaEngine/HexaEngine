namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using System;

    public class OpenGLGraphicsPipeline : IGraphicsPipeline
    {
        private readonly OpenGLGraphicsDevice device;
        private readonly string dbgName;
        private bool disposedValue;
        protected InputElementDescription[]? inputElements;
        protected readonly GraphicsPipelineDesc desc;
        protected ShaderMacro[]? macros;
        protected uint program;
        protected GraphicsPipelineStateDesc state = GraphicsPipelineStateDesc.Default;

        protected bool valid;
        protected volatile bool initialized;

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            Compile();
            initialized = true;
        }

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.state = state;
            Compile();
            initialized = true;
        }

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, InputElementDescription[] inputElements, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.state = state;
            this.inputElements = inputElements;
            Compile();
            initialized = true;
        }

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, InputElementDescription[] inputElements, ShaderMacro[] macros, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.state = state;
            this.inputElements = inputElements;
            this.macros = macros;
            Compile();
            initialized = true;
        }

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.inputElements = inputElements;
            Compile();
            initialized = true;
        }

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, InputElementDescription[] inputElements, ShaderMacro[] macros, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.inputElements = inputElements;
            this.macros = macros;
            Compile();
            initialized = true;
        }

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, GraphicsPipelineStateDesc state, ShaderMacro[] macros, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.state = state;
            this.macros = macros;
            Compile();
            initialized = true;
        }

        public OpenGLGraphicsPipeline(OpenGLGraphicsDevice device, GraphicsPipelineDesc desc, ShaderMacro[] macros, string dbgName)
        {
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            this.macros = macros;
            Compile();
            initialized = true;
        }

        public GraphicsPipelineDesc Description => desc;

        public string DebugName => dbgName;

        public GraphicsPipelineStateDesc State
        {
            get => state;
            set => throw new NotImplementedException();
        }

        public ShaderMacro[]? Macros
        {
            get => macros;
            set => macros = value;
        }

        public bool IsInitialized => initialized;

        public bool IsValid => valid;

        public void BeginDraw(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            if (!valid)
            {
                return;
            }

            var ctx = (OpenGLGraphicsContext)context;
            ctx.SetRasterizerState(state.Rasterizer);
            ctx.SetDepthStencilState(state.DepthStencil, state.StencilRef);
            ctx.SetBlendState(state.Blend, state.BlendFactor, state.SampleMask);
            ctx.SetPrimitiveTopology(state.Topology);
            device.GL.UseProgram(program);
        }

        public void EndDraw(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            if (!valid)
            {
                return;
            }

            var ctx = (OpenGLGraphicsContext)context;
            device.GL.UseProgram(0);

            ctx.SetRasterizerState(default);
            ctx.SetDepthStencilState(default, default);
            ctx.SetBlendState(default, default, default);
            ctx.SetPrimitiveTopology(default);
        }

        public void Recompile()
        {
            initialized = false;

            if (program != 0)
            {
                device.GL.DeleteProgram(program);
            }

            program = 0;

            Compile(true);

            initialized = true;
        }

        private void Compile(bool bypassCache = false)
        {
        }

        protected virtual ShaderMacro[] GetShaderMacros()
        {
            if (macros == null)
            {
                return Array.Empty<ShaderMacro>();
            }
            return macros;
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                PipelineManager.Unregister(this);

                if (program != 0)
                {
                    device.GL.DeleteProgram(program);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}