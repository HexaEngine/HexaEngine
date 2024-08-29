namespace HexaEngine.OpenGL
{
    using HexaEngine.Core.Graphics;
    using System;

    public unsafe class OpenGLComputePipeline : IComputePipeline
    {
        private readonly OpenGLGraphicsDevice device;
        private readonly string dbgName;
        private bool valid;
        private bool initialized;
        private uint program;
        private readonly ComputePipelineDesc desc;
        private ShaderMacro[]? macros;
        private bool disposedValue;

        public event Action<IPipeline>? OnCompile;

        public OpenGLComputePipeline(OpenGLGraphicsDevice device, ComputePipelineDesc desc, string dbgName)
        {
            PipelineManager.Register(this);
            this.device = device;
            this.dbgName = dbgName;
            this.desc = desc;
            Compile();
            initialized = true;
        }

        public OpenGLComputePipeline(OpenGLGraphicsDevice device, ComputePipelineDesc desc, ShaderMacro[] macros, string dbgName)
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

        public ShaderMacro[]? Macros { get => macros; set => macros = value; }

        public ComputePipelineDesc Desc => desc;

        public bool IsInitialized => initialized;

        public bool IsValid => valid;

        public void Recompile()
        {
            initialized = false;

            if (program != 0)
            {
                OpenGLGraphicsDevice.GL.DeleteProgram(program);
            }

            program = 0;

            Compile(true);

            initialized = true;
        }

        private void Compile(bool bypassCache = false)
        {
            var macros = GetShaderMacros();

            if (desc.Path != null)
            {
                device.ShaderCompiler.GetProgramOrCompile(desc, macros, out program, bypassCache);
                valid = true;
            }
        }

        private ShaderMacro[] GetShaderMacros()
        {
            if (macros == null)
            {
                return Array.Empty<ShaderMacro>();
            }

            return macros;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                PipelineManager.Unregister(this);
                if (program != 0)
                {
                    OpenGLGraphicsDevice.GL.DeleteProgram(program);
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