namespace HexaEngine.D3D12
{
    using Silk.NET.Core.Native;
    using Silk.NET.Direct3D.Compilers;
    using Silk.NET.Direct3D12;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class D3D12GraphicsPipeline
    {
        private ComPtr<ID3D12PipelineState> pso;

        public D3D12GraphicsPipeline(ComPtr<ID3D12Device10> device)
        {
            GraphicsPipelineStateDesc desc;
        }

        private void Compile()
        {
        }
    }
}