namespace HexaEngine.Dummy
{
    using HexaEngine.Core.Graphics;
    using System;

    public class DummyComputePipeline : DisposableBase, IComputePipeline
    {
        public DummyComputePipeline(ComputePipelineDesc desc)
        {
            Desc = desc;
        }

        public string DebugName { get; }

        public ShaderMacro[]? Macros { get; set; }

        public ComputePipelineDesc Desc { get; }

        public bool IsInitialized { get; }

        public bool IsValid { get; }

        public event Action<IPipeline>? OnCompile;

        public void Recompile()
        {
        }

        protected override void DisposeCore()
        {
        }
    }
}