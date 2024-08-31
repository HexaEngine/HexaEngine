namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D11;

    public unsafe class D3D11ComputePipelineState : D3D11PipelineState, IComputePipelineState
    {
        private readonly D3D11ComputePipeline pipeline;
        private readonly D3D11ResourceBindingList resourceBindingList;
        private readonly string dbgName;

        internal ComPtr<ID3D11ComputeShader> cs;

        public D3D11ComputePipelineState(D3D11ComputePipeline pipeline, string dbgName = "")
        {
            pipeline.AddRef();
            this.pipeline = pipeline;
            this.dbgName = dbgName;
            PipelineStateManager.Register(this);

            resourceBindingList = new(pipeline);

            pipeline.OnCompile += OnPipelineCompile;
            cs = pipeline.cs;
        }

        private void OnPipelineCompile(IPipeline pipe)
        {
            D3D11ComputePipeline pipeline = (D3D11ComputePipeline)pipe;
            cs = pipeline.cs;
        }

        public IComputePipeline Pipeline => pipeline;

        public IResourceBindingList Bindings => resourceBindingList;

        public bool IsValid => pipeline.IsValid;

        public bool IsInitialized => pipeline.IsInitialized;

        public string DebugName => dbgName;

        internal override void SetState(ComPtr<ID3D11DeviceContext3> context)
        {
            context.CSSetShader(cs, null, 0);

            resourceBindingList.BindCompute(context);
        }

        internal override void UnsetState(ComPtr<ID3D11DeviceContext3> context)
        {
            context.CSSetShader((ID3D11ComputeShader*)null, null, 0);

            resourceBindingList.UnbindCompute(context);
        }

        protected override void DisposeCore()
        {
            PipelineStateManager.Unregister(this);

            pipeline.OnCompile -= OnPipelineCompile;
            pipeline.Dispose();

            resourceBindingList.Dispose();
        }
    }
}